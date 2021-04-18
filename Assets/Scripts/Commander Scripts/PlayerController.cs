//玩家輸入控制
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    private static PlayerController _instance;
    public static PlayerController Instance { get { return _instance; } } //取得單例物件

    [Header("遊戲進行狀態")]
    public bool isControlActive; //控制許可

    //[Header("測試用")]
    //public ScrollBehavior scroll; //捲軸
    //public bool dir; //旋轉方向
    //public int targetIndex; //目標索引



    //---------------------------------------------------------------------------------------------------------------------------------------------

    void Awake()
    {
        if (_instance == null) _instance = this; //設定單例模式
        else Destroy(this.gameObject);
    }

    void Update()
    {
        if (isControlActive)
        {
            KeyListen(); //按鍵監聽
            MouseListen(); //滑鼠事件監聽
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------

    //按鍵監聽
    private void KeyListen()
    {
        if (Input.GetKeyDown(KeyCode.F1)) //跳轉到以目標索引為開頭的捲軸選轉結果
        {
            //    for (int j = 0; j < scroll.elementChain.Count; j++)
            //    {
            //        scroll.elementChain[j].PlayAnimation(ElementAnimationType.全部動畫停止, true);
            //    }

            //    //旋轉結束, 計算欲煞停的指定位置
            //    float vc = ScrollManager.Instance.visibleElementCount; //可視區域長度
            //    float t = scroll.elementChain.Count; //總長度
            //    float u = 1f / ( t - vc ); //Slider.Value最小單位
            //    float i = targetIndex; //第一項索引近似值
            //    float _target = 0; //Slider.Value目標值
            //    Vector2 _f = new Vector2(); //最終圖格索引範圍

            //    if (dir) //正方向旋轉
            //    {
            //        i = Mathf.Ceil(i);
            //        i = ( i + vc ) % ( t - vc );
            //    }
            //    else //反方向旋轉
            //    {
            //        i = Mathf.Floor(i);
            //        i = ( i - vc ) < 0 ? ( ( t - vc ) + ( i - vc ) ) : ( i - vc );
            //    }

            //    _f = new Vector2(i, i + vc - 1); //計算最終圖格索引範圍
            //    _target = i * u; //計算Slider.Value目標值

            //    Debug.Log(string.Format("起始索引 : {0} / 目標索引 : {1} ~ {2}", targetIndex, _f.x, _f.y));
            //    scroll.elementChain[(int)_f.x].PlayAnimation(ElementAnimationType.中獎, true);
            //    scroll.elementChain[(int)_f.y].PlayAnimation(ElementAnimationType.中獎, true);

            //    scroll.sld.value = _target;
            //}

            //if (Input.GetKeyDown(KeyCode.F2)) //跳轉指定的捲軸索引
            //{
            //    float vc = ScrollManager.Instance.visibleElementCount; //可視區域長度
            //    float t = scroll.elementChain.Count; //總長度
            //    float u = 1f / ( t - vc ); //Slider.Value最小單位

            //    scroll.sld.value = targetIndex * u;
        }

    }

    //滑鼠事件監聽
    private void MouseListen()
    {
        if (Input.GetMouseButtonDown(0)) //按下左鍵
        {
            GameObject _go = GameObject.FindGameObjectWithTag("PS");
            RectTransform _tf = _go.GetComponent<RectTransform>();

            Vector3 _mousePos = Vector3.zero;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(_tf, Input.mousePosition, Camera.main, out _mousePos); //鼠標點擊位置

            ParticleEffectController.Instance.OneShotEffect(ParticleEffectType.滑鼠點擊, _mousePos, true); //滑鼠點擊特效
        }
    }
}
