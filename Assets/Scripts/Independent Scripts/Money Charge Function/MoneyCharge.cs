//儲值小遊戲
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;

public class MoneyCharge : MonoBehaviour
{
    [System.Serializable]
    public struct GoldenEggAttribute //金蛋屬性
    {
        public Sprite sprite; //圖片
        public Vector2 prizeRange; //獎金隨機範圍
        public Vector2 scaleRange; //尺寸隨機範圍
        public Vector2 rotateSpeedRange; //旋轉速度隨機範圍
        public Vector2 startSpeedMin; //初速最小值
        public Vector2 startSpeedMax; //初速最大值
    }

    [Header("可自訂參數")]
    public int cooldownTime; //CD時間
    public float gameDuration; //遊戲總時間
    public float eggFallInterval; //金蛋落下間隔時間
    public string suffix; //後綴字串
    public List<GoldenEggAttribute> goldenEggSetting; //金蛋設定

    [Header("遊戲進行狀態")]
    public int countdownSeconds; //剩餘秒數
    public bool chargeStandBy; //可否進行儲值
    private DateTime memoryDateTime; //記憶時間

    [Header("參考物件")]
    public Text countdownTxt; //倒數計時文字
    public Button moneyChargeBtn; //儲值按鈕
    public Transform createRefPos_left; //生成參考點(左)
    public Transform createRefPos_right; //生成參考點(右)
    public Image gameBackground; //儲值遊戲背景

    public bool ChargeStandBy //存取儲值允許狀態
    {
        get { return chargeStandBy; }
        set
        {
            moneyChargeBtn.interactable = value; //改變按鈕狀態
            chargeStandBy = value;
        }
    }

