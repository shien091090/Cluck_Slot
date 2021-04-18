//測試工具
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class TestPanelManager : MonoBehaviour
{
    private static TestPanelManager _instance;
    public static TestPanelManager Instance { get { return _instance; } } //取得單例物件

    [System.Serializable]
    public struct TestPanelPage //分頁
    {
        public string title; //標題
        public GameObject pageGo; //分頁物件
    }

    [Header("可自訂參數")]
    public KeyCode callKey; //呼叫測試介面的按鍵
    public TestPanelPage[] panelSetting; //分頁設定

    [Header("遊戲進行狀態")]
    public int displayPanelIndex = 0; //顯示中分頁編號

    [Header("遊戲進行狀態(機率設定)")]
    public int totalAmount; //元素總數量
    public List<ProbabilitySettingUnit> probUnitList; //機率設定單位列表

    [Header("遊戲進行狀態(拉霸模擬)")]
    public bool slotSimulating = false; //拉霸模擬進行中
    public bool simulateEndingTag = false; //拉霸程序結束標記
    public List<TestSimulationUnit> testUnitList; //測試模擬單位列表
    public int levelMemo; //測試前紀錄等級

    [Header("參考物件")]
    public GameObject visualAreaGo; //測試介面物件
    public Text titleTxt; //標題文字
    public ConfirmWindow confirmWindowScript; //確認視窗腳本

    [Header("參考物件(機率設定)")]
    public Transform probUnitHolder; //機率設定單位父物件

    [Header("參考物件(拉霸模擬)")]
    public Transform testUnitHolder; //測試模擬單位父物件
    public InputField timesIf; //模擬次數輸入框
    public Button simulateButton; //模擬按鈕
    public Text startSimulateBtnText; //"開始模擬"按鈕文字

    [Header("Prefab")]
    public GameObject probabilitySettingUnit; //機率設定單位
    public GameObject testSimulationUnit; //測試模擬單位

    private ElementAttribute elementAttribute; //圖格元素設定

    public event System.EventHandler ElementCountCollected; //收集圖格元素數量

    //---------------------------------------------------------------------------------------------------------------------------------------------

    void Awake()
    {
        if (_instance == null) _instance = this; //設定單例模式
    }

    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(callKey)) //按下指定按鍵打開or關閉介面
        {
            if (visualAreaGo.activeSelf) //關閉介面
            {
                visualAreaGo.SetActive(false);
            }
            else //開啟介面
            {
                visualAreaGo.SetActive(true);
                SetTestPanel();
            }

        }
#endif
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------

#if UNITY_EDITOR
    //機率設定初始化
    public void ProbabilityInitialize()
    {
        if (ScrollManager.Instance.applyRoulette == null || ScrollManager.Instance.elementAttribute == null) return; //若未設定輪盤 或 未設定圖格元素, 則程序直接結束
        List<Roulette> _roulette = ScrollManager.Instance.applyRoulette; //取得輪盤
        elementAttribute = ScrollManager.Instance.elementAttribute; //取得圖格設定

        //機率設定物件創立
        if (probUnitList == null) probUnitList = new List<ProbabilitySettingUnit>(); //若列表不存在則初始化新物件

        for (int i = 0; i < elementAttribute.m_data.Count; i++)
        {
            if (probUnitList.Count < elementAttribute.m_data.Count) //組合項目物件不足時, 創建之
            {
                GameObject _go = Instantiate(probabilitySettingUnit, probUnitHolder);
                ProbabilitySettingUnit _pb = _go.GetComponent<ProbabilitySettingUnit>(); //取得腳本

                probUnitList.Add(_pb); //加入列表
            }

            ElementCountCollected += probUnitList[i].FeedbackAmount; //訂閱數量回報事件
        }

        for (int i = elementAttribute.m_data.Count; i < probUnitList.Count; i++) //暫時隱藏多餘的機率設定單元
        {
            probUnitList[i].Invalid();
        }

        ReloadProbabilitySetting(); //讀取機率設定
    }

    //拉霸模擬初始化
    public void SimulationInitialize()
    {
        //測試模擬物件創立
        startSimulateBtnText.text = "開始模擬"; //更改按鈕文字
        simulateButton.onClick.RemoveAllListeners(); //清空按鈕回調方法
        simulateButton.onClick.AddListener(SlotSimulate); //設定回調方法為"開始模擬"

        if (testUnitList == null) testUnitList = new List<TestSimulationUnit>(); //若列表不存在則初始化新物件

        for (int i = 0; i < BetController.Instance.lineLevelBlockList.Count; i++)
        {
            if (testUnitList.Count < BetController.Instance.lineLevelBlockList.Count) //測試模擬物件不足時, 創建之
            {
                GameObject _go = Instantiate(testSimulationUnit, testUnitHolder);
                TestSimulationUnit _simu = _go.GetComponent<TestSimulationUnit>(); //取得腳本

                testUnitList.Add(_simu); //加入列表
            }

            testUnitList[i].Initialize(i + 1, BetController.Instance.lineLevelBlockList[i].betMoney);
        }

        for (int i = BetController.Instance.lineLevelBlockList.Count; i < testUnitList.Count; i++) //暫時隱藏多餘的測試模擬設定單元
        {
            testUnitList[i].Invalid();
        }
    }

    //設定測試工具
    public void SetTestPanel()
    {
        for (int i = 0; i < panelSetting.Length; i++) //顯示頁面
        {
            if (i == displayPanelIndex) panelSetting[i].pageGo.SetActive(true);
            else panelSetting[i].pageGo.SetActive(false);
        }

        titleTxt.text = panelSetting[displayPanelIndex].title; //標題文字變更
    }

    //切換分頁
    //[param] dir = true : 往左 , false : 往右
    public void SwitchOverButton(bool dir)
    {
        //編號切換
        if (dir) displayPanelIndex = ( displayPanelIndex + 1 ) % panelSetting.Length;
        else displayPanelIndex = ( displayPanelIndex - 1 ) < 0 ? ( panelSetting.Length - 1 ) : ( displayPanelIndex - 1 );

        SetTestPanel(); //設定介面
    }

    //回傳模擬結果
    //[param] infoList = 有中獎的線段列表資訊
    public void FeedbackSimulateResult(List<PrizeLineInfo> infoList)
    {
        for (int i = 0; i < testUnitList.Count; i++) //遍歷模擬測試元件列表(每個元件代表一個等級)
        {
            List<PrizeLineInfo> _inputInfoList = new List<PrizeLineInfo>();

            for (int j = 0; j < infoList.Count; j++) //遍歷拉霸結果中每一條中獎線的中獎資訊
            {
                if (testUnitList[i].unitLevel >= infoList[j].lineLevel) _inputInfoList.Add(infoList[j]); //抽取指定等級的中獎線
            }

            testUnitList[i].TransferSlotData(_inputInfoList); //計算模擬測試結果
        }
    }

#endif
}
