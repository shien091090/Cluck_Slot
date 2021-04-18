//線條渲染
//用於中獎線(Lottery Lines)的顯示
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineDrawer : MonoBehaviour
{
    [Header("遊戲進行狀態")]
    public int level; //等級
    public bool isWorking; //是否啟用(中獎線判斷與否)
    public List<Vector2> coordinates; //相連座標

    private LineRenderer lineRenderer;
    [SerializeField]
    private Material mat;

    //---------------------------------------------------------------------------------------------------------------------------------------------

    void Awake()
    {
        lineRenderer = this.GetComponent<LineRenderer>();
    }

    void Start()
    {
        LotteryLineManager.Instance.LineRendered += DrawLine; //畫線事件註冊
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------

    //線段設定初始化
    //[param] level = 等級, thickness = 粗細度 , style = 線的顏色 , layer = UI層數 , coordinates = 線上所有點座標編號 , table = 座標編號轉換世界位置的字典
    //[output] bool = 線段有效與否
    public bool Initialize(int lv, float thickness, Color style, int layer, List<Vector2> cdn, Dictionary<Vector2, Vector3> table)
    {
        lineRenderer.enabled = false; //關閉渲染

        //線段有效性測試
        List<Vector3> _posList = new List<Vector3>(); //位置列表(從座標轉換到世界位置)
        bool isValid = true; //有效性測試

        if (cdn == null || cdn.Count == 0) //若座標編號列表為空則線段無效, 直接結束程序並返回false
        {
            Invalid(); //線段無效化
            return false;
        }

        for (int i = 0; i < cdn.Count; i++) //遍歷中獎線的每一個點
        {
            if (table.ContainsKey(cdn[i])) //若有符合設定的座標編號則加入至列表
            {
                _posList.Add(table[cdn[i]]);
            }
            else //若有一個座標編號不符合的狀況, 則此線段無效
            {
                isValid = false;
                break;
            }
        }

        if (cdn[cdn.Count - 1].x != ScrollManager.Instance.scrollCount - 1) isValid = false; //若最右邊的點沒有連到最右方的捲軸時, 視為無效線段

        if (!isValid) //線段無效, 直接結束程序並返回false
        {
            Invalid(); //線段無效化
            return false;
        }

        level = lv; //等級設定
        if (lv == 1) isWorking = true; //若等級為1(最低等級)則預設為可運作

        //粗細設定
        if (lineRenderer.startWidth != thickness) lineRenderer.startWidth = thickness;
        if (lineRenderer.endWidth != thickness) lineRenderer.endWidth = thickness;

        //顏色設定
        if (mat == null)
        {
            mat = new Material(Shader.Find("UI/Default"));
        }
        if (mat.color != style) mat.color = style;
        lineRenderer.material = mat; //材質設定

        if (lineRenderer.sortingOrder != layer) lineRenderer.sortingOrder = layer; //Layer與指定UI層一致

        //座標位置設定
        coordinates = cdn;
        lineRenderer.positionCount = _posList.Count; //設定渲染點數量
        lineRenderer.SetPositions(_posList.ToArray()); //設定渲染點位置

        return true;
    }

    //線段無效化
    public void Invalid()
    {
        level = 0; //等級暫時設為0
        lineRenderer.positionCount = 0; //設定渲染點數量為0
        lineRenderer.enabled = false; //關閉渲染
    }

    //加入兩邊端點燈號窗格的位置(使線段往左右兩邊延伸) ※[0]左 [1]右
    public void AddWindowPosition(Vector3[] windowsPos)
    {
        if (windowsPos == null || windowsPos.Length != 2) throw new System.Exception("[ERROR]燈號窗格位置列表輸入錯誤");

        List<Vector3> _newList = new List<Vector3>(); //新的線段列表

        _newList.Add(windowsPos[0]); //左邊窗格位置
        for (int i = 0; i < lineRenderer.positionCount; i++)
        {
            _newList.Add(lineRenderer.GetPosition(i)); //逐一取得目前線段設定的位置
        }
        _newList.Add(windowsPos[1]); //右邊窗格位置

        lineRenderer.positionCount += 2;
        lineRenderer.SetPositions(_newList.ToArray()); //重新設定渲染點位置
    }

    //中獎判定
    //[output] List<PrizeInfo> = 中獎資訊列表
    //[param] sensorInfo = 位置偵測器資訊(各座標上對應的圖格類型) , prizeTable = 支付表設定
    public PrizeLineInfo RewardJudgement(List<SensorInfo> sensorInfo, List<PrizeCombination> prizeTableSource)
    {
        PrizeLineInfo _resultList = new PrizeLineInfo();

        if (level == 0 || !isWorking) return null; //中獎線等級為0 或 線段未啟用 則不計算中獎

        List<ElementImageType> _imageLine = new List<ElementImageType>(); //線上的圖格類型

        for (int i = 0; i < coordinates.Count; i++) //遍歷線上所有點的座標
        {
            for (int j = 0; j < sensorInfo.Count; j++) //遍歷所有圖格資訊
            {
                if (sensorInfo[j].coordinate == coordinates[i]) //取得指定位置上的圖格
                {
                    _imageLine.Add(sensorInfo[j].elementType);
                    continue;
                }
            }
        }

        if (_imageLine.Count != coordinates.Count) throw new System.Exception("[ERROR]線段各點座標與圖格位置座標不完全一致");

        //for (int i = 0; i < _imageLine.Count; i++)
        //{
        //    Debug.Log(string.Format("({0}, {1}) : {2}", coordinates[i].x, coordinates[i].y, _imageLine[i]));
        //}

        List<PrizeCombination> prizeTable = new List<PrizeCombination>();
        prizeTable.AddRange(prizeTableSource);

        //排序支付表組合(組合長度多排到少)
        prizeTable.Sort((PrizeCombination x, PrizeCombination y) =>
        {
            if (x.combinations.Count > y.combinations.Count) return -1;
            if (x.combinations.Count < y.combinations.Count) return 1;
            return 0;
        });

        List<PrizeCombination> _drawedCombs = new List<PrizeCombination>(); //判定成功(有中獎)的支付表組合
        //Debug.Log("[ 線段 " + this.gameObject.name + " 開始判斷]");
        for (int i = 0; i < prizeTable.Count; i++) //逐一檢查支付表所有組合
        {
            bool _isMatch = false; //任一項有符合(淺層判斷)
            bool _matchPass = false; //跳過淺層判斷

            for (int j = 0; j < _imageLine.Count; j++) //若線上有任一狂野符號, 直接進入深層判斷(中獎判斷)
            {
                if (ScrollManager.Instance.Dict_wildSymbol[_imageLine[j]])
                {
                    _matchPass = true;
                    _isMatch = true;
                    break;
                }
            }

            if (!_matchPass) //沒有狂野符號時, 檢測線上所有圖格是否有任意一項與支付表組合圖格相符(淺層判斷)
            {
                if (prizeTable[i].PureTest()) //若組合為純元素構成(只輸入一個圖格類型, 節省遍歷時間)
                {
                    _isMatch = ElementListMatchTest(_imageLine, new List<ElementImageType>() { prizeTable[i].combinations[0] });
                }
                else //若組合為混和元素構成
                {
                    _isMatch = ElementListMatchTest(_imageLine, prizeTable[i].combinations);
                }
            }

            if (_isMatch) //若有任意一項圖格相符時, 深入判斷中獎與否
            {
                int _prize = 0; //中獎獎金
                for (int j = 0; j < _drawedCombs.Count; j++) //判斷中獎的組合是否包含此組合
                {
                    _prize += prizeTable[i].Comparison(_drawedCombs[j].combinations).prizeMoney;
                }

                if (_prize == 0) //已經中獎的組合如果有包含此組合, 則跳過此組合的判斷, 避免重複中獎
                {
                    CombinationPrizeInfo _cbInfo = prizeTable[i].Comparison(_imageLine); //取得中獎資訊

                    if (_cbInfo.prizeMoney > 0) //若獎金大於0(有中獎)
                    {
                        _drawedCombs.Add(prizeTable[i]); //加入判定成功組合列表

                        for (int j = 0; j < _cbInfo.matchedPosList.Count; j++) //對照索引值紀錄相應的座標
                        {
                            _resultList.drawedPosList.Add(coordinates[_cbInfo.matchedPosList[j]]);
                        }

                        _resultList.AddCombinationItem(_cbInfo);
                    }
                }

            }
        }

        _resultList.lineLevel = level; //設定中獎線等級
        _resultList.drawedPosList = ListExtensibleScript<Vector2>.RepetitionFilter(_resultList.drawedPosList); //篩掉列表中的重複元素

        return _resultList;
    }

    //畫線
    private void DrawLine(object sender, LineRenderEventArgs eventArgs)
    {
        //若沒有指定等級 或 
        //指定等級列表為空 或 
        //物件未激活時 或
        //渲染點集合為空時 線段不顯示
        if (eventArgs.levelList == null || eventArgs.levelList.Count == 0 || !this.gameObject.activeSelf || lineRenderer.positionCount == 0)
        {
            lineRenderer.enabled = false;
            return;
        }

        List<int> _level = eventArgs.levelList;

        for (int i = 0; i < _level.Count; i++) //遍歷指定的等級列表
        {
            if (level == _level[i]) //若符合其中一個指定等級則顯示之
            {
                lineRenderer.enabled = true; //如果是指定等級的線段則顯示
                isWorking = true; //可運作(中獎判斷依據)
                return;
            }
        }

        isWorking = false; //不可運作
        lineRenderer.enabled = false; //其他狀況則不顯示
    }

    //比照x列表與y列表若有其中一項元素是符合的則回傳True, 所有項目皆不符合則回傳False
    private bool ElementListMatchTest(List<ElementImageType> x, List<ElementImageType> y)
    {
        //比照兩個列表的元素
        for (int i = 0; i < x.Count; i++)
        {
            for (int j = 0; j < y.Count; j++)
            {
                if (x[i] == y[j]) return true;
            }
        }

        return false;
    }
}
