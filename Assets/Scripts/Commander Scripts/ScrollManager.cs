//捲軸控制管理腳本
//※負責捲軸的程式邏輯
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public partial class ScrollManager : MonoBehaviour
{
    private static ScrollManager _instance;
    public static ScrollManager Instance { get { return _instance; } } //取得單例物件

    [Header("可自訂參數(Layout)")]
    public int visibleElementCount; //可視圖格數量
    public int scrollCount; //捲軸數量(條數)
    public float scrollMinWidth; //捲軸最小寬度
    public float scrollPrefrerrdWidth; //捲軸優先寬度
    public Vector2 elementSpacing; //圖格間隔
    public float layoutAnimationDuration; //UI布局變換動畫時間

    [Header("可自訂參數(Spin)")]
    public float brakeBufferTime; //剎車緩衝時間(捲軸煞停時的強制等待時間)
    public float scrollSoundFrequency; //捲軸音效頻率(越低則越快)
    public AnimationCurve scrollStartingCurve; //捲軸旋轉起步動作曲線(進入正式旋轉前捲軸的旋轉加速度曲線, X軸為時間 ; Y軸為0~1的值, 1代表終端速度)
    public List<SlotLevel> slotLevelLayoutSetting; //拉霸等級設定

    [Header("可自訂參數(Effect)")]
    public float leverHintWaitingTime; //拉霸提示特效等待時間

    [Header("可自訂參數(Test)")]
    public bool layoutUpdateAnimation; //UI布局更新時是否使用動畫
    public KeyCode paramUpdateKey; //更新參數按鍵(測試用)

    [Header("遊戲進行狀態")]
    public List<ScrollBehavior> scrollGroup; //捲軸集合
    public List<ElementImageType> defaultElementChain; //預設圖格鍊
    public List<PositionSensorBehavior> posSensorArray; //位置偵測器陣列
    public List<Roulette> applyRoulette; //套用中輪盤
    public List<SensorInfo> previousSpinResult; //上一次的拉霸結果

    [Header("參考物件")]
    public Transform scrollHolder; //捲軸父物件
    public Transform posSensorHolder; //位置偵測器父物件
    public ElementAttribute elementAttribute; //圖格元素設定(Scriptable Object)
    public Transform anchor_leftDown; //定位點(左下)
    public Transform anchor_rightUp; //定位點(右上)
    public LayoutData layoutData; //UI布局資料儲存
    public Animator leverAnim; //拉桿動畫
    public Transform coinCollectionTargetRect; //金幣收集目標

    [Header("Prefab")]
    public GameObject elementPrefab; //圖格預置物
    public GameObject posSensorPrefab; //位置偵測器預置物
    public GameObject scrollPrefab; //捲軸預置物

    public Dictionary<ElementImageType, ElementAttributeData> Dict_elementAttribute { private set; get; } //(字典)從圖格類型查找圖格設定資料
    public Dictionary<ElementImageType, bool> Dict_wildSymbol { private set; get; } //(字典)圖格類型是否為狂野符號
    public Dictionary<Vector2, Vector2> Dict_cdnToWorldPos { private set; get; } //(字典)從圖格座標編號查找圖格世界位置

    public event System.EventHandler<PositionSensorEventArgs> SpinJudged; //旋轉位置判定事件
    public event System.EventHandler<ElementAnimationEventArgs> ElementAnimationPlayed; //圖格動畫撥放事件

    //---------------------------------------------------------------------------------------------------------------------------------------------

    void Awake()
    {
        if (_instance == null) _instance = this; //設定單例模式

        //字典建立
        Dict_elementAttribute = new Dictionary<ElementImageType, ElementAttributeData>();
        Dict_wildSymbol = new Dictionary<ElementImageType, bool>();
        for (int i = 0; i < elementAttribute.m_data.Count; i++)
        {
            Dict_elementAttribute.Add(elementAttribute.m_data[i].imgType, elementAttribute.m_data[i]);
            Dict_wildSymbol.Add(elementAttribute.m_data[i].imgType, ( elementAttribute.m_data[i].property == ElementProperty.狂野符號 ? true : false ));
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(paramUpdateKey))
        {
            StartCoroutine(Cor_SetScrollPanel(scrollCount, visibleElementCount, elementSpacing, layoutUpdateAnimation));
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------

    //捲軸旋轉(多載1/2) ※統一指定旋轉模式
    //[param] direction = true : 往下轉, false : 往上轉, elementSpeed = 速度(每秒幾張圖格(會算上間隔)), spinTime = 旋轉時間(時間結束後才會抽選欲轉到的值), linkTime = 牽動等待時間
    public void Spin(bool direction, float elementSpeed, float spinTime, float linkTime)
    {
        List<SpinMode> spinModeList = new List<SpinMode>() { new SpinMode(direction, elementSpeed, spinTime) };

        StartCoroutine(Cor_BeginSpin(spinModeList, linkTime, false));
    }

    //捲軸旋轉(多載2/2) ※個別指定旋轉模式
    public void Spin(SpinModePack spinModeList, float linkTime)
    {
        StartCoroutine(Cor_BeginSpin(spinModeList.spinModePack, linkTime, false));
    }

    //(協程)捲軸模擬測試
    public IEnumerator Cor_SpinTest()
    {
        yield return StartCoroutine(Cor_BeginSpin(null, 0, true));
    }

    //建立圖格鍊(僅回傳一個Enum List)
    //[param] isCover = 是否覆蓋儲存至共用圖格鍊
    //public List<ElementImageType> BuildElementList(bool isCover)
    //{
    //    List<ElementImageType> _sourceList = new List<ElementImageType>(); //來源列表(抽取用)
    //    List<ElementImageType> _outputList = new List<ElementImageType>(); //輸出列表

    //    for (int i = 0; i < elementAttribute.m_data.Count; i++) //遍歷所有種類圖格
    //    {
    //        for (int j = 0; j < elementAttribute.m_data[i].amount; j++) //照各種類之設定數量重複執行
    //        {
    //            _sourceList.Add(elementAttribute.m_data[i].imgType);
    //        }
    //    }

    //    RandomExtract(_sourceList, ref _outputList); //打亂排序

    //    //TestMessageScript<ElementImageType>.PrintListElements("[BuildElementList]", _outputList);

    //    if (isCover) defaultElementChain = _outputList; //覆蓋儲存至共用圖格鍊
    //    return _outputList;
    //}

    //圖格動畫撥放
    //[param] animationType = 動畫類型 , targetPosList = 指定撥放動畫的座標圖格 , onOff = 開關
    public void PlayElementAnimation(ElementAnimationType animationType, List<Vector2> targetPosList, bool onOff)
    {
        ElementAnimationEventArgs e = new ElementAnimationEventArgs(animationType, targetPosList, onOff);
        if (ElementAnimationPlayed != null) ElementAnimationPlayed.Invoke(this, e);
    }

    //停止所有圖格動畫撥放
    public void StopAllElementAnimation()
    {
        PlayElementAnimation(ElementAnimationType.全部動畫停止, null, true);
    }

    //隨機圖格設定初始化
    public void RouletteInitialize()
    {
        int _nowLevel = GameController.Instance.nowSlotLevel; //取得目前拉霸等級
        List<Roulette> _nowRoulette = slotLevelLayoutSetting[_nowLevel].rouletteData.OutputRouletteList(); //取得輪盤

        if (_nowRoulette == null) throw new System.Exception("[ERROR]沒有設定該拉霸等級的輪盤");
        applyRoulette = new List<Roulette>();
        applyRoulette.AddRange(_nowRoulette); //設定輪盤
    }

    //從列表中迭代抽取元素, 打亂List排序
    private void RandomExtract(List<ElementImageType> extractList, ref List<ElementImageType> collectList)
    {
        if (extractList.Count == 0) return; //當抽取列表為空時結束程序

        int _dice = Random.Range(0, extractList.Count); //從List長度取一隨機數

        ElementImageType _type = extractList[_dice]; //抽取元素
        collectList.Add(_type); //匯集至目標列表
        extractList.RemoveAt(_dice); //消除抽取位置

        RandomExtract(extractList, ref collectList); //迭代呼叫
    }

    //建立隨機旋轉結果
    private List<List<ElementImageType>> BuildResultScroll()
    {
        List<List<ElementImageType>> _result = new List<List<ElementImageType>>();

        //建立圖格總數列表
        List<ElementImageType> _elementTotalList = new List<ElementImageType>();
        for (int i = 0; i < applyRoulette.Count; i++)
        {
            for (int j = 0; j < applyRoulette[i].amount; j++)
            {
                _elementTotalList.Add(applyRoulette[i].type);
            }
        }

        //string _t = "";
        //for (int i = 0; i < _elementTotalList.Count; i++)
        //{
        //    _t += _elementTotalList[i] + ( i == _elementTotalList.Count - 1 ? "" : ", " );
        //}
        //Debug.Log("完整列表 : " + _t);

        for (int i = 0; i < scrollCount; i++) //依捲軸數量建立結果的欄數
        {
            List<ElementImageType> _randomList = new List<ElementImageType>(); //抽取結果

            List<ElementImageType> _list = new List<ElementImageType>(); //抽取來源
            _list.AddRange(_elementTotalList);

            for (int j = 0; j < visibleElementCount; j++) //依可視圖格數量隨機建立圖格
            {
                //抽取圖格
                int _random = Random.Range(0, _list.Count);
                _randomList.Add(_list[_random]);
                _list.RemoveAt(_random);
            }

            _result.Add(_randomList); //增加一欄捲軸
        }

        //for (int i = 0; i < _result.Count; i++)
        //{
        //    string _s = "";
        //    for (int j = 0; j < _result[i].Count; j++)
        //    {
        //        _s += _result[i][j] + ( j == _result[i].Count - 1 ? "" : ", " );
        //    }

        //    Debug.Log(string.Format("[{0}] : {1}", i, _s));
        //}

        return _result;
    }

    //捲軸旋轉(協程)
    //[param] spinModeList = 旋轉模式 , linkTime = 牽動等待時間 , isTest = 測試與否
    private IEnumerator Cor_BeginSpin(List<SpinMode> spinModeList, float linkTime, bool isTest)
    {
        if (!isTest) //測試的狀況跳過
        {
            if (spinModeList.Count != scrollCount && spinModeList.Count != 1) throw new System.Exception("[ERROR]輸入的旋轉模式設定有誤!");
            
            //旋轉前準備
            if (MoneyManager.Instance.nowMoney < BetController.Instance.nowBetMoney)
            {
                AudioManagerScript.Instance.PlayAudioClip("SE取消");
                Coroutine moneyNotEnough = StartCoroutine(MoneyManager.Instance.Cor_MoneyNotEnough()); //錢不夠提示

                yield return moneyNotEnough;

                GameController.Instance.SetOperationState(true); //操作狀態設為允許

                yield break;
            }

            leverAnim.Play("lever_spin", 0, 0); //撥放動畫

            AudioManagerScript.Instance.PlayAudioClip("SE拉桿");

            StartCoroutine(MoneyManager.Instance.Cor_SetMoney(MoneyManager.Instance.nowMoney - BetController.Instance.nowBetMoney, false)); //扣除賭金

            StopAllElementAnimation(); //停止所有圖格動畫
        }

        //預估旋轉結果
        List<List<ElementImageType>> spinResult = BuildResultScroll(); //旋轉結果

        List<SensorInfo> blocksResult = new List<SensorInfo>(); //將旋轉結果的圖格列表形式轉換成捲軸偵測器形式
        for (int i = 0; i < posSensorArray.Count; i++)
        {
            SensorInfo _info = new SensorInfo(); //建立偵測器
            _info.coordinate = posSensorArray[i].coordinate;
            _info.elementType = spinResult[(int)posSensorArray[i].coordinate.x][i % visibleElementCount];

            blocksResult.Add(_info); //加入列表
        }

        //Debug.Log("BlocksResult Test -------------");
        //for (int i = 0; i < blocksResult.Count; i++)
        //{
        //    Debug.Log(string.Format("({0}, {1}) = {2}", blocksResult[i].coordinate.x, blocksResult[i].coordinate.y, blocksResult[i].elementType));
        //}

        List<PrizeLineInfo> prizeLineDistribution = new List<PrizeLineInfo>(); //中獎資訊列表
        int prizeTotal = 0; //總獎金

        List<LineDrawer> _workingLineList = LotteryLineManager.Instance.GetWorkingLines(); //取得有效線段
        List<Vector2> _drawedPosList = new List<Vector2>(); //中獎圖格座標列表

        for (int i = 0; i < _workingLineList.Count; i++) //遍歷所有線段是否中獎
        {
            PrizeLineInfo _pInfo = _workingLineList[i].RewardJudgement(blocksResult, PrizeTableManager.Instance.prizeTable.m_data); //判斷該線段中獎資訊, 並將其儲存
            _drawedPosList.AddRange(_pInfo.drawedPosList); //加入中獎圖格座標列表

            if (_pInfo.sumPrize > 0) //有中獎時
            {
                prizeTotal += _pInfo.sumPrize; //總獎金累計
            }

            prizeLineDistribution.Add(_pInfo);
        }

        //Debug.Log("總計中獎獎金 = " + prizeTotal);
        //for (int i = 0; i < prizeLineDistribution.Count; i++)
        //{
        //    Debug.Log(string.Format("等級 {0} 合計中獎獎金 = {1}", prizeLineDistribution[i].lineLevel, prizeLineDistribution[i].sumPrize));
        //}

#if UNITY_EDITOR
        if (isTest) //測試的狀況
        {
            TestPanelManager.Instance.FeedbackSimulateResult(prizeLineDistribution); //儲存模擬結果

            yield break;
        }
#endif

        _drawedPosList = ListExtensibleScript<Vector2>.RepetitionFilter(_drawedPosList); //篩掉列表中的重複元素

        //捲軸旋轉狀態
        bool[] _stateArr = new bool[scrollCount];
        for (int i = 0; i < _stateArr.Length; i++)
        {
            _stateArr[i] = false;
        }

        ScrollBehavior.s_scrollRollingStates = _stateArr; //初始化旋轉狀態
        GameController.Instance.leverCanSnap = true; //可手動中斷捲軸

        //捲軸旋轉動畫
        //float _delayTime = 0; //捲軸煞停延遲時間
        for (int i = 0; i < scrollCount; i++) //逐個捲軸執行旋轉動作
        {
            int _index = 0;
            if (spinModeList.Count > 1) _index = i; //若為統一旋轉模式則List索引固定為0, 若個別指定則索引值隨迴圈變動

            bool _dir = spinModeList[_index].direction; //旋轉方向
            float _speed = spinModeList[_index].elementSpeed; //旋轉速度
            float _time = spinModeList[_index].spinTime; //旋轉時間

            float _sliderSpeed = ( ( _dir ? _speed : -_speed ) / ( scrollGroup[i].elementChain.Count - visibleElementCount ) ) * Time.fixedDeltaTime; //換算速度(圖格速度 ➡ 拉條速度)

            StartCoroutine(scrollGroup[i].Cor_ScrollSpin(_sliderSpeed, _time, spinResult[i], scrollStartingCurve));

            if (i == scrollCount - 1) //最後一個捲軸動作時, 紀錄煞停延遲時間
            {
                //_delayTime = ( (float)( visibleElementCount + 1 ) / ( scrollGroup[i].elementChain.Count - visibleElementCount ) ) / ( Mathf.Abs(_sliderSpeed) * ( 1f / Time.fixedDeltaTime ) );
                //Debug.Log("SliderSpeed(/s) = " + ( ( Mathf.Abs(_sliderSpeed) * ( 1f / Time.fixedDeltaTime ) ) ));
                //Debug.Log("Vc = " + visibleElementCount);
                //Debug.Log("t - Vc = " + ( scrollGroup[i].elementChain.Count - visibleElementCount ));
                //Debug.Log("Vc / (t - Vc) = " + ( (float)visibleElementCount / ( scrollGroup[i].elementChain.Count - visibleElementCount ) ));
                //Debug.Log("DelayTime = " + _delayTime);

                break;
            }

            yield return new WaitForSeconds(linkTime);
        }

        for (int i = 0; i < scrollCount; i++) //等待所有捲軸旋轉完畢
        {
            yield return new WaitWhile(() => scrollGroup[i].isRolling);
        }

        yield return new WaitForSeconds(brakeBufferTime);

        GameController.Instance.leverCanSnap = false; //禁止手動中斷捲軸

        PositionSensorEventArgs eventArgs = new PositionSensorEventArgs(visibleElementCount * scrollCount);
        if (SpinJudged != null) SpinJudged.Invoke(this, eventArgs);

        yield return new WaitUntil(() => ( eventArgs.overNumber == 0 )); //等待所有圖格位置偵測器觸發結束

        //for (int i = 0; i < eventArgs.SpinResultList.Count; i++)
        //{
        //    Debug.Log(string.Format("[{0}] ({1}, {2}) : {3}", i, eventArgs.SpinResultList[i].coordinate.x, eventArgs.SpinResultList[i].coordinate.y, eventArgs.SpinResultList[i].elementType));
        //}

        //旋轉結果
        previousSpinResult = eventArgs.SpinResultList; //記錄拉霸結果

        PlayElementAnimation(ElementAnimationType.中獎, _drawedPosList, true); //中獎圖格特效

        yield return StartCoroutine(MoneyManager.Instance.Cor_GetPrize(prizeTotal, prizeLineDistribution)); //獲得獎金

        GameController.Instance.SetOperationState(true); //操作狀態設為允許
    }
}