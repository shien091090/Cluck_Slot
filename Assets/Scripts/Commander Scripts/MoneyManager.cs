//金錢管理腳本
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class MoneyManager : MonoBehaviour
{
    private static MoneyManager _instance;
    public static MoneyManager Instance { get { return _instance; } } //取得單例物件

    [Header("可自訂參數")]
    public int initialMoney; //初始資金
    public string moneyLabelSuffix; //金錢文字後綴
    public float animationDuration; //動畫執行時間
    public float gainMoneyEffectDelay; //獎金獲得特效延遲時間

    [Header("遊戲進行狀態")]
    public int nowMoney; //目前所持金錢

    [Header("參考物件")]
    public Text totalMoneyLabel; //總資金文字

    private BoxCollider2D boxCollider;
    public BoxCollider2D BoxCollider
    {
        get
        {
            if (boxCollider == null) boxCollider = this.GetComponent<BoxCollider2D>();
            return boxCollider;
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------

    void Awake()
    {
        if (_instance == null) _instance = this; //設定單例模式
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------

    //初始化
    public void Initialize()
    {
        int _money = PlayerPrefs.GetInt("GAME_MONEY", initialMoney);

        StartCoroutine(Cor_SetMoney(_money, false));

    }

    //所持金錢變化
    //[param] money = 金額 ,  isAnimation = 是否撥放動畫
    public IEnumerator Cor_SetMoney(int money, bool isAnimation)
    {
        int targetMoney = Mathf.Clamp(money, 0, int.MaxValue); //校正金額在0~int上限

        System.Action SetValue = () =>
        {
            nowMoney = targetMoney; //設定持有金錢
            PlayerPrefs.SetInt("GAME_MONEY", targetMoney); //紀錄持有金錢
            totalMoneyLabel.text = nowMoney + moneyLabelSuffix; //設定顯示文字
        };

        if (isAnimation)
        {
            //金額累積動畫
            int _money = nowMoney;
            Tweener tw_textVariation = DOTween.To(() => _money, (x) => _money = x, targetMoney, animationDuration)
                .SetEase(Ease.OutSine)
                .OnUpdate(() =>
                {
                    totalMoneyLabel.text = _money + moneyLabelSuffix;
                });

            yield return tw_textVariation.WaitForCompletion();

            SetValue();
        }
        else SetValue(); //無動畫
    }

    //(協程)獲得中獎報酬
    //[param] prize = 中獎獎金 , prizeInfoList = 所有線段的中獎資訊列表
    public IEnumerator Cor_GetPrize(int prize, List<PrizeLineInfo> prizeInfoList)
    {
        if (prize <= 0) yield break; //中獎獎金小於0, 直接結束程序

        //金幣收集特效
        List<PrizeLineInfo> _infoList = new List<PrizeLineInfo>();
        _infoList.AddRange(prizeInfoList); //複製一份新的資訊列表

        _infoList.RemoveAll((PrizeLineInfo x) => //篩掉沒有中獎的線段
        {
            if (x.sumPrize <= 0) return true;
            else return false;
        });

        List<Vector2> worldPosList = new List<Vector2>();
        for (int i = 0; i < _infoList.Count; i++)
        {
            for (int j = 0; j < _infoList[i].drawedPosList.Count; j++)
            {
                Vector2 _pos = ScrollManager.Instance.Dict_cdnToWorldPos[_infoList[i].drawedPosList[j]]; //從座標編號轉換成世界位置
                worldPosList.Add(_pos); //將世界位置加入列表
                ParticleEffectController.Instance.OneShotEffect(ParticleEffectType.金幣收集, _pos, true, _infoList[i].sumPrize);
            }
        }

        AudioManagerScript.Instance.PlayAudioClip("SE金幣");

        //中獎圖格特效
        worldPosList = ListExtensibleScript<Vector2>.RepetitionFilter(worldPosList);
        for (int i = 0; i < worldPosList.Count; i++)
        {
            ParticleEffectController.Instance.OneShotEffect(ParticleEffectType.中獎圖格, worldPosList[i], true);
        }

        //累積拉霸等級獎金
        Coroutine cor_UnlockPrize = StartCoroutine(SpinLevelPanel.Instance.Cor_AccumulateUnlockPrize(prize));

        yield return new WaitForSeconds(gainMoneyEffectDelay);

        //獲得獎金
        Coroutine cor_Set = StartCoroutine(Cor_SetMoney(nowMoney + prize, true));

        //等待拉霸等級解鎖&獲得獎金特效
        yield return cor_UnlockPrize;
        yield return cor_Set;
    }

    //錢不夠特效
    public IEnumerator Cor_MoneyNotEnough()
    {
        Transform tf = totalMoneyLabel.transform;

        Sequence _shake = DOTween.Sequence()
            .Append(tf.DOShakePosition(0.5f, 20, 20, 90))
            .Join(totalMoneyLabel.DOColor(Color.red, 0.25f))
            .Insert(0.25f,totalMoneyLabel.DOColor(Color.white, 0.25f));
        
        yield return _shake.WaitForCompletion();
    }
}
