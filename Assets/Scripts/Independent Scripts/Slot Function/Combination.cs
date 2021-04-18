//支付表項目元素
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Combination : MonoBehaviour
{
    [Header("參考物件")]
    public Text txt;
    public Image img;
    public Transform imageHolder;

    [Header("遊戲進行狀態")]
    public List<Pattern> patternList; //圖案列表

    //---------------------------------------------------------------------------------------------------------------------------------------------

    //初始化
    //[param] typeList = 圖案樣式列表 , prizeMoney = 獎金 , backgroundColor = 背景色 , dict_imageTable = 圖案樣式與圖案精靈的對照表(字典)
    public void Initialize(List<ElementImageType> typeList, int prizeMoney, Color backgroundColor, Dictionary<ElementImageType, Sprite> dict_imageTable)
    {
        if (!this.gameObject.activeSelf || typeList == null || typeList.Count == 0) return; //若此物件未激活 或 輸入的圖案列表無效時 直接結束程序

        this.gameObject.SetActive(true); //激活此物件

        //圖案物件處理
        if (patternList == null) patternList = new List<Pattern>(); //若圖案列表不存在則建立之
        GameObject patternPrefab = PrizeTableManager.Instance.patternPrefab; //取得圖案物件預置物

        for (int i = 0; i < typeList.Count; i++)
        {
            if (patternList.Count < typeList.Count) //圖案物件不足時, 創建之
            {
                GameObject _go = Instantiate(patternPrefab, imageHolder);
                Pattern _pattern = _go.GetComponent<Pattern>(); //取得腳本

                patternList.Add(_pattern); //加入列表
            }

            patternList[i].Initialize(dict_imageTable[typeList[i]], PrizeTableManager.Instance.preferredSize); //設定圖片
        }

        for (int i = typeList.Count; i < patternList.Count; i++) //暫時隱藏多餘的圖案
        {
            patternList[i].Invalid();
        }

        //顯示獎金文字
        string _suffix = MoneyManager.Instance.moneyLabelSuffix; //取得文字後綴
        txt.text = prizeMoney.ToString() + _suffix; //設定文字

        //設定背景色
        img.color = backgroundColor;
    }

    //項目無效化
    public void Invalid()
    {
        txt.text = "";
        this.gameObject.SetActive(false);
    }
}
