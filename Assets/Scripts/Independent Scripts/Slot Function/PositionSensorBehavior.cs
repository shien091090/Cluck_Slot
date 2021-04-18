//位置偵測器腳本
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionSensorBehavior : MonoBehaviour
{
    [Header("遊戲進行狀態")]
    public Vector2 coordinate; //座標位置
    public ElementCellBehavior pointerElement; //所在位置的圖格

    private Rigidbody2D rb;
    private bool isTriggered = false; //是否已觸發Sensor

    //---------------------------------------------------------------------------------------------------------------------------------------------

    void Awake()
    {
        rb = this.GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        ScrollManager.Instance.SpinJudged += SensorTrigger; //旋轉判定事件訂閱
        ScrollManager.Instance.ElementAnimationPlayed += PlayElementAnimation; //圖格動畫事件訂閱
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        ElementCellBehavior _element = collision.GetComponent<ElementCellBehavior>();
        pointerElement = _element; //設定所在位置圖格

        isTriggered = true;
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------

    //設定物件狀態(多載1/2)
    //[param] state = 隱藏or顯示 , pos = 位置
    public void SetSensorActive(bool state, Vector2 pos, Vector2 cdn)
    {
        this.gameObject.SetActive(state); //物件顯示or隱藏
        rb.simulated = false; //剛體模擬(一律關閉)
        transform.position = pos; //設定物件位置
        coordinate = cdn; //設定座標編號
    }

    //設定物件狀態(多載2/2) ※不干涉位置
    public void SetSensorActive(bool state)
    {
        this.gameObject.SetActive(state); //物件顯示or隱藏
        rb.simulated = false; //剛體模擬(一律關閉)
    }

    //偵測器觸發事件(並將結果儲存至PositionSensorEventArgs.spinResultList中)
    private void SensorTrigger(object sender, PositionSensorEventArgs eventArgs)
    {
        if (!gameObject.activeSelf) return;
        StartCoroutine(Cor_SensorTrigger(eventArgs));
    }

    //圖格動畫撥放事件
    private void PlayElementAnimation(object sender, ElementAnimationEventArgs eventArgs)
    {
        if (!this.gameObject.activeSelf) return; //若物件為隱藏狀態時, 直接結束程序
        if (pointerElement == null) return; //若尚未設定圖格物件, 直接結束程序
        if (eventArgs.targetPosList != null && !eventArgs.targetPosList.Contains(coordinate)) return; //若非作用座標的圖格, 直接結束程序(座標列表為null, 直接視為套用所有座標)

        pointerElement.PlayAnimation(eventArgs.animationType, eventArgs.onOff);
    }

    //偵測器觸發(協程)
    private IEnumerator Cor_SensorTrigger(PositionSensorEventArgs eventArgs)
    {
        rb.simulated = true; //開啟剛體模擬(捕捉Trigger訊號)

        yield return new WaitUntil(() => isTriggered);

        eventArgs.AddResult(coordinate, pointerElement.elementType); //加入偵測結果
        eventArgs.overNumber--;

        isTriggered = false;
        rb.simulated = false; //關閉剛體模擬
    }
}
