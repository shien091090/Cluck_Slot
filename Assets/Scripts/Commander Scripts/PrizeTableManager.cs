//支付表管理腳本
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PrizeTableManager : MonoBehaviour, IPointerClickHandler
{
    private static PrizeTableManager _instance;
    public static PrizeTableManager Instance { get { return _instance; } } //取得單例物件

    [Header("可自訂參數")]
    public int ItemColumnCount; //項目欄數
    public float defultSpacing; //預設間隔
    public Vector2 preferredSize; //圖案預設尺寸
    public Color defaultColor; //預設背景色
    public Color freeOrderColor; //"自由排列"背景色

    [Header("遊戲進行狀態")]
    public List<Combination> combinationList; //支付表項目元素列表

    [Header("參考物件")]
    public GameObject prizeTablePanel; //支付表面板物件
    public PrizeTableSetting prizeTable; //支付表設定
    public GridLayoutGroup combinationHolder; //組合項目父物件(LayoutGroup)

    [Header("Prefab")]
    public GameObject CombinationPrefab; //組合項目預置體
    public GameObject patternPrefab; //圖案預置體

    public Dictionary<ElementImageType, Sprite> Dict_imageTable { private set; get; } //(字典)圖案樣式與圖案精靈的對照表

    //---------------------------------------------------------------------------------------------------------------------------------------------

    void Awake()
    {
        if (_instance == null) _instance = this; //設定單例模式    
    }

    //滑鼠左鍵點擊
    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        CallPrizeTableUI(false);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------

    //呼叫支付表介面
    public void CallPrizeTableUI(bool b)
    {
        Time.timeScale = b ? 0 : 1; //開啟支付表時遊戲暫停

        AudioManagerScript.Instance.PlayAudioClip("SE短按鈕");

        prizeTablePanel.SetActive(b);
    }

    //初始化
    public void Initiailze(PrizeTableSetting setting)
    {
        if (setting == null || setting.m_data.Count == 0) return; //若支付表尚未設定則直接結束程序

        prizeTable = setting; //套用設定

        //調整UI
        int _total = prizeTable.m_data.Count; //取得項目總數
        float _holderHeight = combinationHolder.gameObject.GetComponent<RectTransform>().rect.height; //取得視窗高度
        float _elementHeight = combinationHolder.cellSize.y; //取得項目元素高度
        float _spacing = defultSpacing; //取得預設間隔
        int _max = CalculateMaxCount(ItemColumnCount, _holderHeight, _elementHeight, _spacing); //取得項目可容納最大數量

        if (_total > _max) //若欲顯示的項目數超過可容納最大數, 則壓縮間隔(Spacing)
        {
            int _inputCount = ( Mathf.CeilToInt((float)( _total - _max ) / ItemColumnCount) * ItemColumnCount ) + _max; //取得欲計算最適間距的項目數
            float _newSpacing = CalculateFitSpacing(_inputCount, ItemColumnCount, _holderHeight, _elementHeight); //計算適當間距

            combinationHolder.spacing = new Vector2(combinationHolder.spacing.x, _newSpacing); //套用新的項目間隔
        }
        else
        {
            combinationHolder.spacing = new Vector2(combinationHolder.spacing.x, defultSpacing); //設定項目間隔
        }

        //創建圖案字典
        if (Dict_imageTable == null) //若字典尚未建立, 則建立之
        {
            Dict_imageTable = new Dictionary<ElementImageType, Sprite>();
            List<ElementAttributeData> _data = ScrollManager.Instance.elementAttribute.m_data;

            for (int i = 0; i < _data.Count; i++)
            {
                Dict_imageTable.Add(_data[i].imgType, _data[i].simpleSprite);
            }
        }

        //列表重新整理&排序
        List<PrizeCombination> _sortedList = new List<PrizeCombination>(); //排列完成列表
        _sortedList = new List<PrizeCombination>();

        List<List<PrizeCombination>> repetitionSortList = CombinationSort(prizeTable.m_data, CombinationSortType.純元素與否); //依純元素or混和元素分割列表
        for (int i = 0; i < repetitionSortList.Count; i++)
        {
            if (i == 0) //純元素
            {
                List<List<PrizeCombination>> patternSortList = CombinationSort(repetitionSortList[i], CombinationSortType.同花色與否); //依花色分割列表
                for (int j = 0; j < patternSortList.Count; j++)
                {
                    patternSortList[j].Sort(SortByAmount); //依圖案數量排序
                }

                patternSortList.Sort(SortByAveragePrize); //依集合平均獎金排序

                for (int j = 0; j < patternSortList.Count; j++) //逐一加入輸出列表
                {
                    _sortedList.AddRange(patternSortList[j]);
                }

            }
            if (i == 1) //混和元素
            {
                repetitionSortList[i].Sort(SortByPrize); //依照獎金多寡排序
                _sortedList.AddRange(repetitionSortList[i]);
            }
        }

        //物件創立
        if (combinationList == null) combinationList = new List<Combination>(); //若組合項目列表不存在則建立之

        for (int i = 0; i < _sortedList.Count; i++)
        {
            if (combinationList.Count < _sortedList.Count) //組合項目物件不足時, 創建之
            {
                GameObject _go = Instantiate(CombinationPrefab, combinationHolder.transform);
                Combination _comb = _go.GetComponent<Combination>(); //取得腳本

                combinationList.Add(_comb); //加入列表
            }

            Color _color = _sortedList[i].freeOrder ? freeOrderColor : defaultColor; //取得背景色
            combinationList[i].Initialize(_sortedList[i].combinations, _sortedList[i].prizeMoney, _color, Dict_imageTable);
        }

        for (int i = _sortedList.Count; i < combinationList.Count; i++) //暫時隱藏多餘的支付表項目
        {
            combinationList[i].Invalid();
        }
    }

    //計算UI視窗範圍內最多可容納多少項目
    private int CalculateMaxCount(int columnCount, float holderHeight, float elementHeight, float spacing)
    {
        int _result = 0;

        /* 計算UI視窗範圍內最多可容納多少項目
         * 
         * Hh = 視窗高度
         * Eh = 項目元素高度
         * s = 間隔
         * Cc = 欄數
         * Floor(x) = 無條件捨去函式
         * 
         * 公式 : Floor((Hh + s / Eh + s)) * Cc
         */
        _result = Mathf.FloorToInt(( holderHeight + spacing ) / ( elementHeight + spacing )) * columnCount;

        return _result; //返回結果
    }

    //計算最適當間隔
    private float CalculateFitSpacing(int total, int columnCount, float holderHeight, float elementHeight)
    {
        float _result = 0;

        /* 計算最適當間距
         * 
         * t = 項目總數
         * Hh = 視窗高度
         * Eh = 項目元素高度
         * Cc = 欄數
         * Floor(x) = 無條件捨去函式
         * 
         * 公式 : Floor(Hh - (t / Cc) * Eh) / ( (t / Cc) - 1 )
         * ※由於Floor函數無法指定小數點以下第N位, 故需要另外使用"*10^N / 10^N"的方式來計算
         */
        _result = Mathf.Floor(( ( holderHeight - ( ( ( (float)total / (float)columnCount ) * elementHeight ) ) ) / ( (float)total / (float)columnCount - 1 ) ) * 10) / 10;

        return _result; //返回結果

    }

    private List<List<PrizeCombination>> CombinationSort(List<PrizeCombination> sourceList, CombinationSortType sortType)
    {
        List<List<PrizeCombination>> _result = new List<List<PrizeCombination>>();

        switch (sortType)
        {
            case CombinationSortType.純元素與否:
                //[0]=純元素
                //[1]=非純元素
                List<PrizeCombination> pure = new List<PrizeCombination>();
                List<PrizeCombination> mix = new List<PrizeCombination>();

                for (int i = 0; i < sourceList.Count; i++) //測試是否為純元素
                {
                    if (sourceList[i].PureTest()) pure.Add(sourceList[i]);
                    else mix.Add(sourceList[i]);
                }

                _result.Add(pure);
                _result.Add(mix);

                break;

            case CombinationSortType.同花色與否:
                for (int i = 0; i < sourceList.Count; i++)
                {
                    List<PrizeCombination> _combList = new List<PrizeCombination>();

                    if (_result.Count != 0)
                    {
                        bool _out = false;

                        for (int j = 0; j < _result.Count; j++)
                        {
                            if (sourceList[i].combinations[0] == _result[j][0].combinations[0])
                            {
                                _result[j].Add(sourceList[i]);
                                _out = true;
                                break;
                            }
                        }

                        if (_out) continue;
                    }

                    _combList.Add(sourceList[i]);
                    _result.Add(_combList);
                }

                break;

            case CombinationSortType.排序相似與否:
                break;
        }

        return _result;
    }

    //依元素數量排序
    private int SortByAmount(PrizeCombination x, PrizeCombination y)
    {
        if (x.combinations.Count > y.combinations.Count) return 1;
        if (x.combinations.Count < y.combinations.Count) return -1;
        return 0;
    }

    //依元素獎金排序
    private int SortByPrize(PrizeCombination x, PrizeCombination y)
    {
        if (x.prizeMoney > y.prizeMoney) return 1;
        if (x.prizeMoney < y.prizeMoney) return -1;
        return 0;
    }

    //依集合元素平均獎金排序
    private int SortByAveragePrize(List<PrizeCombination> x, List<PrizeCombination> y)
    {
        float _averageX = 0; // x列表的平均獎金
        float _averageY = 0; // y列表的平均獎金

        for (int i = 0; i < x.Count; i++)
        {
            _averageX += x[i].prizeMoney;
        }
        _averageX /= x.Count;

        for (int i = 0; i < y.Count; i++)
        {
            _averageY += y[i].prizeMoney;
        }
        _averageY /= y.Count;

        if (_averageX > _averageY) return 1;
        if (_averageX < _averageY) return -1;
        return 0;
    }
}