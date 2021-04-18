//金雞蛋行為
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
public class GoldenEgg : MonoBehaviour, IPointerDownHandler
{
    [Header("遊戲進行狀態")]
    public int carryMoney; //攜帶金錢量
    public float rotateSpeed; //旋轉速度

    private Rigidbody2D rb;
    public Rigidbody2D Rb //取得剛體
    {
        get
        {
            if (rb == null) rb = this.GetComponent<Rigidbody2D>();
            return rb;
        }
    }

    private CapsuleCollider2D cld;
    public CapsuleCollider2D Cld //取得碰撞體
    {
        get
        {
            if (cld == null) cld = this.GetComponent<CapsuleCollider2D>();
            return cld;
        }
    }

    private Image img;
    public Image Img //取得圖片
    {
        get
        {
            if (img == null) img = this.GetComponent<Image>();
            return img;
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------

    void Update()
    {
        if (this.gameObject.activeSelf)
        {
            this.transform.Rotate(Vector3.forward * rotateSpeed * Time.deltaTime);
        }
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("EggCollection")) //觸碰邊界時回收物件
        {
            SetEggActive(false);
        }
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData) //點擊時獲得獎勵
    {
        StartCoroutine(MoneyManager.Instance.Cor_SetMoney(MoneyManager.Instance.nowMoney + carryMoney, false)); //所持金錢增加

        Vector3 _mousePos = Vector3.zero;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(StartMenuManager.Instance.screenRect, Input.mousePosition, Camera.main, out _mousePos); //鼠標點擊位置

        //音效
        AudioManagerScript.Instance.PlayAudioClip("SE金幣");

        //特效
        ParticleEffectController.Instance.OneShotEffect(ParticleEffectType.金雞蛋, this.transform.position, true);
        ParticleEffectController.Instance.OneShotEffect(ParticleEffectType.金幣收集, _mousePos, true, carryMoney);

        SetEggActive(false);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------

    //初始化
    //[param] spt = 圖片精靈 , money = 金錢量 , pos = 出現位置 , scale = 尺寸 , rotateSpd = 旋轉速度 , startSpd = 初速
    public void Initialize(Sprite spt, int money, Vector3 pos, float scale, float rotateSpd, Vector2 startSpd)
    {
        //Debug.Log(pos);
        this.transform.localPosition = pos; //設定位置

        Img.sprite = spt; //設定圖片
        carryMoney = money; //設定金錢攜帶量
        rotateSpeed = rotateSpd; //設定旋轉速度
        this.transform.localScale = new Vector2(scale, scale); //設定尺寸
        Cld.size = this.GetComponent<RectTransform>().sizeDelta * scale; //設定碰撞體範圍

        SetEggActive(true);
        Rb.velocity = startSpd; //設定初速
    }

    //設定蛋的激活狀態
    private void SetEggActive(bool b)
    {
        Rb.simulated = b;
        this.gameObject.SetActive(b);
    }
}
