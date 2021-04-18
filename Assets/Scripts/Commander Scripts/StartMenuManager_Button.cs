//開頭選單流程控制腳本
//[Partial]按鈕呼叫方法
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public partial class StartMenuManager : MonoBehaviour
{
    //開始遊戲
    public void Btn_StartGame()
    {
        //Debug.Log("開始遊戲");
        AudioManagerScript.Instance.Stop(0); //背景音樂停止

        StartCoroutine(Cor_LoadScene());
    }

    //儲值
    public void Btn_Charge()
    {
        //Debug.Log("儲值");
        confirmWindow.WindowPopup("是否進行儲值遊戲?", moneyChargeScript.ChargeGame);
    }

    //設定
    public void Btn_Setting()
    {
        //Debug.Log("設定");

        gameSettingPanel.CallWindowUI(true);
    }

    //離開遊戲
    public void Btn_ExitGame()
    {
        //Debug.Log("離開遊戲");

        Application.Quit();
    }

    //刪除紀錄
    public void Btn_ClearSaving()
    {
        UnityEngine.Events.UnityAction ClearSaving = () =>
        {
            PlayerPrefs.DeleteKey("GAME_SLOTLEVEL"); //拉霸等級
            PlayerPrefs.DeleteKey("GAME_SUMPRIZE"); //拉霸等級累計獎金
            PlayerPrefs.DeleteKey("GAME_SPINLEVELPROGRESS"); //拉霸等級進度條
            PlayerPrefs.DeleteKey("GAME_MONEY"); //所持金錢
            PlayerPrefs.DeleteKey("D_YEAR"); //時間紀錄(儲值遊戲)

            SceneManager.LoadScene("StartMenu");
        };

        confirmWindow.WindowPopup("確定刪除所有紀錄(包含所持金錢)？", ClearSaving);
    }
}
