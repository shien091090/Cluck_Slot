//音訊設定包
//※包含音訊剪輯(Clip)與音源(AudioSource)的設定
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudiosPack : MonoBehaviour
{
    public List<ClipUnit> clipsPack = new List<ClipUnit>(); //音訊剪輯設定
    public List<AudioSourceUnit> audioSourcesPack = new List<AudioSourceUnit>(); //音源設定

    public Dictionary<byte, List<AudioSourceUnit>> audioSourceDict = new Dictionary<byte, List<AudioSourceUnit>>(); //從SourceNum查找AudioSourceUnit清單的Dictionary
    public Dictionary<AudioSource, float> initialVolumeDict = new Dictionary<AudioSource, float>(); //從AudioSource查找初始音量的Dictionary
    public Dictionary<AudioSource, byte> audioSourceStateDict = new Dictionary<AudioSource, byte>(); //紀錄所有AudioSource的目前狀態(0 = 完全停止 / 1 = 撥放中 / 2 = 停止中)

    //--------------------------------------------------------------------------------------------------------------

    //彙整audioSourceDict
    public void SetAudioSourceDict()
    {
        if (audioSourceDict.Count != 0) audioSourceDict = new Dictionary<byte, List<AudioSourceUnit>>(); //初始化audioSourceDict

        List<byte> numList = new List<byte>(); //儲存所有出現過的AudioSourceUnit的SourceNum
        List<List<AudioSourceUnit>> allotTable = new List<List<AudioSourceUnit>>(); //將每個對應SourceNum的AudioSourceUnit按照順序編入List

        for (int i = 0; i < audioSourcesPack.Count; i++) //遍歷所有AudioSourceUnit
        {
            if (!initialVolumeDict.ContainsKey(audioSourcesPack[i].source)) initialVolumeDict.Add(audioSourcesPack[i].source, audioSourcesPack[i].source.volume); //紀錄音源的初始音量
            if (!audioSourceStateDict.ContainsKey(audioSourcesPack[i].source)) audioSourceStateDict.Add(audioSourcesPack[i].source, 0); //紀錄音源狀態

            if (numList.Count == 0) //第一個元素
            {
                numList.Add(audioSourcesPack[i].GetSourceNum);
                allotTable.Add(new List<AudioSourceUnit>());
                allotTable[0].Add(audioSourcesPack[i]);
                continue;
            }

            for (int j = 0; j < numList.Count; j++) //第二個元素後, 遍歷現有SourceNum尋找是否有無重複
            {
                if (numList[j] == audioSourcesPack[i].GetSourceNum) //若找到相同者, 將此項AudioSourceUnit加入對應的List
                {
                    allotTable[j].Add(audioSourcesPack[i]);
                    break;
                }

                if (j == numList.Count - 1) //遍歷到最後一項時, 視為新的SourceNum並註冊之
                {
                    numList.Add(audioSourcesPack[i].GetSourceNum);
                    allotTable.Add(new List<AudioSourceUnit>());
                    allotTable[allotTable.Count - 1].Add(audioSourcesPack[i]);
                    break;
                }
            }
        }

        //Debug.Log("-----Test-----");
        //Debug.Log("numList.Count = " + numList.Count);
        //Debug.Log("allotTable.Count = " + allotTable.Count);
        //for (int i = 0; i < allotTable.Count; i++)
        //{
        //    Debug.Log("allotTable[" + i + "].count = " + allotTable[i].Count);
        //}
        //Debug.Log("-----Test2-----");
        //for (int i = 0; i < numList.Count; i++)
        //{
        //    Debug.Log("Num [" + numList[i] + "]");
        //    for (int j = 0; j < allotTable[i].Count; j++)
        //    {
        //        Debug.Log("(" + allotTable[i][j].sourceNum + ")<" + j + "> " + allotTable[i][j].source.name);
        //    }
        //}

        for (int i = 0; i < allotTable.Count; i++) //移除同個SourceNum中指定重複音源
        {
            for (int j = 0; j < allotTable[i].Count; j++)
            {
                for (int k = j + 1; k < allotTable[i].Count; k++)
                {
                    if (allotTable[i][j].source == allotTable[i][k].source)
                    {
                        allotTable[i][k].source = null;
                        Debug.Log("[ERROR]在同個SourceNum[" + allotTable[i][j].GetSourceNum + "]指定重複音源!!");
                    }
                }
            }
        }

        for (int i = 0; i < allotTable.Count; i++) //將彙整好的AudioSourceUnit導入Dictionary中
        {
            audioSourceDict.Add(numList[i], allotTable[i]);
        }

        //Debug.Log("audioSourceDict.Count = " + audioSourceDict.Count);
        //for (int i = 0; i < numList.Count; i++)
        //{
        //    Debug.Log("Dictionary內容測試 ------- Num:" + numList[i]);
        //    for (int j = 0; j < audioSourceDict[numList[i]].Count; j++)
        //    {
        //        Debug.Log("第" + j + "個 [" + audioSourceDict[numList[i]][j].source.name + "] num:" + audioSourceDict[numList[i]][j].sourceNum);
        //    }
        //}
    }

    //指定ClipUnit改變AudioSourceNum
    //(多載1/2) name:音訊名稱 / num:轉換編號
    //(多載2/2) tag:標籤 / num:轉換編號
    public void ChangeClipSource(string name, byte num) { _changeClipSource(name, null, num); }
    public void ChangeClipSource(string[] tag, byte num) { _changeClipSource(null, tag, num); }
    private void _changeClipSource(string name, string[] tag, byte num)
    {
        if (name == null && tag == null) return;

        List<ClipUnit> clipUnitQueue = new List<ClipUnit>(); //待轉換的ClipUnit佇列

        for (int i = 0; i < clipsPack.Count; i++)
        {
            if (clipsPack[i].GetName == name) //從名稱搜尋
            {
                clipUnitQueue.Add(clipsPack[i]);
                continue;
            }

            if (tag == null) continue;
            if (tag.Length == 0) continue;

            for (int j = 0; j < clipsPack[i].GetTag.Count; j++) //從標籤搜尋
            {
                if (clipUnitQueue.Contains(clipsPack[i])) break;
                for (int k = 0; k < tag.Length; k++)
                {
                    if (clipsPack[i].GetTag[j] == tag[k])
                    {
                        clipUnitQueue.Add(clipsPack[i]);
                        break;
                    }
                }
            }
        }

        if (clipUnitQueue.Count == 0) return; //查無結果時中止程序

        //Debug.Log("clipUnitQueue.Count = " + clipUnitQueue.Count);
        //for (int i = 0; i < clipUnitQueue.Count; i++)
        //{
        //    Debug.Log("名稱:" + clipUnitQueue[i].name);
        //    string tagLabel = "";
        //    for (int j = 0; j < clipUnitQueue[i].tag.Count; j++)
        //    {
        //        tagLabel += clipUnitQueue[i].tag[j] + "/";
        //    }
        //    Debug.Log("Tag:" + tagLabel);
        //    Debug.Log("音源編號:" + clipUnitQueue[i].outputSourceNum);
        //}

        for (int i = 0; i < clipUnitQueue.Count; i++) //轉換佇列中所有ClipUnit的音源編號
        {
            clipUnitQueue[i].SetOutputSourceNum = num;
        }
    }

    //設定ClipUnit的Tag
    public void SetClipTag(string name, string[] tag)
    {
        if (name == null) return;

        for (int i = 0; i < clipsPack.Count; i++)
        {
            if (clipsPack[i].GetName == name)
            {
                clipsPack[i].SetTag(tag);
            }
        }
    }

    //設定淡入淡出參數
    //多載(1/3) num:音源編號 / fadeInEffectMode:淡入模式 / fadeOutEffectMode:淡出模式
    //多載(2/3) num:音源編號 / fadeInEffectMode:淡入模式 / dur_fadeIn:淡入作動時間 / fadeOutEffectMode:淡出模式 / dur_fadeOut:淡出作動時間
    //多載(3/3) num:音源編號 / fastRate:聲調漸快幅度百分比 / lowerRate:聲調漸慢幅度百分比
    public void SetFadeParam(byte num, AudioEffectMode fadeInEffectMode, AudioEffectMode fadeOutEffectMode) { _SetFadeParam(num, fadeInEffectMode, -1, fadeOutEffectMode, -1); }
    public void SetFadeParam(byte num, AudioEffectMode fadeInEffectMode, float dur_fadeIn, AudioEffectMode fadeOutEffectMode, float dur_fadeOut) { _SetFadeParam(num, fadeInEffectMode, dur_fadeIn, fadeOutEffectMode, dur_fadeOut); }
    public void SetFadeParam(byte num, float fastRate, float lowerRate)
    {
        for (int i = 0; i < audioSourcesPack.Count; i++)
        {
            if (audioSourcesPack[i].GetSourceNum == num)
            {
                audioSourcesPack[i].SetFadeParam(fastRate, lowerRate);
            }
        }
    }
    private void _SetFadeParam(byte num, AudioEffectMode fadeInEffectMode, float dur_fadeIn, AudioEffectMode fadeOutEffectMode, float dur_fadeOut)
    {
        for (int i = 0; i < audioSourcesPack.Count; i++)
        {
            if (audioSourcesPack[i].GetSourceNum == num)
            {
                audioSourcesPack[i].SetFadeParam(fadeInEffectMode, fadeOutEffectMode);
                if (dur_fadeIn != -1) audioSourcesPack[i].SetFadeInDuration = dur_fadeIn;
                if (dur_fadeOut != -1) audioSourcesPack[i].SetFadeOutDuration = dur_fadeOut;
            }
        }
    }

    //設定撥放方法
    public void SetSourceOutputMethod(byte num, OutputMethod method)
    {
        for (int i = 0; i < audioSourcesPack.Count; i++)
        {
            if (audioSourcesPack[i].GetSourceNum == num)
            {
                audioSourcesPack[i].SetOutputMethod = method;
            }
        }
    }

    //取得AudioSourceUnit
    public AudioSourceUnit GetAudioSourceUnit(string name)
    {
        List<AudioSourceUnit> unit = new List<AudioSourceUnit>();

        for (int i = 0; i < audioSourcesPack.Count; i++)
        {
            if (audioSourcesPack[i].GetName == name) unit.Add(audioSourcesPack[i]);
        }

        if (unit.Count > 1) //若搜尋結果超過1個, 則返回錯誤
        {
            Debug.Log("[ERROR]返回結果不可為1個以上");
            return null;
        }

        return unit[0];
    }
}
