using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinCollectionEffect : MonoBehaviour, IExtensibleParticle
{
    public AnimationCurve speedCurve; //錢幣移動曲線
    public float delayTime; //延遲時間(動畫撥放多久後開始移動)
    public AnimationCurve gainCoinRatio; //獲利&特效金幣數量比值

    private Vector3 targetPos; //目標位置
    private float timer; //計時器
    private ParticleSystem ps;
    private ParticleSystem.Particle[] arrPar;

    //---------------------------------------------------------------------------------------------------------------------------------------------

    //錢幣移動
    private IEnumerator Cor_CoinMove()
    {
        targetPos = MoneyManager.Instance.transform.position; //取得移動目標的位置
        timer = 0; //計時器

        while (timer < delayTime) //等待延遲時間
        {
            timer += Time.deltaTime; //計時器推進
            yield return new WaitForEndOfFrame();
        }

        timer = 0; //計時器歸零

        while (timer < speedCurve.keys[speedCurve.length - 1].time)
        {
            int parCount = ps.GetParticles(arrPar);

            for (int i = 0; i < parCount; i++)
            {
                arrPar[i].position = Vector3.Lerp(arrPar[i].position, targetPos, speedCurve.Evaluate(timer)); //錢幣移動
            }

            ps.SetParticles(arrPar, parCount);

            timer += Time.deltaTime; //計時器推進
            yield return new WaitForEndOfFrame();
        }

    }

    //特效初始化(撥放前準備)
    void IExtensibleParticle.Initialize(object param)
    {
        int _gain = 0;
        if (param != null) _gain = (int)param; //取得獲利獎金

        if (!ps) ps = this.GetComponent<ParticleSystem>(); //取得粒子特效組件
        arrPar = new ParticleSystem.Particle[ps.main.maxParticles];

        var _triggerModule = ps.trigger;
        _triggerModule.enabled = true;
        _triggerModule.SetCollider(0, MoneyManager.Instance.BoxCollider); //設定碰撞器

        var _emission = ps.emission;
        _emission.rateOverTimeMultiplier = gainCoinRatio.Evaluate(_gain); //依獲利獎金設定粒子數量

        StopAllCoroutines();

        StartCoroutine(Cor_CoinMove()); //開始錢幣移動
    }
}
