using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//捲軸UI布局調整事件 ScrollManager.LayoutSetted(Event)的EventArgs類型
public class LayoutSettingEventArgs : System.EventArgs
{
    public LayoutSheet layoutData; //UI布局資料

    public int visibleElementCount; //可見圖格數量
    public UnityEngine.Vector2 elementSpacing; //圖格間隔
    //public bool isAnimation; //啟用動畫
    public AnimationCurve curve; //動畫曲線

    //建構子
    public LayoutSettingEventArgs(LayoutSheet data, int visibleCount, UnityEngine.Vector2 spacing, AnimationCurve ac)
    {
        layoutData = data;

        visibleElementCount = visibleCount;
        elementSpacing = spacing;
        curve = ac;
    }
}

//旋轉位置判定事件 ScrollManager.SpinJudged(Event)的EventArgs類型
public class PositionSensorEventArgs : System.EventArgs
{
    public List<SensorInfo> SpinResultList { private set; get; } //旋轉判定結果(透過PositionSensorBehavior中的事件將判定結果儲存在這個List內)

    public int overNumber; //事件呼叫倒數次數(透過事件呼叫時-1的方式判斷事件是否完整結束)

    //---------------------------------------------------------------------------------------------------------------------------------------------

    //建構子
    //[param] sensorCount = 偵測器的數量
    public PositionSensorEventArgs(int sensorCount)
    {
        SpinResultList = new List<SensorInfo>();
        overNumber = sensorCount;
    }

    //儲存偵測結果
    //[param] cdn = 座標編號 , type = 圖格類型
    public void AddResult(Vector2 cdn, ElementImageType type)
    {
        SensorInfo _info = new SensorInfo();
        _info.coordinate = cdn;
        _info.elementType = type;

        SpinResultList.Add(_info);
    }
}

//創建燈號事件LevelLampManager.LampsBuilt(Event)的EventArgs類型
public class BuildLampsEventArgs : System.EventArgs
{
    public LotteryLineSetting setting; //中獎線設定集
    public List<LineDrawer> activeLines; //有效中獎線列表

    //建構子
    public BuildLampsEventArgs(LotteryLineSetting set, List<LineDrawer> lines)
    {
        setting = set;
        activeLines = lines;
    }
}

//圖格動畫事件
public class ElementAnimationEventArgs : System.EventArgs
{
    public ElementAnimationType animationType; //動畫類型
    public bool onOff; //開關
    public List<Vector2> targetPosList; //作用座標列表

    //建構子(多載1/2)
    public ElementAnimationEventArgs(ElementAnimationType type, List<Vector2> posList, bool b)
    {
        animationType = type;
        targetPosList = posList;
        onOff = b;
    }

    //建構子(多載2/2) 不指定開關時, 預設為開(true)
    public ElementAnimationEventArgs(ElementAnimationType type, List<Vector2> posList)
    {
        animationType = type;
        targetPosList = posList;
        onOff = true;
    }
}