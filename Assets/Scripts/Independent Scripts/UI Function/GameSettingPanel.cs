//遊戲設定介面
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GameSettingPanel : MonoBehaviour
{
    [Header("參考物件")]
    public GameObject visualWindow; //顯示視窗物件
    public Sprite[] muteStateSprite; //靜音狀態圖示(0=靜音 / 1=非靜音)
    public ConfirmWindow confirmWindow; //確認視窗腳本

    [Header("BGM音量設定")]
    public Slider bgmVolumeSlider; //BGM音量設定拉桿
    public Image bgmMuteImg; //BGM靜音狀態圖片

    [Header("SE音量設定")]
    public Slider seVolumeSlider; //SE音量設定拉桿
    public Image seMuteImg; //SE靜音狀態圖片

    [Header("全螢幕設定")]
    public GameObject[] isFullScreenStateGo; //全螢幕狀態物件(0=視窗模式 / 1=全螢幕模式)

    [Header("遊戲儲存值")]
    public PrefsSettingDataManager dataManager; //儲存值管理

    //---------------------------------------------------------------------------------------------------------------------------------------------

    void Start()
    {
        PrefsDataInitialize(); //初始化儲存值資料

        LoadComponentState(); //讀取儲存值設定
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------

    //初始化儲存值資料
    public void PrefsDataInitialize()
    {
        List<PrefsSettingData> prefsDataList = new List<PrefsSettingData>();

        //---------------------------------------------------------------------------------
        //若有增加設定項目, 在這邊設定PlayerPrefs相關參數
        //---------------------------------------------------------------------------------

        prefsDataList.Add(new PrefsSettingData("BGM_VOLUME", "0.7", PlayerPrefsDataType.Float));
        prefsDataList.Add(new PrefsSettingData("BGM_MUTE", "0", PlayerPrefsDataType.Int));

        prefsDataList.Add(new PrefsSettingData("SE_VOLUME", "0.7", PlayerPrefsDataType.Float));
        prefsDataList.Add(new PrefsSettingData("SE_MUTE", "0", PlayerPrefsDataType.Int));

        prefsDataList.Add(new PrefsSettingData("FULL_SCREEN", "0", PlayerPrefsDataType.Int));

        //---------------------------------------------------------------------------------
        //---------------------------------------------------------------------------------

        dataManager = new PrefsSettingDataManager(prefsDataList);
    }

    //讀取儲存值至組件
    public void LoadComponentState()
    {
        //BGM設定
        SetBgmVolume((float)dataManager.GetDataValue("BGM_VOLUME"));
        SetBgmMuteState((int)dataManager.GetDataValue("BGM_MUTE") == 1);

        //SE設定
        SetSeVolume((float)dataManager.GetDataValue("SE_VOLUME"));
        seMuteImg.sprite = muteStateSprite[(int)dataManager.GetDataValue("SE_MUTE")];

        //全螢幕設定
        isFullScreenStateGo[0].SetActive((int)dataManager.GetDataValue("FULL_SCREEN") == 0);
        isFullScreenStateGo[1].SetActive((int)dataManager.GetDataValue("FULL_SCREEN") == 1);
    }

    //開關視窗
    public void CallWindowUI(bool onOff)
    {
        Time.timeScale = onOff ? 0 : 1; //開啟設定介面時遊戲暫停

        AudioManagerScript.Instance.PlayAudioClip("SE短按鈕");

        visualWindow.SetActive(onOff);
    }

    //設定BGM音量
    public void SetBgmVolume(float v)
    {
        if (bgmVolumeSlider.value != v) bgmVolumeSlider.value = v; //同步拉桿值
        AudioManagerScript.Instance.SetVolume(0, v); //設定音量

        dataManager.SetDataValue("BGM_VOLUME", v); //儲存值
    }

    //設定BGM靜音狀態 多載(1/2) 自動切換開關
    public void SetBgmMuteState()
    {
        bool muteState = (int)dataManager.GetDataValue("BGM_MUTE") == 1; //目前靜音狀態
        muteState = !muteState;

        SetBgmMuteState(muteState);
    }

    //設定BGM靜音狀態 多載(2/2) 指定開關狀態
    private void SetBgmMuteState(bool b)
    {
        AudioManagerScript.Instance.SetMuteState(0, b); //設定音源靜音狀態
        bgmMuteImg.sprite = muteStateSprite[b ? 1 : 0]; //設定圖片

        dataManager.SetDataValue("BGM_MUTE", ( b ? 1 : 0 )); //將靜音狀態存入遊戲儲存值
    }

    //設定SE音量
    public void SetSeVolume(float v)
    {
        if (seVolumeSlider.value != v) seVolumeSlider.value = v; //同步拉桿值
        AudioManagerScript.Instance.SetVolume(1, v); //設定音量

        dataManager.SetDataValue("SE_VOLUME", v);
    }

    //設定SE靜音狀態 多載(1/2) 自動切換開關
    public void SetSeMuteState()
    {
        bool muteState = (int)dataManager.GetDataValue("SE_MUTE") == 1; //目前靜音狀態
        muteState = !muteState;

        SetSeMuteState(muteState);
    }

    //設定SE靜音狀態 多載(2/2) 指定開關狀態
    private void SetSeMuteState(bool b)
    {
        AudioManagerScript.Instance.SetMuteState(1, b); //設定音源靜音狀態
        seMuteImg.sprite = muteStateSprite[b ? 1 : 0]; ; //設定圖片

        dataManager.SetDataValue("SE_MUTE", ( b ? 1 : 0 )); //將靜音狀態存入遊戲儲存值
    }

    //設定全螢幕狀態
    public void SetFullScreenState()
    {
        bool isFullScreen = (int)dataManager.GetDataValue("FULL_SCREEN") == 1; //目前全螢幕狀態
        isFullScreen = !isFullScreen;

        string dialogueContent = string.Empty; //對話框文字
        UnityEngine.Events.UnityAction SetFullScreenMode = () => //設置全螢幕模式
        {
            Screen.fullScreen = isFullScreen;

            isFullScreenStateGo[0].SetActive(!isFullScreen);
            isFullScreenStateGo[1].SetActive(isFullScreen);
            dataManager.SetDataValue("FULL_SCREEN", ( isFullScreen ? 1 : 0 ));
        };

        if (isFullScreen) dialogueContent = "是否設定為全螢幕模式 ?";
        else dialogueContent = "是否設定為視窗模式 ?";

        confirmWindow.WindowPopup(dialogueContent, SetFullScreenMode);
    }
}
