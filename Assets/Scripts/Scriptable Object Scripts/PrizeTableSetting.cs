//(ScriptableObject)
//支付表設定
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPrizeTable", menuName = "SNTools/Create Prize Table", order = 3)]
public class PrizeTableSetting : ScriptableObject //支付表設定
{
    public List<PrizeCombination> m_data;
}

[System.Serializable]
public class PrizeCombination //支付組合
{
    public List<ElementImageType> combinations; //圖形串接樣式
    public int prizeMoney; //獎金
    public bool freeOrder; //可任意順序
    public PrizeLineType prizeLineType; //連線獎項類型

    //判斷組合是否為純元素所構成
    public bool PureTest()
    {
        bool b = true;

        for (int i = 0; i < combinations.Count; i++) //所有元素相互比較
        {
            for (int j = i + 1; j < combinations.Count; j++)
            {
                if (combinations[i] != combinations[j]) return false; //若其中一項不等於則判斷為非純元素
            }
        }

        return b;
    }

    //列表比對(判斷是否中獎)
    //[output] CombinationPrizeInfo = 支付表項目比對中獎資訊
    //[param] mainList = 主圖格列表
    public CombinationPrizeInfo Comparison(List<ElementImageType> mainList)
    {
        CombinationPrizeInfo _info = new CombinationPrizeInfo();
        _info.prizeMoney = 0;
        _info.prizeType = prizeLineType; //設定連線獎項類型
        _info.patternList = combinations; //設定圖案串接形式

        //string _s = "";
        //for (int i = 0; i < combinations.Count; i++)
        //{
        //    _s += combinations[i] + ( i == combinations.Count - 1 ? "" : "," );
        //}

        //Debug.Log("[組合]" + _s);

        List<int> _matchedPos = new List<int>();

        if (freeOrder) //可任意順序排列
        {
            for (int i = 0; i < mainList.Count - ( combinations.Count - 1 ); i++) //遍歷線段圖格至(組合長度 - 1)位置
            {
                int _k = i; //索引定位標記
                bool _anchor = true; //索引定位與否
                int _wildCount = 0; //狂野符號數量
                List<ElementImageType> _comb = new List<ElementImageType>();
                _comb.AddRange(combinations); //暫存支付表組合
                _matchedPos = new List<int>(); //初始化判斷成功的索引值列表

                while (_anchor)
                {
                    _anchor = false; //假設判斷會失敗

                    for (int j = 0; j < _comb.Count; j++) //遍歷支付表組合
                    {
                        bool isWild = ScrollManager.Instance.Dict_wildSymbol[mainList[_k]]; //是否為狂野符號
                        if (isWild) _wildCount += 1; //紀錄狂野符號數量

                        if (mainList[_k] == _comb[j] || isWild) //若線段上第 i 項與支付表組合相符 或 該圖格為狂野符號時
                        {
                            if (!isWild) _comb.RemoveAt(j); //將組合中符合的項目抽掉(若為狂野符號則不抽掉任何元素)
                            _matchedPos.Add(_k); //紀錄符合的索引

                            if (_k + 1 == mainList.Count) break; //若預判斷的圖格已經到最後一項則直接結束
                            _k++;
                            _anchor = true; //判斷成功, 繼續判斷下一項是否符合
                            break;
                        }
                    }

                    //Debug.Log(string.Format("i = {0}, WildCount = {1}", i, _wildCount));
                    if (_comb.Count - _wildCount == 0) //若組合元素全部被抽光, 表示元素全部比對成功
                    {
                        _info.matchedPosList = _matchedPos;
                        _info.prizeMoney = this.prizeMoney;
                        break;
                    }
                }

            }

        }
        else //按照固定順序排列
        {
            for (int i = 0; i < mainList.Count; i++) //遍歷線段圖格列表
            {
                int _k = i; //索引定位標記
                int _matchCount = 0; //符合的數量
                _matchedPos = new List<int>(); //初始化判斷成功的索引值列表

                for (int j = 0; j < combinations.Count; j++) //遍歷支付表組合
                {
                    //當捲軸第 i 項判斷成功時, 則以該索引為開頭, 逐一往後比對
                    if (mainList[_k] != combinations[j] && !ScrollManager.Instance.Dict_wildSymbol[mainList[_k]]) break;
                    else
                    {
                        _matchCount++; //累計符合數量
                        _matchedPos.Add(_k); //紀錄符合的索引

                        if (_k + 1 == mainList.Count) break; //若預判斷的圖格已經到最後一項則直接結束
                        _k++;
                    }
                }

                if (_matchCount == combinations.Count) //若符合數量等於列表長度(所有皆比對成功), 跳出迴圈並返回結果
                {
                    _info.matchedPosList = _matchedPos;
                    _info.prizeMoney = this.prizeMoney;
                    break;
                }
            }
        }

        return _info;
    }
}