//中獎線等級方塊
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LineLevelBlock : MonoBehaviour
{
    [Header("可自訂參數")]
    public Color activeSytle; //激活顏色
    public Color inactiveStyle; //未激活顏色

    [Header("遊戲進行狀態")]
    public string levelLabel; //等級標籤

    private Image img;

    //---------------------------------------------------------------------------------------------------------------------------------------------

    void Awake()
    {
        img = this.GetComponent<Image>();
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------

    //設定狀態
    public void SetState(LineLevelBlockStyle style)
    {
        switch (style)
        {
            case LineLevelBlockStyle.激活顏色:
                if (img.color != activeSytle) SetEffect(true); //撥放特效
                img.color = activeSytle;
                break;

            case LineLevelBlockStyle.未激活顏色:
                if (img.color != inactiveStyle) SetEffect(false); //關閉特效
                img.color = inactiveStyle;
                break;

            case LineLevelBlockStyle.未解鎖顏色:
                if (img.color != BetController.Instance.lockingStyle) SetEffect(false); //關閉特效
                img.color = BetController.Instance.lockingStyle;
                break;
        }
    }

    //設定特效
    private void SetEffect(bool onOff)
    {
        ParticleEffectController.Instance.SetStaticEffect(levelLabel, onOff); //設置粒子特效
    }
}
