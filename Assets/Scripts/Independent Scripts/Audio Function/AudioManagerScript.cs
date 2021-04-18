//音訊控制腳本
//※掛載在空物件上即可
//※掛載時自動建立AudiosPack Class, 音訊剪輯(Clip)與音源(AudioSource)在Hierarchy上的AudiosPack設定
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudiosPack))]
public class AudioManagerScript : MonoBehaviour
{
    private static AudioManagerScript _instance; //單例模式
    public static AudioManagerScript Instance
    {
        get { return _instance; }
    }

    public AudioListener audioListener; //聆聽器
    private AudiosPack audiosPack; //音訊設定包

    //--------------------------------------------------------------------------------------------------------------

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this; //設定單例
        }
        else
        {
            Destroy(this.gameObject);
        }

        audiosPack = this.GetComponent<AudiosPack>();
        audiosPack.SetAudioSourceDict(); //初始化字典

        DontDestroyOnLoad(this); //跨場景物件
    }

    //--------------------------------------------------------------------------------------------------------------

    //簡易撥放
    //(多載1/2) clip:音訊剪輯
    //(多載2/2) clip:音訊剪輯 / v:音量 [強制指定音量]
    public void SimplePlay(AudioClip clip) { _simplePlay(clip, 1); }
    public void SimplePlay(AudioClip clip, float v) { _simplePlay(clip, v); }
    private void _simplePlay(AudioClip clip, float v)
    {
        AudioSource.PlayClipAtPoint(clip, audioListener.transform.localPosition, v);
    }

    //從AudiosPack抓取對應音訊剪輯並撥放
    //(多載1/2) name:名稱
    //(多載2/2) tag:標籤
    public void PlayAudioClip(string name) { _playAudioClip(name, null, false); }
    public void PlayAudioClip(string[] tag) { _playAudioClip(null, tag, false); }
    private void _playAudioClip(string name, string[] tag, bool canCover)
    {
        if (name == null && tag.Length == 0) return; //無指定直接返回

        List<ClipUnit> clipsQueue = new List<ClipUnit>(); //待撥放佇列

        for (int i = 0; i < audiosPack.clipsPack.Count; i++) //符合條件的ClipUnit加入待撥放佇列
        {
            if (name == audiosPack.clipsPack[i].GetName) //從名字搜尋
            {
                clipsQueue.Add(audiosPack.clipsPack[i]);
                continue;
            }

            if (tag == null) continue; //無tag時跳過
            for (int j = 0; j < tag.Length; j++) //從Tag搜尋
            {
                for (int k = 0; k < audiosPack.clipsPack[i].GetTag.Count; k++)
                {
                    if (tag[j] == audiosPack.clipsPack[i].GetTag[k])
                    {
                        clipsQueue.Add(audiosPack.clipsPack[i]);
                    }
                }
            }
        }

        List<AudioSource> doneSourceList = new List<AudioSource>(); //經手音源(防止音源衝突)
        for (int i = 0; i < clipsQueue.Count; i++) //逐一撥放
        {
            if (clipsQueue[i].GetClips == null || clipsQueue[i].GetClips.Count == 0) //音訊剪輯為空或是無指定的狀況時跳過
            {
                Debug.Log("[ERROR]未指定Clip");
                continue;
            }

            if (clipsQueue[i].GetClips == null || clipsQueue[i].GetClips.Count == 0) //音訊剪輯為空或是無指定的狀況時跳過
            {
                Debug.Log("[ERROR]未指定Clip");
                continue;
            }

            if (!audiosPack.audioSourceDict.ContainsKey(clipsQueue[i].GetOutputSourceNum)) //若音源編號找不到則跳過
            {
                Debug.Log("[ERROR]未指定音源");
                continue;
            }

            List<AudioClip> ouputClips = new List<AudioClip>(); //篩選欲輸出音訊
            switch (clipsQueue[i].GetOverlappingMode)
            {
                case OverlappingMode.擇一撥放:
                    int _dice = Random.Range(0, clipsQueue[i].GetClips.Count);
                    AudioClip _clip = clipsQueue[i].GetClips[_dice]; //隨機取得一個Clip
                    ouputClips.Add(_clip);
                    break;

                case OverlappingMode.同時重疊撥放:
                    ouputClips = clipsQueue[i].GetClips;
                    break;
            }

            List<AudioSourceUnit> audioSourceQueue = audiosPack.audioSourceDict[clipsQueue[i].GetOutputSourceNum]; //音源佇列
            for (int j = 0; j < audioSourceQueue.Count; j++)
            {
                if (doneSourceList.Contains(audioSourceQueue[j].source) && audioSourceQueue[j].GetOutputMethod == OutputMethod.Play)
                {
                    Debug.Log("[ERROR]音源名稱 " + audioSourceQueue[j].source.name + " 因設為Play模式, 有音訊無法順利撥放");
                }

                doneSourceList.Add(audioSourceQueue[j].source);
                StartCoroutine(Corou_PlayAudioClip(ouputClips, audioSourceQueue[j], clipsQueue[i].GetVolumeScale, canCover));
            }
        }
    }

    //覆蓋撥放
    //(多載1/2) name:名稱
    //(多載2/2) tag:標籤
    public void CoverPlayAudioClip(string name) { _playAudioClip(name, null, true); }
    public void CoverPlayAudioClip(string[] tag) { _playAudioClip(null, tag, true); }

    //停止撥放
    //(多載1/2) sourceNum:音源編號
    //(多載2/2) tag:音源標籤
    public void Stop(byte sourceNum) { _Stop(sourceNum, null); }
    public void Stop(string[] tag) { _Stop(255, tag); }
    private void _Stop(byte sourceNum, string[] tag)
    {
        List<AudioSourceUnit> sourceQueue = FilterAudioSources(sourceNum, tag);

        for (int i = 0; i < sourceQueue.Count; i++) //對音源佇列逐一發出停止命令
        {
            StartCoroutine(Corou_StopPlaying(sourceQueue[i]));
        }
    }

    //暫停撥放
    //多載1/2 sourceNum : 音源編號
    //多載2/2 tag : 音源標籤
    public void SetPause(byte sourceNum) { _SetPause(sourceNum, null); }
    public void SetPause(string[] tag) { _SetPause(255, tag); }
    private void _SetPause(byte sourceNum, string[] tag)
    {
        List<AudioSourceUnit> sourceQueue = FilterAudioSources(sourceNum, tag);

        for (int i = 0; i < sourceQueue.Count; i++) //對音源佇列逐一發出暫停命令
        {
            if (sourceQueue[i].source.isPlaying) sourceQueue[i].source.Pause();
            else sourceQueue[i].source.UnPause();

        }
    }

    //設定音量
    //(多載1/2) num:音源編號 / v:音量
    //(多載2/2) v:音量 / tag:音源標籤
    public void SetVolume(byte num, float v) { _setvolume(num, null, v); }
    public void SetVolume(string[] tag, float v) { _setvolume(255, tag, v); }
    private void _setvolume(byte num, string[] tag, float v)
    {
        List<AudioSourceUnit> sourceList = new List<AudioSourceUnit>(); //目標AudioSource佇列
        sourceList = FilterAudioSources(num, tag);

        for (int i = 0; i < sourceList.Count; i++)
        {
            audiosPack.initialVolumeDict[sourceList[i].source] = v; //重新設定AudioSource的初始音量
            sourceList[i].source.volume = v; //設定AudioSource的現在音量
        }
    }

    //設定靜音狀態
    public void SetMuteState(byte num, bool state) { _SetMuteState(num, null, state); }
    public void SetMuteState(string[] tag, bool state) { _SetMuteState(255, tag, state); }

    private void _SetMuteState(byte num, string[] tag, bool state)
    {
        List<AudioSourceUnit> sourceList = new List<AudioSourceUnit>(); //目標AudioSource佇列
        sourceList = FilterAudioSources(num, tag);

        for (int i = 0; i < sourceList.Count; i++)
        {
            sourceList[i].source.mute = state; //設定音源靜音狀態
        }
    }

    //音源篩選
    private List<AudioSourceUnit> FilterAudioSources(byte sourceNum, string[] tag)
    {
        List<AudioSourceUnit> sourceQueue = new List<AudioSourceUnit>();

        if (audiosPack.audioSourceDict.ContainsKey(sourceNum) && sourceNum != 255) //從音源編號搜尋
        {
            for (int i = 0; i < audiosPack.audioSourceDict[sourceNum].Count; i++)
            {
                sourceQueue.Add(audiosPack.audioSourceDict[sourceNum][i]);
            }
        }

        if (tag != null && tag.Length > 0)
        {
            for (int i = 0; i < audiosPack.audioSourcesPack.Count; i++) //從Tag搜尋
            {
                for (int j = 0; j < tag.Length; i++)
                {
                    for (int k = 0; k < audiosPack.audioSourcesPack[i].GetTag.Count; k++)
                    {
                        if (tag[j] == audiosPack.audioSourcesPack[i].GetTag[k])
                        {
                            sourceQueue.Add(audiosPack.audioSourcesPack[i]);
                        }
                    }
                }
            }
        }

        //挑調重複的音源
        for (int i = 0; i < sourceQueue.Count; i++)
        {
            for (int j = i + 1; j < sourceQueue.Count; j++)
            {
                if (sourceQueue[i].source == sourceQueue[j].source) sourceQueue[j].source = null;
            }
        }
        sourceQueue.RemoveAll((AudioSourceUnit u) => { return u.source == null; });

        return sourceQueue;
    }

    //撥放音訊
    private IEnumerator Corou_PlayAudioClip(List<AudioClip> clips, AudioSourceUnit audioSourceUnit, float volumeScale, bool canCover)
    {
        if (audioSourceUnit.source == null)
        {
            Debug.Log("[ERROR]空的AudioSource");
            yield break;
        }

        if (audioSourceUnit.GetOutputMethod == OutputMethod.OneShot) //OneShot模式可以同時撥放複數Clip, 也不會有因為指定同個AudioSource而導致撥放衝突的問題
        {
            if (audiosPack.audioSourceStateDict[audioSourceUnit.source] == 2) yield break; //即使是OneShot, 若AudioSource狀態為2(停止中)則必須等完全停止才可撥放
            audioSourceUnit.source.pitch = 1.0f; //音調初始化
            audioSourceUnit.source.volume = audiosPack.initialVolumeDict[audioSourceUnit.source]; //音量初始化
            for (int i = 0; i < clips.Count; i++)
            {
                audioSourceUnit.source.PlayOneShot(clips[i], volumeScale);
            }
            yield break;
        }

        if (clips.Count > 1) //Play模式不可撥放複數Clip
        {
            Debug.Log("[ERROR]Play模式不可使用複數Clip");
            yield break;
        }

        if (!canCover && audiosPack.audioSourceStateDict[audioSourceUnit.source] != 0) yield break; //音源尚未撥放完畢, 無法再度撥放
        if (canCover && audiosPack.audioSourceStateDict[audioSourceUnit.source] != 0) //覆蓋撥放
        {
            audioSourceUnit.source.Stop();
            yield return new WaitUntil(() => audiosPack.audioSourceStateDict[audioSourceUnit.source] == 0);
        }

        audiosPack.audioSourceStateDict[audioSourceUnit.source] = 1; //狀態=撥放中
        audioSourceUnit.source.pitch = 1.0f; //音調初始化
        audioSourceUnit.source.volume = audiosPack.initialVolumeDict[audioSourceUnit.source]; //音量初始化
        audioSourceUnit.source.clip = clips[0];
        audioSourceUnit.source.volume = Mathf.Clamp(audioSourceUnit.source.volume * volumeScale, 0.0f, 1.0f); //音量調整

        audioSourceUnit.source.Play();

        float t = 0;
        float volumeLimit = audioSourceUnit.source.volume; //音量淡入時的目標音量

        float basePitch = new float(); //音調基數
        if (audioSourceUnit.GetFadeInEffectMode == AudioEffectMode.音調漸快 || audioSourceUnit.GetFadeInEffectMode == AudioEffectMode.音調漸快且淡化) basePitch = 1.0f * ( 1.0f - audioSourceUnit.GetFastRate );
        if (audioSourceUnit.GetFadeInEffectMode == AudioEffectMode.音調漸慢 || audioSourceUnit.GetFadeInEffectMode == AudioEffectMode.音調漸慢且淡化) basePitch = 1.0f * ( 1.0f + audioSourceUnit.GetLowerRate );

        while (t < audioSourceUnit.GetFadeDuration[0] && audioSourceUnit.source.isPlaying && audioSourceUnit.GetFadeInEffectMode != AudioEffectMode.無 && audiosPack.audioSourceStateDict[audioSourceUnit.source] != 2)
        {
            if (audioSourceUnit.GetFadeInEffectMode == AudioEffectMode.音量淡化 || audioSourceUnit.GetFadeInEffectMode == AudioEffectMode.音調漸快且淡化 || audioSourceUnit.GetFadeInEffectMode == AudioEffectMode.音調漸慢且淡化)
            {
                audioSourceUnit.source.volume = volumeLimit * ( t / audioSourceUnit.GetFadeDuration[0] );
            }

            if (audioSourceUnit.GetFadeInEffectMode == AudioEffectMode.音調漸快 || audioSourceUnit.GetFadeInEffectMode == AudioEffectMode.音調漸快且淡化)
            {
                audioSourceUnit.source.pitch = basePitch + ( ( 1.0f - basePitch ) * ( t / audioSourceUnit.GetFadeDuration[0] ) );
            }

            if (audioSourceUnit.GetFadeInEffectMode == AudioEffectMode.音調漸慢 || audioSourceUnit.GetFadeInEffectMode == AudioEffectMode.音調漸慢且淡化)
            {
                audioSourceUnit.source.pitch = basePitch - ( ( basePitch - 1.0f ) * ( t / audioSourceUnit.GetFadeDuration[0] ) );
            }

            t += Time.deltaTime;
            if (t >= audioSourceUnit.GetFadeDuration[0]) //效果結束, 設定為目標值
            {
                audioSourceUnit.source.volume = volumeLimit;
                audioSourceUnit.source.pitch = 1.0f;
            }

            yield return new WaitForEndOfFrame();
        }

        yield return new WaitUntil(() => !audioSourceUnit.source.isPlaying); //待音源停止
        audiosPack.audioSourceStateDict[audioSourceUnit.source] = 0; //狀態=完全停止
    }

    //停止撥放的特殊效果
    private IEnumerator Corou_StopPlaying(AudioSourceUnit audioSourceUnit)
    {
        if (!audioSourceUnit.source.isPlaying || audiosPack.audioSourceStateDict[audioSourceUnit.source] == 2) yield break; //若已經停止, 或是正在停止中, 則中止程序
        audiosPack.audioSourceStateDict[audioSourceUnit.source] = 2; //狀態=停止中

        AudioSource source = audioSourceUnit.source;

        if (audioSourceUnit.GetFadeOutEffectMode == AudioEffectMode.無) //無特殊效果則直接停止
        {
            source.Stop();
            yield return new WaitUntil(() => !audioSourceUnit.source.isPlaying); //待音源停止
            audiosPack.audioSourceStateDict[audioSourceUnit.source] = 0; //狀態=完全靜止
            yield break;
        }

        float t = 0;
        float iniVolume = audioSourceUnit.source.volume;
        float iniPitch = audioSourceUnit.source.pitch;
        while (t < audioSourceUnit.GetFadeDuration[1] && audioSourceUnit.source.isPlaying)
        {
            if (audioSourceUnit.GetFadeOutEffectMode == AudioEffectMode.音量淡化 || audioSourceUnit.GetFadeOutEffectMode == AudioEffectMode.音調漸快且淡化 || audioSourceUnit.GetFadeOutEffectMode == AudioEffectMode.音調漸慢且淡化)
            {
                audioSourceUnit.source.volume = iniVolume - ( iniVolume * ( t / audioSourceUnit.GetFadeDuration[1] ) );
            }

            if (audioSourceUnit.GetFadeOutEffectMode == AudioEffectMode.音調漸快 || audioSourceUnit.GetFadeOutEffectMode == AudioEffectMode.音調漸快且淡化)
            {
                audioSourceUnit.source.pitch = iniPitch + ( ( ( audioSourceUnit.GetFastRate + 1.0f ) - iniPitch ) * ( t / audioSourceUnit.GetFadeDuration[1] ) );
            }

            if (audioSourceUnit.GetFadeOutEffectMode == AudioEffectMode.音調漸慢 || audioSourceUnit.GetFadeOutEffectMode == AudioEffectMode.音調漸慢且淡化)
            {
                audioSourceUnit.source.pitch = iniPitch - ( ( iniPitch - ( 1.0f - ( audioSourceUnit.GetLowerRate ) ) ) * ( t / audioSourceUnit.GetFadeDuration[1] ) );
            }

            t += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        source.Stop();
        yield return new WaitUntil(() => !audioSourceUnit.source.isPlaying); //待音源停止
        audiosPack.audioSourceStateDict[audioSourceUnit.source] = 0; //狀態=完全靜止
    }
}

