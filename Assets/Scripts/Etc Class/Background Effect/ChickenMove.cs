//小雞移動
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ChickenMove : MonoBehaviour
{
    [Header("遊戲進行狀態")]
    public bool isIdling; //是否在閒置狀態

    //---------------------------------------------------------------------------------------------------------------------------------------------

    void Start()
    {

    }

    //---------------------------------------------------------------------------------------------------------------------------------------------

    //初始化
    public void Initialize(ChickenPropsBehavior parentScript)
    {
        parentScript.GetIdleChicken += ReturnIdleState; //註冊事件
        isIdling = false; //非閒置狀態
    }

    //移動
    public void Move(float scale, Transform born, Transform target, float duration)
    {
        this.transform.localScale = new Vector3(scale, scale, this.transform.localScale.z); //設定尺寸
        this.transform.position = born.position; //初始化位置

        if (born.position.x > target.position.x) //小雞面對左邊
        {
            this.transform.rotation = new Quaternion(this.transform.rotation.x, 0, this.transform.rotation.z, this.transform.rotation.w);
        }
        else //小雞面對右邊
        {
            this.transform.rotation = new Quaternion(this.transform.rotation.x, 180, this.transform.rotation.z, this.transform.rotation.w);
        }

        this.gameObject.SetActive(true); //顯示物件
        isIdling = false; //設定為非閒置狀態

        Tweener moving = this.transform.DOMove(target.position, duration) //開始移動
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                this.gameObject.SetActive(false);
                isIdling = true;
            });
    }

    //回傳閒置狀態
    public void ReturnIdleState(object sender, IdleChickenEventArgs eventArgs)
    {
        if (!isIdling) return; //非閒置狀態, 結束程序
        if (eventArgs.idleChickenObject != null) return; //已有閒置小雞存在時, 結束程序

        eventArgs.idleChickenObject = this.gameObject;
    }
}
