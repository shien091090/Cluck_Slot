//圖格元素行為
//※掛載在圖格的預置物(Element Cell)上
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ElementCellBehavior : MonoBehaviour
{
    [Header("遊戲進行狀態")]
    public ElementImageType elementType; //圖格類型

    private Image img; //Image組件
    private BoxCollider2D collider; //碰撞體組件
    private Animator anim; //動畫器組件

    //---------------------------------------------------------------------------------------------------------------------------------------------

    //設定圖片
    public void SetType(ElementImageType type)
    {
        if (img == null) img = this.GetComponent<Image>();

        elementType = type; //設定圖格類型

        Sprite _sprite = ScrollManager.Instance.Dict_elementAttribute[type].blockSprite;
        img.sprite = _sprite; //套用圖片
    }

    //設定碰撞體大小
    public void SetColliderSize(Vector2 size)
    {
        if (collider == null) collider = this.GetComponent<BoxCollider2D>();

        collider.size = size;
    }

    //中獎動畫控制
    public void PlayAnimation(ElementAnimationType animType, bool b)
    {
        if (anim == null) anim = this.GetComponent<Animator>();

        switch (animType)
        {
            case ElementAnimationType.全部動畫停止:
                anim.SetBool("isMatch", false);
                break;

            case ElementAnimationType.中獎:
                anim.SetBool("isMatch", b);
                break;
        }

    }
}
