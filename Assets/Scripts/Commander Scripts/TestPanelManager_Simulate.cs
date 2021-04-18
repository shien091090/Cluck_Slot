//測試工具
//(Paritial)拉霸模擬
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class TestPanelManager : MonoBehaviour
{
    //拉霸模擬
    public void SlotSimulate()
    {
        if (slotSimulating) return; //尚在模擬時, 強制結束程序

        int _times = 0; //模擬次數
        if (!int.TryParse(timesIf.text, out _times) || _times <= 0 || _times > 9999) //輸入值為非數字時 或 次數不可≦0 或 > 9999
        {
            timesIf.text = "0";
            return;
        }

        slotSimulating = true; //開始模擬
        simulateEndingTag = false; //重置結束標記

        startSimulateBtnText.text = "結束模擬"; //更改按鈕文字
        simulateButton.onClick.RemoveAllListeners(); //清空按鈕回調方法
        simulateButton.onClick.AddListener(StopSimulate); //設定回調方法為"結束模擬"

        StartCoroutine(Cor_SlotSimulate(_times));
    }

    //結束拉霸模擬
    public void StopSimulate()
    {
        if (simulateEndingTag) return; //已經發出模擬中斷訊號, 結束程序以避免重複執行

        StartCoroutine(Cor_StopSimulate());
    }

    //(協程)結束拉霸模擬
    private IEnumerator Cor_StopSimulate()
    {
        simulateEndingTag = true;

        yield return new WaitWhile(() => slotSimulating); //等待模擬程序執行完畢

        startSimulateBtnText.text = "開始模擬"; //更改按鈕文字
        simulateButton.onClick.RemoveAllListeners(); //清空按鈕回調方法
        simulateButton.onClick.AddListener(SlotSimulate); //設定回調方法為"開始模擬"
    }

    //(協程)拉霸模擬
    private IEnumerator Cor_SlotSimulate(int times)
    {
        levelMemo = (int)BetController.Instance.betSlider.value; //紀錄等級

        //初始化
        TestSimulationUnit.s_totalTimes = 0; //總模擬次數歸零
        for (int i = 0; i < testUnitList.Count; i++) //重置測試模擬單位的參數
        {
            testUnitList[i].ResetParameter();
        }

        List<int> levelList = new List<int>(); //等級列表
        for (int i = 0; i < BetController.Instance.lineLevelBlockList.Count; i++)
        {
            levelList.Add(i + 1);
        }

        LotteryLineManager.Instance.DisplayLines(levelList); //開放所有等級的中獎線

        for (int i = 0; i < times; i++) //拉霸模擬程序
        {
            if (simulateEndingTag) break;
            TestSimulationUnit.s_totalTimes++; //模擬次數+1
            yield return StartCoroutine(ScrollManager.Instance.Cor_SpinTest());
        }

        LotteryLineManager.Instance.DisplayLines(levelMemo); //中獎線等級設定復歸
        levelMemo = 0;

        slotSimulating = false; //結束模擬狀態

        StartCoroutine(Cor_StopSimulate());
    }
}
