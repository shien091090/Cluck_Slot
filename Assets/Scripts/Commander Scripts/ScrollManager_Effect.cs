//捲軸控制管理腳本
//[Partial]特效管理
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ScrollManager : MonoBehaviour
{
    //設定拉霸提示特效
    public void SetLeverHintEffect(bool onOff)
    {
        if (onOff) //開啟
        {
            StopCoroutine(Cor_LeverHintEffect());
            StartCoroutine(Cor_LeverHintEffect());
        }
        else //關閉
        {
            ParticleEffectController.Instance.SetStaticEffect("Par_LeverHint", false);
        }
    }

    //拉霸提示特效
    private IEnumerator Cor_LeverHintEffect()
    {
        yield return new WaitUntil(() => GameController.Instance.leverCanUse); //等待拉霸操作被允許

        float timer = 0; //計時器

        while (timer <= leverHintWaitingTime)
        {
            if (!GameController.Instance.leverCanUse) yield break; //若中途拉霸已經被禁止操作(已經拉下), 則結束程序

            timer += Time.deltaTime; //計時器推進
            yield return new WaitForEndOfFrame();
        }

        //開啟粒子特效
        ParticleEffectController.Instance.SetStaticEffect("Par_LeverHint", true);

        leverAnim.Play("lever_focus", 0, 0); //撥放動畫
    }
}
