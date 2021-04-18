//開頭選單流程控制腳本
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public partial class StartMenuManager : MonoBehaviour
{
    private static StartMenuManager _instance;
    public static StartMenuManager Instance { get { return _instance; } } //單例物件

    [Header("可自訂參數")]
    public float moneyPanelFadeDuration; //金錢面板移動動畫時間
    public Animator OpenningAnim; //開頭動畫
    public CanvasGroup buttonControllCg; //按鈕控制用

    [Header("參考物件")]
    public GameSettingPanel gameSettingPanel; //遊戲設定面板
    public MoneyCharge moneyChargeScript; //儲值遊戲腳本
    public RectTransform moneyPanel; //金錢面板
    public ConfirmWindow confirmWindow; //確認視窗腳本
    public RectTransform screenRect; //螢幕區域

    //---------------------------------------------------------------------------------------------------------------------------------------------

    void Awake()
    {
        if (_instance == null) _instance = this; //設定單例模式
    }

    void Start()
    {
        Initialize();
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------

    //金錢面板移動效果
    //[param] moveDir = 移動方向(true : 從上到下淡入 / false : 從下到上淡出)
    public void MoneyPanelMove(bool moveDir)
    {
        //if (!moneyChargeScript.moneyChargeBtn.interactable || !buttonControllCg.blocksRaycasts) return; //若儲值按鈕不可使用時, 則不會呼叫金錢面板

        float _targetY = ( moneyPanel.sizeDelta.y / 2 ) * ( moveDir ? -1 : 1 ); //目標Y軸位置

        moneyPanel.DOAnchorPosY(_targetY, moneyPanelFadeDuration).SetEase(Ease.OutQuart); //Y軸位置變化動畫
    }

    //初始化
    private void Initialize()
    {
        buttonControllCg.blocksRaycasts = false; //按鈕上鎖

        //ParticleEffectController.Instance.Initialize(); //粒子特效管理腳本初始化

        MoneyManager.Instance.Initialize(); //金錢腳本初始化

        StartCoroutine(Cor_Openning()); //執行開頭演出
    }

    //(協程)開頭演出
    private IEnumerator Cor_Openning()
    {
        Coroutine gateAnim = StartCoroutine(BarnGateBehavior.Instance.Cor_GateAnimation(true)); //撥放開門動畫

        yield return gateAnim;

        OpenningAnim.SetTrigger("Openning"); //撥放開頭選單動畫

        yield return new WaitUntil(() => ( OpenningAnim.GetCurrentAnimatorStateInfo(0).IsName("startMenu_openning") && OpenningAnim.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 )); //等待選單動畫撥放完畢

        buttonControllCg.blocksRaycasts = true; //按鈕解鎖

        AudioManagerScript.Instance.PlayAudioClip("BGM開頭");
    }

    private IEnumerator Cor_LoadScene()
    {
        buttonControllCg.blocksRaycasts = false; //按鈕上鎖

        Coroutine gateAnim = StartCoroutine(BarnGateBehavior.Instance.Cor_GateAnimation(false)); //撥放關門動畫

        yield return gateAnim;

        SceneManager.LoadScene("Main");
    }
}
