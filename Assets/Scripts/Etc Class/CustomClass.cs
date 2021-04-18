using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SensorInfo
{
    public Vector2 coordinate; //座標
    public ElementImageType elementType; //圖格類型
}

//旋轉模式
[System.Serializable]
public class SpinMode
{
    public bool direction; //旋轉方向(true = 往下轉, false = 往上轉)
    public float elementSpeed; //旋轉速度(每秒幾張圖格(會算上間隔)
    public float spinTime; //旋轉時間

    //建構子
    public SpinMode(bool dir, float speed, float time)
    {
        direction = dir;
        elementSpeed = speed;
        spinTime = time;
    }
}

//整體旋轉模式
[System.Serializable]
public class SpinModePack
{
    public List<SpinMode> spinModePack;
}

//拉霸轉動後的中獎線資訊
[System.Serializable]
public class PrizeLineInfo
{
    public int lineLevel; //中獎線等級
    public int sumPrize; //總獎金
    public List<CombinationPrizeInfo> PrizeItemList { private set; get; } //支付表比對中獎項目列表
    public List<Vector2> drawedPosList; //中獎連線座標

    //建構子
    public PrizeLineInfo()
    {
        sumPrize = 0;
        PrizeItemList = new List<CombinationPrizeInfo>();
        drawedPosList = new List<Vector2>();
    }

    //加入中獎支付表項目
    public void AddCombinationItem(CombinationPrizeInfo Item)
    {
        sumPrize += Item.prizeMoney;
        PrizeItemList.Add(Item);
    }
}

//支付表比對資訊
[System.Serializable]
public class CombinationPrizeInfo
{
    public int prizeMoney; //獎金
    public List<ElementImageType> patternList; //圖案串接形式
    public List<int> matchedPosList; //中獎圖格索引值列表
    public PrizeLineType prizeType; //獎項類型
}

//拉霸等級設定
[System.Serializable]
public struct SlotLevel
{
    public int level; //等級
    public int visibleCount; //可視圖格數量
    public int scrollCount; //捲軸數量
    public Vector2 elementSpacing; //圖格間隔
    public int activeBetLevel; //可使用的下注等級
    public LotteryLineSetting lineSetting; //中獎線設定
    public RouletteSetting rouletteData; //輪盤設定
    public PrizeTableSetting prizeTable; //支付表設定
    public float linkTime; //牽動等待時間
    public List<SpinModePack> spinModeSetting; //旋轉模式列表(每次旋轉從列表中抽選)
}