    private int CountdownSeconds //存取剩餘秒數
    {
        get { return countdownSeconds; }
        set
        {
            int _min = value / 60;
            float _sec = value % 60;

            countdownTxt.text = _min.ToString("00") + ":" + _sec.ToString("00") + suffix; //顯示倒數時間

            countdownSeconds = value;
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------

    void Start()
    {
        memoryDateTime = LoadDateTime(); //讀取時間

        if ((int)DateTime.Now.Subtract(memoryDateTime).TotalSeconds >= cooldownTime) //若紀錄時間距今已超過CD時間
        {
            ChargeStandBy = true; //可儲值
            CountdownSeconds = 0; //剩餘時間為0
        }
        else //若尚未超過CD時間則開始倒數計時
        {
            ChargeStandBy = false; //不可儲值
            CountdownSeconds = cooldownTime - (int)DateTime.Now.Subtract(memoryDateTime).TotalSeconds;
            StartCoroutine(Cor_Countdown()); //倒數計時程序
        }

    }

    //---------------------------------------------------------------------------------------------------------------------------------------------

    //開始儲值
    public void ChargeGame()
    {
        if (goldenEggSetting == null || goldenEggSetting.Count == 0) throw new Exception("[ERROR]上未設定金蛋屬性");

        StartCoroutine(Cor_ChargeGame()); //進行儲值遊戲
    }

    //讀取時間
    private DateTime LoadDateTime()
    {
        if (!PlayerPrefs.HasKey("D_YEAR")) SaveDateTime();

        int _year = PlayerPrefs.GetInt("D_YEAR", DateTime.Now.Year);
        int _month = PlayerPrefs.GetInt("D_MONTH", DateTime.Now.Month);
        int _day = PlayerPrefs.GetInt("D_DAY", DateTime.Now.Day);
        int _hour = PlayerPrefs.GetInt("D_HOUR", DateTime.Now.Hour);
        int _min = PlayerPrefs.GetInt("D_MIN", DateTime.Now.Minute);
        int _sec = PlayerPrefs.GetInt("D_SEC", DateTime.Now.Second);

        DateTime _date = new DateTime(_year, _month, _day, _hour, _min, _sec); //取得紀錄中的時間

        return _date;
    }

    //紀錄時間
    private void SaveDateTime()
    {
        int _year = DateTime.Now.Year;
        int _month = DateTime.Now.Month;
        int _day = DateTime.Now.Day;
        int _hour = DateTime.Now.Hour;
        int _min = DateTime.Now.Minute;
        int _sec = DateTime.Now.Second;

        PlayerPrefs.SetInt("D_YEAR", _year);
        PlayerPrefs.SetInt("D_MONTH", _month);
        PlayerPrefs.SetInt("D_DAY", _day);
        PlayerPrefs.SetInt("D_HOUR", _hour);
        PlayerPrefs.SetInt("D_MIN", _min);
        PlayerPrefs.SetInt("D_SEC", _sec);

        DateTime _date = new DateTime(_year, _month, _day, _hour, _min, _sec); //紀錄時間

        memoryDateTime = _date;
    }

    //時間倒數
    private IEnumerator Cor_Countdown()
    {
        DateTime _memory = DateTime.Now;

        while (CountdownSeconds > 0) //倒數計時尚未到0時
        {
            while (true)
            {
                yield return new WaitForFixedUpdate();

                if ((int)DateTime.Now.Subtract(_memory).TotalSeconds >= 1) break; //當記憶時間距現在超過1秒
            }

            CountdownSeconds -= (int)DateTime.Now.Subtract(_memory).TotalSeconds; //剩餘時間縮短

            _memory = DateTime.Now; //紀錄時間
        }

        CountdownSeconds = 0;
        ChargeStandBy = true;
    }

    //儲值遊戲
    private IEnumerator Cor_ChargeGame()
    {
        StartMenuManager.Instance.MoneyPanelMove(true); //金錢面板淡入

        StartMenuManager.Instance.buttonControllCg.blocksRaycasts = false; //禁用所有按鈕

        AudioManagerScript.Instance.Stop(0); //停止BGM

        yield return new WaitForSeconds(0.2f);

        gameBackground.gameObject.SetActive(true);
        gameBackground.color *= new Color(1, 1, 1, 0);

        Tweener backgroundFadeIn = DOTween.ToAlpha(() => gameBackground.color, x => gameBackground.color = x, 0.65f, 1.8f); //遊戲背景淡入

        yield return backgroundFadeIn.WaitForCompletion();

        yield return new WaitForSeconds(0.8f);

        AudioManagerScript.Instance.PlayAudioClip("BGM儲值遊戲");

        yield return new WaitForSeconds(0.8f);

        float _timer = 0; //遊戲計時器
        float _itv = 0; //間隔時間計算

        while (_timer < gameDuration) //尚未超過遊戲時間
        {
            if (_itv >= eggFallInterval) //到達間隔時間, 烙下金蛋
            {
                GameObject _go = ObjectPoolManager.Instance.PickUpObject("金雞蛋");
                GoldenEgg _script = _go.GetComponent<GoldenEgg>();

                int _eggIndex = UnityEngine.Random.Range(0, goldenEggSetting.Count);
                GoldenEggAttribute _att = goldenEggSetting[_eggIndex]; //隨機抽取一個金蛋屬性數據

                int _money = UnityEngine.Random.Range((int)_att.prizeRange.x, (int)_att.prizeRange.y); //隨機金錢獎勵
                float _x = UnityEngine.Random.Range(createRefPos_left.localPosition.x, createRefPos_right.localPosition.x); //隨機X軸位置
                float _scale = UnityEngine.Random.Range(_att.scaleRange.x, _att.scaleRange.y); //隨機尺寸
                float _rotateSpd = UnityEngine.Random.Range((int)_att.rotateSpeedRange.x, (int)_att.rotateSpeedRange.y); //隨機旋轉速度
                Vector2 _startSpd = new Vector2(UnityEngine.Random.Range(_att.startSpeedMin.x, _att.startSpeedMax.x), UnityEngine.Random.Range(_att.startSpeedMin.y, _att.startSpeedMax.y)); //隨機初速

                _script.Initialize(_att.sprite, _money, new Vector3(_x, createRefPos_left.localPosition.y, 0), _scale, _rotateSpd, _startSpd); //初始化金蛋行為

                _itv -= eggFallInterval;
            }

            _itv += Time.deltaTime;
            _timer += Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }

        AudioManagerScript.Instance.Stop(0); //停止BGM

        yield return new WaitForSeconds(3f);

        backgroundFadeIn = DOTween.ToAlpha(() => gameBackground.color, x => gameBackground.color = x, 0, 2); //遊戲背景淡入

        yield return backgroundFadeIn.WaitForCompletion();

        gameBackground.gameObject.SetActive(false);

        StartMenuManager.Instance.buttonControllCg.blocksRaycasts = true; //啟用所有按鈕
        StartMenuManager.Instance.MoneyPanelMove(false); //金錢面板淡出

        ChargeStandBy = false; //不可儲值
        CountdownSeconds = cooldownTime; //重置CD時間
        SaveDateTime(); //紀錄現在時間

        StartCoroutine(Cor_Countdown()); //倒數計時程序

        AudioManagerScript.Instance.PlayAudioClip("BGM開頭");
    }
}
