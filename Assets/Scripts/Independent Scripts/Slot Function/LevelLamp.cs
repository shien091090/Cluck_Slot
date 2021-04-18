//中獎線等級燈號腳本
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelLamp : MonoBehaviour
{
    [Header("遊戲進行狀態")]
    public int level; //等級
    public Color activeSytle; //激活顏色
    public Color inactiveStyle; //未激活顏色

    private Text txt;
    private Image img;
    private LayoutElement le;

    //---------------------------------------------------------------------------------------------------------------------------------------------

    void Awake()
    {
        txt = this.GetComponentInChildren<Text>();
        img = this.GetComponent<Image>();
        le = this.GetComponent<LayoutElement>();
    }

    void Start()
    {
        LotteryLineManager.Instance.LineRendered += LampResponse; //燈號反應事件註冊
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------

    //初始化
    public void Initialize(int lv, Color active, Color inactive, float height)
    {
        this.gameObject.SetActive(true);

        if (txt == null) txt = this.GetComponentInChildren<Text>();
        if (img == null) img = this.GetComponent<Image>();
        if (le == null) le = this.GetComponent<LayoutElement>();

        //等級設定
        level = lv;
        txt.text = lv.ToString();

        //顏色設定
        activeSytle = active;
        inactiveStyle = inactive;
        img.color = inactive;

        //高度設定
        le.preferredHeight = height;
    }

    //暫時隱藏
    public void Invalid()
    {
        this.gameObject.SetActive(false);
    }

    //燈號反應
    public void LampResponse(object sender, LineRenderEventArgs eventArgs)
    {
        if (!this.gameObject.activeSelf) return; //物件未激活時 直接結束程序

        if (eventArgs.levelList == null || eventArgs.levelList.Count == 0) //若沒有指定等級 或 指定等級列表為空時 設為未激活顏色&結束程序
        {
            img.color = inactiveStyle;
            return;
        }

        for (int i = 0; i < eventArgs.levelList.Count; i++)
        {
            if (level == eventArgs.levelList[i]) //符合指定等級時
            {
                img.color = activeSytle;
                return;
            }
        }

        img.color = inactiveStyle; //無符合等級時
    }
}
