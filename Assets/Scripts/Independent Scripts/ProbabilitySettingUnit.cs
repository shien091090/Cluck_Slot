//機率設定單元
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProbabilitySettingUnit : MonoBehaviour
{
    [Header("遊戲進行狀態")]
    public int settingIndex; //設定列表的索引值
    public int elementAmount; //圖格元素數量

    [Header("參考物件")]
    public Image img;
    public Slider sld;
    public Text percentTxt; //百分比文字
    public Text amountTxt; //總數量文字

    //---------------------------------------------------------------------------------------------------------------------------------------------

    //設定總數量
    public float SetAmount
    {
        set
        {
            elementAmount = (int)value; //設定目前圖格元素數量
            if (sld.value != elementAmount) sld.value = elementAmount; //設定捲軸
            amountTxt.text = "× " + ( (int)value ).ToString(); //數量顯示

            TestPanelManager.Instance.ModifyElementCount(settingIndex, elementAmount); //修改元素數量
        }

        get
        {
            return elementAmount;
        }
    }

    //初始化
    public void Initialize(Sprite sp, int i, int a, int max)
    {
        this.gameObject.SetActive(true);
        img.sprite = sp; //設定圖片
        settingIndex = i; //設定索引值
        sld.maxValue = max; //設定拉桿最大值
        SetAmount = a; //設定圖格元素數量
    }

    //無效化
    public void Invalid()
    {
        this.gameObject.SetActive(false);
        settingIndex = -1;
    }

    //回報元素數量
    public void FeedbackAmount(object sender, System.EventArgs e)
    {
        if (!this.gameObject.activeSelf) return; //若物件隱藏時直接結束程序

        TestPanelManager mg = (TestPanelManager)sender;
        mg.totalAmount += elementAmount;
    }

    //顯示百分比
    public void ShowPercentage(int total)
    {
        if (!this.gameObject.activeSelf) return; //若物件隱藏時直接結束程序

        float _p = elementAmount == 0 ? 0 : (float)elementAmount / total;
        percentTxt.text = ( _p * 100 ).ToString("0.00") + "%";

        //Debug.Log(string.Format("索引 : {0} / 設定值 : {1}", settingIndex, percentTxt.text));
    }
}
