//下注控制
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class BetController : MonoBehaviour
{
    private static BetController _instance;
    public static BetController Instance { get { return _instance; } } //取得單例物件

    [System.Serializable]
    public struct LotteryLineSettingTable //中獎線設定集對照表
    {
        public string name; //設定集的名稱
        public LotteryLineSetting setting; //設定集(ScriptableObject)
    }

    [System.Serializable]
    public struct LineLevelSetting
    {
        public LineLevelBlock lineLevelBlock; //中獎等級方塊
        public int betMoney; //賭注金額
    }

    [Header("可自訂參數")]
    public List<LineLevelSetting> lineLevelBlockList; //中獎線等級方塊設定列表
    public Color lockingStyle; //未解鎖顏色
    public float alphaChangeSpeed; //透明度增減速度
    public float alphaFloor; //透明度下限

    [Header("中獎線設定")]
    public float destroyDelayTime; //延遲銷毀時間(秒)
    public List<LotteryLineSettingTable> LotteryLineSettingGroup; //中獎線設定集對照表

    [Header("參考物件")]
    public Slider betSlider; //下注拉桿
    public Button[] betButton; //按鈕(增&減)
    public CanvasGroup canvasGroup; //透明度控制用
    public Text betMoneyLabel; //賭注金額文字
    public GameObject par_levelBlockActive; //等級方塊粒子特效

    [Header("遊戲進行狀態")]
    public int nowUnlockLevel; //目前解鎖等級
    public bool isPointerEnter = false; //滑鼠移入與否(true = 移入 , false = 移出)
    public int nowBetMoney; //目前賭注金額

    public int MaxLevel { private set; get; } //最高等級

    //---------------------------------------------------------------------------------------------------------------------------------------------

    void Awake()
    {
        if (_instance == null) _instance = this; //設定單例模式
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------

    //初始化
    public void Initialize()
    {
        MaxLevel = lineLevelBlockList.Count; //設定最高等級

        betSlider.maxValue = MaxLevel; //設定下注拉桿

        nowBetMoney = lineLevelBlockList[(int)betSlider.value - 1].betMoney; //設定目前下注等級的賭注金額
        betMoneyLabel.text = nowBetMoney + MoneyManager.Instance.moneyLabelSuffix; //賭注金額顯示

        StopAllCoroutines();
        StartCoroutine(Cor_AlphaChange()); //UI透明度變化

        for (int i = 0; i < lineLevelBlockList.Count; i++) //設置各等級方塊的激活特效
        {
            string _levelLabel = "Par_LineLevelBlock" + ( i + 1 );

            lineLevelBlockList[i].lineLevelBlock.levelLabel = _levelLabel; //設定等級方塊標籤(呼叫特效用)
            ParticleEffectController.Instance.AddStaticEffect(_levelLabel, par_levelBlockActive, lineLevelBlockList[i].lineLevelBlock.transform); //加入特效列表
        }

        UnlockLevel(); //等級解鎖(使用預設等級)
    }

    //等級解鎖(多載1/2) ※指定等級
    //[param] level = 解鎖等級(若輸入3, 則1~3都會解鎖)
    public void UnlockLevel(int level)
    {
        int _level = level > MaxLevel ? MaxLevel : level; //解鎖等級不可超過最高等級

        nowUnlockLevel = _level; //設定目前解鎖等級

        //設定下注拉桿限制
        if (betSlider.value > _level) betSlider.value = _level;

        //設定中獎線等級方塊的顏色樣式
        for (int i = 0; i < lineLevelBlockList.Count; i++)
        {
            if (i < _level) //已解鎖樣式
            {
                if (i < betSlider.value) lineLevelBlockList[i].lineLevelBlock.SetState(LineLevelBlockStyle.激活顏色);
                else lineLevelBlockList[i].lineLevelBlock.SetState(LineLevelBlockStyle.未激活顏色);
            }
            else lineLevelBlockList[i].lineLevelBlock.SetState(LineLevelBlockStyle.未解鎖顏色); //未解鎖樣式
        }
    }

    //等級解鎖(多載2/2) ※使用初始等級
    public void UnlockLevel()
    {
        int _nowLevel = GameController.Instance.nowSlotLevel; //取得目前拉霸等級
        int _unlockLevel = ScrollManager.Instance.slotLevelLayoutSetting[_nowLevel].activeBetLevel; //取得欲解鎖等級

        UnlockLevel(_unlockLevel);
    }

    //下注(多載1/2) ※透過Button On Click呼叫
    public void Bet(bool plusMinus)
    {
        float _value = betSlider.value;

        if (plusMinus) //"加"的情況
        {
            if (_value >= MaxLevel) return; //如果已經是最高等級, 則直接結束程序
            else _value += 1;
        }
        else //"減"的情況
        {
            if (_value <= 1) return; //如果已經是1, 則直接結束程序
            else _value -= 1;
        }

        Bet(_value, true);
    }

    //下注(多載2/2) ※透過Slider On Value Changed呼叫
    public void Bet(float level)
    {
        Bet(level, false);
    }

    //操作上鎖&解鎖
    public void SetOperationLockOut(bool onOff)
    {
        betSlider.interactable = onOff; //設定拉桿狀態

        for (int i = 0; i < betButton.Length; i++) //設定按鈕狀態
        {
            betButton[i].interactable = onOff;
        }
    }

    //(private)下注
    //[param] level = 下注等級 , delay = 中獎線延遲銷毀
    private void Bet(float level, bool delay)
    {
        if (level > nowUnlockLevel) //若欲下注的中獎線等級超過解鎖等級, 自動往回拉
        {
            betSlider.value = nowUnlockLevel;
            return;
        }
        else
        {
            betSlider.value = level; //再次調整Slider值(透過Button呼叫的情況)
        }

        //賭注金額變化
        nowBetMoney = lineLevelBlockList[(int)betSlider.value - 1].betMoney; //設定目前下注等級的賭注金額
        betMoneyLabel.text = nowBetMoney + MoneyManager.Instance.moneyLabelSuffix; //賭注金額顯示

        //中獎線等級方塊特效
        for (int i = 0; i < nowUnlockLevel; i++)
        {
            if (i < betSlider.value) lineLevelBlockList[i].lineLevelBlock.SetState(LineLevelBlockStyle.激活顏色);
            else lineLevelBlockList[i].lineLevelBlock.SetState(LineLevelBlockStyle.未激活顏色);
        }

        AudioManagerScript.Instance.PlayAudioClip("SE長按鈕");

        //中獎線顯示
        List<int> levelList = new List<int>();
        for (int i = 1; i <= level; i++)
        {
            levelList.Add(i);
        }

        //等級方塊選擇特效
        ParticleEffectController.Instance.OneShotEffect(ParticleEffectType.選擇等級, lineLevelBlockList[(int)betSlider.value - 1].lineLevelBlock.transform.position, true); //撥放選擇特效

        if (delay || !isPointerEnter) LotteryLineManager.Instance.DisplayLineCountDown(destroyDelayTime, levelList); //顯示後延遲銷毀(使用Button呼叫時)
        else LotteryLineManager.Instance.DisplayLines(levelList); //正常顯示(使用Slider呼叫時)

    }

    //(協程)UI透明度變化
    private IEnumerator Cor_AlphaChange()
    {
        while (true)
        {
            if (isPointerEnter) //透明
            {
                canvasGroup.alpha = Mathf.Clamp(canvasGroup.alpha -= Time.deltaTime * alphaChangeSpeed, alphaFloor, 1);
                if (canvasGroup.alpha == alphaFloor) yield return new WaitUntil(() => !isPointerEnter); //完全透明時, 等待透明方向改變
            }
            else //不透明
            {
                canvasGroup.alpha = Mathf.Clamp(canvasGroup.alpha += Time.deltaTime * alphaChangeSpeed, alphaFloor, 1);
                if (canvasGroup.alpha == 1) yield return new WaitUntil(() => isPointerEnter); //完全不透明時, 等待透明方向改變
            }

            yield return new WaitForEndOfFrame();
        }
    }
}