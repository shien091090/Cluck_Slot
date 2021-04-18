//音訊處理自定義類別
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Enum

public enum AudioEffectMode //特殊音訊效果
{
    無, 音量淡化, 音調漸快, 音調漸慢, 音調漸快且淡化, 音調漸慢且淡化
}

public enum OutputMethod //輸出方法
{
    Play, OneShot
}

public enum OverlappingMode //重疊撥放設定
{
    擇一撥放, 同時重疊撥放
}

//========================================================================================================================

[System.Serializable]
public class ClipUnit //管理音訊剪輯的列表
{
    [SerializeField]
    private string name; //名稱
    public string GetName { get { return name; } } //取得名稱

    //----------------------------------------------------------

    [SerializeField]
    private List<AudioClip> clips = new List<AudioClip>(); //撥放剪輯
    public List<AudioClip> GetClips { get { return clips; } } //取得撥放剪輯

    //----------------------------------------------------------

    [SerializeField]
    private List<string> tag = new List<string>(); //標籤(可複數)
    public List<string> GetTag { get { return tag; } } //取得標籤

    //----------------------------------------------------------

    [SerializeField]
    [Range(0.0f, 2.0f)]
    private float volumeScale = 1.0f; //音量輸出率, 用以微調音效的各自音量(會再和AudioManagerCore.setVolume加算)
    public float GetVolumeScale { get { return volumeScale; } } //取得音量輸出率

    //----------------------------------------------------------

    [SerializeField]
    private OverlappingMode overlappingMode; //重疊撥放設定(如果一個ClipUnit同時設定多個Clip, 可以選擇撥放時抽出一個撥放還是重複同時撥放, 然而在同一個音源且為Play模式時, 無法重複同時撥放)
    public OverlappingMode GetOverlappingMode { get { return overlappingMode; } } //取得重複撥放設定

    //----------------------------------------------------------

    [SerializeField]
    private byte outputSourceNum; //使用的音源標號
    public byte GetOutputSourceNum { get { return outputSourceNum; } } //取得音源編號
    public byte SetOutputSourceNum { set { outputSourceNum = (byte)Mathf.Clamp(value, 0, 254); } } //設定音源編號(不可超過254)

    //--------------------------------------------------------------------------------------------------------------

    public void SetTag(string[] tagStr) //設定標籤
    {
        string[] tmp_tag = tagStr;

        if (tmp_tag == null) tmp_tag = new string[] { "" }; //以空字串代替null
        if (tmp_tag.Length == 0) tmp_tag = new string[] { "" }; //以空字串代替空內容
        tag = new List<string>();
        tag.AddRange(tmp_tag);
    }

}

//========================================================================================================================

[System.Serializable]
public class AudioSourceUnit //管理音源的列表
{
    [SerializeField]
    private string name; //名稱
    public string GetName { get { return name; } } //取得名稱

    //--------------------------------------------------------------------------------------------------------------

    public AudioSource source;

    //--------------------------------------------------------------------------------------------------------------

    [SerializeField]
    private byte sourceNum; //編號
    public byte GetSourceNum { get { return sourceNum; } } //取得音源編號

    //--------------------------------------------------------------------------------------------------------------

    [SerializeField]
    private List<string> tag = new List<string>(); //標籤(可複數)
    public List<string> GetTag { get { return tag; } } //取得標籤

    //--------------------------------------------------------------------------------------------------------------

    [SerializeField]
    private AudioEffectMode fadeInEffectMode = AudioEffectMode.無; //淡入效果:預設為一般
    public AudioEffectMode GetFadeInEffectMode { get { return fadeInEffectMode; } } //取得淡入模式

    //--------------------------------------------------------------------------------------------------------------

    [SerializeField]
    private AudioEffectMode fadeOutEffectMode = AudioEffectMode.無; //淡出效果:預設為一般
    public AudioEffectMode GetFadeOutEffectMode { get { return fadeOutEffectMode; } } //取得淡出模式

    //--------------------------------------------------------------------------------------------------------------

    [SerializeField]
    private OutputMethod outputMethod = OutputMethod.Play; //撥放方法:預設為Play(不支援複數Clip)
    public OutputMethod GetOutputMethod { get { return outputMethod; } } //取得撥放方法
    public OutputMethod SetOutputMethod //設定撥放方法
    {
        set
        {
            if (value == OutputMethod.OneShot) fadeInEffectMode = AudioEffectMode.無;
            outputMethod = value;
        }
    }

    //--------------------------------------------------------------------------------------------------------------

    [SerializeField]
    private float iniParam_becomeFastRate = 0.5f; //(淡入參數)聲調漸快幅度百分比 Ex:0.2f(20%) > Pitch基值1.0淡入時0.8→1.0;淡出時1.0→1.2 或是Pitch基值1.2淡入時0.96→1.2;淡出時1.2→1.44
    public float GetFastRate { get { return iniParam_becomeFastRate; } } //取得聲調漸快幅度百分比

    //--------------------------------------------------------------------------------------------------------------

    [SerializeField]
    private float iniParam_becomeLowerRate = 0.35f; //(淡入參數)聲調漸慢幅度百分比
    public float GetLowerRate { get { return iniParam_becomeLowerRate; } } //取得聲調漸慢幅度百分比

    //--------------------------------------------------------------------------------------------------------------

    [SerializeField]
    private float duration_fadeIn = 0.5f; //(淡入參數)淡入作動時間

    [SerializeField]
    private float duration_fadeOut = 0.5f; //(淡入參數)淡出作動時間
    public float[] GetFadeDuration { get { return new float[] { duration_fadeIn, duration_fadeOut }; } } //取得淡入淡出作動時間([0]=淡入作動時間/[1]=淡出作動時間)

    //--------------------------------------------------------------------------------------------------------------

    public float SetFadeInDuration //設定淡入作動時間
    {
        set
        {
            duration_fadeIn = value < 0.0f ? 0.0f : value;
        }
    }
    public float SetFadeOutDuration //設定淡入作動時間
    {
        set
        {
            duration_fadeOut = value < 0.0f ? 0.0f : value;
        }
    }

    //--------------------------------------------------------------------------------------------------------------

    //設定淡入參數
    //(多載1/2) fadeInMode:淡入模式 / fadeOutMode:淡出模式
    public void SetFadeParam(AudioEffectMode fadeInMode, AudioEffectMode fadeOutMode)
    {
        fadeInEffectMode = fadeInMode;
        fadeOutEffectMode = fadeOutMode;
    }

    //(多載2/2) becomeFastRate:聲調漸快幅度百分比 / becomeLowerRate:聲調漸慢幅度百分比
    public void SetFadeParam(float becomeFastRate, float becomeLowerRate)
    {
        iniParam_becomeFastRate = Mathf.Clamp(becomeFastRate, 0.0f, 1.0f);
        iniParam_becomeLowerRate = Mathf.Clamp(becomeLowerRate, 0.0f, 1.0f);
    }


}