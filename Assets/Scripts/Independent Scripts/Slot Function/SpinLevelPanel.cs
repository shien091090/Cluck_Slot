//拉霸等級解鎖介面
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SpinLevelPanel : MonoBehaviour
{
    private static SpinLevelPanel _instance;
    public static SpinLevelPanel Instance { get { return _instance; } } //取得單例物件

    [System.Serializable]
    public struct SpinLevel //拉霸等級
    {
        public int nextLevel; //下一等級
        public int unlockMoney; //解鎖累積金額
    }

    [Header("可自訂參數")]
    public List<SpinLevel> spinLevelSetting; //拉霸等級設定
    public float sliderVariationDuration; //拉條變化時間

    [Header("遊戲進行狀態")]
    //public int nowLevelIndex; //目前等級索引
    public int sumPrize; //累積獎金

    [Header("參考物件")]
    public GameObject spinLevelPanel; //拉霸等級介面物件
    public Slider progressBar; //進度條Slider
    public Text unlockMoneyTxt; //解鎖金額Text
    public Text nextLevelTxt; //下一等級Text
    private CanvasGroup panelCg;
    private Transform panelTf;

    //---------------------------------------------------------------------------------------------------------------------------------------------

    void Awake()
    {
        if (_instance == null) _instance = this; //設定單例模式
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------

    //初始化
    public void Initialize()
    {
        if (spinLevelSetting == null || spinLevelSetting.Count == 0) throw new System.Exception("[ERROR]拉霸等級未設定");

        if (panelCg == null) panelCg = spinLevelPanel.GetComponent<CanvasGroup>();
        if (panelTf == null) panelTf = spinLevelPanel.GetComponent<Transform>();

        //自動排序拉霸等級設定
        List<SpinLevel> _spinLevelSetting = new List<SpinLevel>();
        _spinLevelSetting.AddRange(spinLevelSetting);

        _spinLevelSetting.Sort((SpinLevel x, SpinLevel y) => //由等級低至高排序
        {
            if (x.nextLevel > y.nextLevel) return 1;
            else if (x.nextLevel < y.nextLevel) return -1;
            return 0;
        });

        spinLevelSetting = _spinLevelSetting;

        //參數初始化
        sumPrize = PlayerPrefs.GetInt("GAME_SUMPRIZE", 0);
        progressBar.value = PlayerPrefs.GetFloat("GAME_SPINLEVELPROGRESS", 0f);
        panelCg.alpha = 1;
        panelTf.localPosition = Vector2.zero;

        //顯示初始化
        if (GameController.Instance.nowSlotLevel < spinLevelSetting.Count) //已經是最高等級時, 跳過顯示初始化
        {
            unlockMoneyTxt.text = spinLevelSetting[GameController.Instance.nowSlotLevel].unlockMoney.ToString() + " $"; //剩餘解鎖獎金
            nextLevelTxt.text = spinLevelSetting[GameController.Instance.nowSlotLevel].nextLevel.ToString(); //下一等級
        }

    }

    //解鎖拉霸等級
    private IEnumerator Cor_SpinLevelUp(int level)
    {
        SlotLevel _levelData = ScrollManager.Instance.slotLevelLayoutSetting[level]; //取得下一個拉霸等級設定資訊

        ScrollManager.Instance.StopAllElementAnimation(); //停止所有圖格動畫

        AudioManagerScript.Instance.PlayAudioClip("SE等級上升");

        yield return StartCoroutine(ScrollManager.Instance.Cor_SetScrollPanel(_levelData.scrollCount, _levelData.visibleCount, _levelData.elementSpacing, true, _levelData.lineSetting)); //UI布局變動

        if (_levelData.activeBetLevel > BetController.Instance.nowUnlockLevel) BetController.Instance.UnlockLevel(_levelData.activeBetLevel); //下注等級解鎖

        PrizeTableManager.Instance.Initiailze(_levelData.prizeTable); //設定新的支付表

        ScrollManager.Instance.applyRoulette = _levelData.rouletteData.OutputRouletteList(); //設定新的輪盤
#if UNITY_EDITOR
        TestPanelManager.Instance.ReloadProbabilitySetting(); //重新讀取輪盤設定至測試介面
#endif

        PlayerPrefs.SetInt("GAME_SLOTLEVEL", level);

        yield return new WaitForSeconds(0.5f);
    }

    //(協程)累計解鎖獎金
    public IEnumerator Cor_AccumulateUnlockPrize(int money)
    {
        if (GameController.Instance.nowSlotLevel >= spinLevelSetting.Count) yield break; //拉霸等級已經是最高等級時, 直接結束程序

        GameController gc = GameController.Instance;

        int total = sumPrize + money; //累加後金額
        int unlockTargetMoney = spinLevelSetting[gc.nowSlotLevel].unlockMoney; //解鎖目標金額

        //設定顯示狀態
        unlockMoneyTxt.text = ( unlockTargetMoney - sumPrize ).ToString() + " $"; //再累積多少錢可以解鎖
        nextLevelTxt.text = spinLevelSetting[gc.nowSlotLevel].nextLevel.ToString(); //下一等級

        //相關物件初始化
        panelCg.alpha = 1;
        panelTf.localPosition = Vector2.zero;

        //介面淡入動畫
        if (!spinLevelPanel.activeSelf) spinLevelPanel.SetActive(true);

        Sequence sq_panelFadeIn = DOTween.Sequence() //淡入動畫
           .Append(panelCg.DOFade(0, 0.5f).From())
           .Join(panelTf.DOLocalMoveY(panelTf.localPosition.y - 70, 0.7f).From())
           .SetEase(Ease.OutQuart)
           .OnStart(() => { GameController.Instance.SetUILockState(false); });

        Sequence sq_panelFadeOut = DOTween.Sequence() //淡出動畫(保存用, 不撥放)
            .Append(panelCg.DOFade(0, 0.4f))
            .Insert(0, panelTf.DOLocalMoveY(panelTf.localPosition.y - 70, 0.4f))
            .SetEase(Ease.OutQuart)
            .OnComplete(() =>
            {
                spinLevelPanel.SetActive(false);
                GameController.Instance.SetUILockState(true);
            })
            .Pause();

        while (total >= unlockTargetMoney) //若累加金額超過解鎖目標金額
        {
            //升級動畫
            Sequence sq_levelUp = DOTween.Sequence()
                .Append(progressBar.DOValue(1f, sliderVariationDuration))
                .Join(DOTween.To(() => sumPrize, x => sumPrize = x, unlockTargetMoney, sliderVariationDuration)
                .OnUpdate(() => { unlockMoneyTxt.text = ( unlockTargetMoney - sumPrize ).ToString() + " $"; }))
                .SetEase(Ease.InOutQuint);

            AudioManagerScript.Instance.PlayAudioClip("SE等級條");

            yield return sq_levelUp.WaitForCompletion();

            panelCg.DOFade(0.5f, 0.5f).SetEase(Ease.OutCubic); //介面稍微淡出

            //升級回調程序
            yield return StartCoroutine(Cor_SpinLevelUp(spinLevelSetting[gc.nowSlotLevel].nextLevel)); //拉霸等級提升

            total -= spinLevelSetting[gc.nowSlotLevel].unlockMoney; //重置累加金額
            gc.nowSlotLevel++;

            if (gc.nowSlotLevel >= spinLevelSetting.Count) //已經升到最高等級的狀況, 直接結束程序
            {
                sq_panelFadeOut.Restart();

                yield return sq_panelFadeOut.WaitForCompletion();

                PlayerPrefs.SetInt("GAME_SUMPRIZE", unlockTargetMoney);
                PlayerPrefs.SetFloat("GAME_SPINLEVELPROGRESS", 1f);

                yield break;
            }

            unlockTargetMoney = spinLevelSetting[gc.nowSlotLevel].unlockMoney; //解鎖目標金額
            sumPrize = 0; //累積獎金歸零
            progressBar.value = 0; //進度條歸零

            panelCg.DOFade(1, 0.3f).SetEase(Ease.OutCubic); //介面淡入回復
            nextLevelTxt.text = spinLevelSetting[gc.nowSlotLevel].nextLevel.ToString(); //下一等級
        }

        float _sliderTargetValue = Mathf.Clamp((float)total / (float)spinLevelSetting[gc.nowSlotLevel].unlockMoney, 0f, 1f); //目標Slider值

        //進度條動畫
        Sequence sq_sliderVariation = DOTween.Sequence()
            .Append(progressBar.DOValue(_sliderTargetValue, sliderVariationDuration))
            .Join(DOTween.To(() => sumPrize, x => sumPrize = x, total, sliderVariationDuration)
            .OnUpdate(() => { unlockMoneyTxt.text = ( unlockTargetMoney - sumPrize ).ToString() + " $"; }))
            .SetEase(Ease.InOutQuint);

        AudioManagerScript.Instance.PlayAudioClip("SE等級條");

        yield return sq_sliderVariation.WaitForCompletion();

        //Debug.Log("進度條動畫結束");

        PlayerPrefs.SetInt("GAME_SUMPRIZE", total);
        PlayerPrefs.SetFloat("GAME_SPINLEVELPROGRESS", _sliderTargetValue);

        sq_panelFadeOut.Restart();

        yield return sq_panelFadeOut.WaitForCompletion();
    }
}
