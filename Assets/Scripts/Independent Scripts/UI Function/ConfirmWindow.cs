//確認視窗
//使用方式：在欲呼叫確認視窗的Button組件上依序加入事件 StartUp_SetContent(設定視窗文字) >> StartUp_SetCall(設定呼叫方法) >> WindowPopup(彈出視窗)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ConfirmWindow : MonoBehaviour
{
    [Header("參考物件")]
    public GameObject visualArea; //視窗顯示物件
    public Text windowContent; //視窗文字
    public Button yesBtn; //"是"按鈕

    //---------------------------------------------------------------------------------------------------------------------------------------------

    //彈出視窗
    public void WindowPopup(string content, UnityAction yesButtonCall)
    {
        windowContent.text = content; //設定視窗文字

        yesBtn.onClick.RemoveAllListeners();
        yesBtn.onClick.AddListener(yesButtonCall); //設定按鈕呼叫方法1 : 主要執行方法
        yesBtn.onClick.AddListener(ClossWindow); //設定按鈕呼叫方法2 : 關閉視窗

        visualArea.SetActive(true);
    }

    //關閉視窗
    public void ClossWindow()
    {
        visualArea.SetActive(false);

        //清除視窗內容
        windowContent.text = "";
        yesBtn.onClick.RemoveAllListeners();
    }
}
