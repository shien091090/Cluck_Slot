//遊戲流程控制

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    private static GameController _instance;
    public static GameController Instance { get { return _instance; } } //取得單例物件

    [Header("遊戲進行狀態")]
    public int nowSlotLevel; //目前拉霸等級
    public bool leverCanUse; //拉霸可操控
    public bool leverCanSnap; //玩家可否手動中斷捲軸
    public bool leverAnimPlaying; //拉霸動畫撥放中(不可操作)

    [Header("參考物件")]
    //public Animator barnGateAnim; //過場門動畫
    public CanvasGroup[] uiLockGroups; //UI鎖定物件群組
    public ConfirmWindow confirmWindow; //確認視窗

    //---------------------------------------------------------------------------------------------------------------------------------------------

    void Awake()
    {
        if (_instance == null) _instance = this; //設定單例模式
    }

    void Start()
    {
        GameInitialize();
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------

    //拉霸
    public void PullLever()
    {
        if (leverCanUse) //拉桿可控制時
        {
            SetOperationState(false); //操作狀態設為禁止

            SlotLevel _nowSlot = ScrollManager.Instance.slotLevelLayoutSetting[nowSlotLevel]; //取得拉霸等級設定
            SpinModePack _spinModeList = ListExtensibleScript<SpinModePack>.RandomReturn(_nowSlot.spinModeSetting); //取得旋轉模式列表

            ScrollManager.Instance.Spin(_spinModeList, _nowSlot.linkTime);
            //ScrollManager.Instance.Spin(true, 12, 2f, 0.6f);
        }
        else if (leverCanSnap) //捲軸旋轉中, 手動中斷捲軸
        {
            //當拉霸動畫尚未撥放完畢時, 禁止操作拉霸
            AnimatorStateInfo _leverAnimInfo = ScrollManager.Instance.leverAnim.GetCurrentAnimatorStateInfo(0);
            if (( _leverAnimInfo.IsName("lever_spin") || _leverAnimInfo.IsName("lever_snap") ) && _leverAnimInfo.normalizedTime <= 1) return;

            if (ScrollBehavior.s_scrollRollingStates != null && ScrollBehavior.s_scrollRollingStates.Length > 0)
            {
                for (int i = 0; i < ScrollBehavior.s_scrollRollingStates.Length; i++) //當有任何捲軸處於旋轉狀態時, 操控拉霸依序中斷旋轉
                {
                    if (ScrollBehavior.s_scrollRollingStates[i])
                    {
                        AudioManagerScript.Instance.PlayAudioClip("SE捲軸中斷");

                        ScrollManager.Instance.leverAnim.Play("lever_snap", 0, 0); //撥放捲軸中斷動畫
                        ScrollBehavior.s_scrollRollingStates[i] = false;
                        break;
                    }
                }
            }
        }
    }

    //設定玩家操作狀態
    public void SetOperationState(bool onOff)
    {
        leverCanUse = onOff; //設定拉霸操作
        BetController.Instance.SetOperationLockOut(onOff); //設定下注操作
        ScrollManager.Instance.SetLeverHintEffect(onOff); //設定拉霸提示效果
    }

    //UI鎖定狀態設定
    public void SetUILockState(bool b)
    {
        for (int i = 0; i < uiLockGroups.Length; i++)
        {
            uiLockGroups[i].blocksRaycasts = b;
        }
    }

    //返回按鈕
    public void ReturnToMenu()
    {
        UnityEngine.Events.UnityAction Return = () =>
        {
            AudioManagerScript.Instance.Stop(0); //背景音樂停止

            StartCoroutine(Cor_LoadScene());
        };

        confirmWindow.WindowPopup("是否返回主選單？", Return);
    }

    private IEnumerator Cor_LoadScene()
    {
        Coroutine gateAnim = StartCoroutine(BarnGateBehavior.Instance.Cor_GateAnimation(false)); //撥放關門動畫

        yield return gateAnim;

        SceneManager.LoadScene("StartMenu");
    }

    //遊戲初始化(重置遊戲)
    private void GameInitialize()
    {
        SetOperationState(false); //操作狀態設為禁止
        leverCanSnap = false;

        nowSlotLevel = PlayerPrefs.GetInt("GAME_SLOTLEVEL", 0); //拉霸等級初始化

        SlotLevel _defaultSlot = ScrollManager.Instance.slotLevelLayoutSetting[nowSlotLevel]; //取得預設拉霸等級設定資訊

        BetController.Instance.Initialize(); //下注控制腳本初始化

        LotteryLineSetting _lineSetting = _defaultSlot.lineSetting;
        StartCoroutine(ScrollManager.Instance.Cor_SetScrollPanel(ScrollManager.Instance.defaultElementChain, false, _lineSetting)); //設置捲軸區域UI

        PrizeTableSetting _prizeTable = _defaultSlot.prizeTable; //取得初始支付表
        PrizeTableManager.Instance.Initiailze(_prizeTable); //支付表初始化

        ScrollManager.Instance.RouletteInitialize(); //輪盤初始化

        MoneyManager.Instance.Initialize(); //金錢管理腳本初始化

        SpinLevelPanel.Instance.Initialize(); //拉霸等級介面初始化

#if UNITY_EDITOR
        TestPanelManager.Instance.ProbabilityInitialize(); //測試工具初始化 : 機率設定
        TestPanelManager.Instance.SimulationInitialize(); //測試工具初始化 : 拉霸模擬
#endif

        StartCoroutine(Cor_GameStart());
    }

    //(協程)遊戲開始過場
    private IEnumerator Cor_GameStart()
    {
        yield return new WaitForEndOfFrame();

        AudioManagerScript.Instance.CoverPlayAudioClip("BGM主要");

        Coroutine gateAnim = StartCoroutine(BarnGateBehavior.Instance.Cor_GateAnimation(true)); //撥放開門動畫

        yield return gateAnim;

        SetOperationState(true); //操作狀態設為允許
    }
}
