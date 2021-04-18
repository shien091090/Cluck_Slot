using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 專門用來處理List的腳本, 有許多快速返回特定值的方法可供調用
/// </summary>
/// <typeparam name="T"></typeparam>
public static class ListExtensibleScript<T>
{
    /// <summary>
    /// 取出List最大值
    /// </summary>
    /// <param name="sourceList">欲取出值的List</param>
    /// <returns></returns>
    public static T GetMaxValue(List<T> sourceList)
    {
        sourceList.Sort();
        return sourceList[sourceList.Count - 1];
    }

    /// <summary>
    /// 取出List最小值
    /// </summary>
    /// <param name="sourceList">欲取出值的List</param>
    /// <returns></returns>
    public static T GetMinValue(List<T> sourceList)
    {
        sourceList.Sort();
        return sourceList[0];
    }

    /// <summary>
    /// 取出所有List中最大值的List(若List中的最大值有複數個時)
    /// </summary>
    /// <param name="sourceList">欲取出值的List</param>
    /// <returns></returns>
    public static List<T> GetMaxList(List<T> sourceList)
    {
        sourceList.Sort();
        List<T> maxList = new List<T>() { sourceList[sourceList.Count - 1] };

        for (int i = sourceList.Count - 2; i >= 0; i--)
        {
            //直接用邏輯運算子"=="時會判斷為false, 必須使用Equals來判斷是否相等
            if (sourceList[i].Equals(sourceList[sourceList.Count - 1])) maxList.Add(sourceList[i]);
            else break;
        }

        return maxList;
    }

    /// <summary>
    /// 篩掉重複值並返回新的List
    /// </summary>
    /// <param name="sourceList">欲篩掉重複值的來源List</param>
    /// <returns></returns>
    public static List<T> RepetitionFilter(List<T> sourceList)
    {
        List<T> _outputList = new List<T>();

        for (int i = 0; i < sourceList.Count; i++)
        {
            if (_outputList.Count == 0) //List的第一項, 無條件加入列表
            {
                _outputList.Add(sourceList[i]);
                continue;
            }

            bool isRepeat = false;
            for (int j = 0; j < _outputList.Count; j++) //比較來源List與待輸出List中是否存在重複值
            {
                if (sourceList[i].Equals(_outputList[j])) isRepeat = true;
            }

            if (!isRepeat) _outputList.Add(sourceList[i]); //若值不重複則加入至待輸出List
        }

        return _outputList; //輸出結果
    }

    /// <summary>
    /// 從列表中隨機抽選
    /// </summary>
    /// <param name="sourceList">欲隨機抽選的來源List</param>
    /// <returns></returns>
    public static T RandomReturn(List<T> sourceList)
    {
        int c = sourceList.Count;
        int dice = Random.Range(0, c); //隨機抽選索引值

        return sourceList[dice];
    }
}
