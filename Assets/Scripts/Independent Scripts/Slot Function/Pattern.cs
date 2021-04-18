//支付表圖案
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pattern : MonoBehaviour
{
    private Image img;
    private LayoutElement le;

    //---------------------------------------------------------------------------------------------------------------------------------------------

    void Awake()
    {
        img = this.gameObject.GetComponent<Image>();
        le = this.gameObject.GetComponent<LayoutElement>();
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------

    //設定圖片
    public void Initialize(Sprite s, Vector2 size)
    {
        //設定尺寸
        if (le == null) le = this.gameObject.GetComponent<LayoutElement>();
        le.preferredWidth = size.x;
        le.preferredHeight = size.y;

        //設定圖片
        if (img == null) img = this.gameObject.GetComponent<Image>();
        img.enabled = true;
        img.sprite = s;
    }

    //隱藏圖片
    public void Invalid()
    {
        if (img == null) img = this.gameObject.GetComponent<Image>();

        img.enabled = false;
    }
}
