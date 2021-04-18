//(ScriptableObject)
//輪盤設定數據(產生捲軸結果時從中抽取)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewRouletteSetting", menuName = "SNTools/Create RouletteSetting", order = 4)]
public class RouletteSetting : ScriptableObject
{
    [System.Serializable]
    public struct RouletteData
    {
        public ElementImageType type; //圖格類型
        public int amount; //數量
    }

    public List<RouletteData> m_data;

    //輸出成List<Roulette>
    public List<Roulette> OutputRouletteList()
    {
        List<Roulette> _resultList = new List<Roulette>();

        for (int i = 0; i < m_data.Count; i++)
        {
            Roulette _r = new Roulette(m_data[i].type, m_data[i].amount);
            _resultList.Add(_r);
        }

        return _resultList;
    }

    //輸入List<Roulette>並轉換儲存
    public void SaveRouletteData(List<Roulette> RouletteList)
    {
        m_data = new List<RouletteData>();

        for (int i = 0; i < RouletteList.Count; i++)
        {
            RouletteData _data = new RouletteData();
            _data.type = RouletteList[i].type;
            _data.amount = RouletteList[i].amount;

            m_data.Add(_data);
        }
    }
}

[System.Serializable]
public class Roulette
{
    public ElementImageType type; //圖格類型
    public int amount; //數量

    //建構子
    public Roulette(ElementImageType t, int a)
    {
        type = t;
        amount = a;
    }
}