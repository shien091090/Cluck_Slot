//(ScriptableObject)
//中獎線設定
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 一個中獎線設定集可以設定多個中獎線等級
 * 每一級別可能有多個中獎線
 * 每個中獎線設定複數個點連成一個線段
 */

//中獎線設定集
[CreateAssetMenu(fileName = "NewLotteryLineSetting", menuName = "SNTools/Create LotteryLineSetting", order = 2)]
public class LotteryLineSetting : ScriptableObject
{
    public List<LotteryLineLevel> lotteryLineLevelList; //中獎線等級列表
}

//中獎線等級
[System.Serializable]
public class LotteryLineLevel
{
    public int levelNumber; //等級編號
    public Color lineColor; //線段顏色
    public Color inactiveColor; //未激活顏色
    public float thickness; //線段粗細度
    public List<LotteryLine> lines; //線段列表
}

//中獎線
[System.Serializable]
public class LotteryLine
{
    public List<Vector2> coordinates; //相連座標
}

//線段渲染事件參數
public class LineRenderEventArgs : System.EventArgs
{
    public List<int> levelList; //指定等級(可複數)

    //建構子
    public LineRenderEventArgs(List<int> v)
    {
        levelList = v;
    }
}