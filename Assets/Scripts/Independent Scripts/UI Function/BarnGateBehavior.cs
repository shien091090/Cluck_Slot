//過場門物件行為
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarnGateBehavior : MonoBehaviour
{
    private static BarnGateBehavior _instance;
    public static BarnGateBehavior Instance { get { return _instance; } }

    private bool gateState = true;
    public bool GateState //存取門的狀態
    { get
        {
            return gateState;
        }
        set
        {
            gateState = value;
        }
    }
    private Animator gateAnim; //門動畫機

    //---------------------------------------------------------------------------------------------------------------------------------------------

    void Awake()
    {
        if (_instance == null) _instance = this;
        else Destroy(this.gameObject);
    }

    void Start()
    {
        DontDestroyOnLoad(this.transform.parent); //跨場景物件
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------

    //撥放門動畫
    //[param] isOpen = true : 關閉➡開啟 / false : 開啟➡關閉
    public IEnumerator Cor_GateAnimation(bool isOpen)
    {
        if (GateState == isOpen) yield break; //若門已經是指定狀態則直接退出

        if (gateAnim == null)
        {
            if (this.gameObject.GetComponent<Animator>() == null) throw new System.Exception("[ERROR]物件需掛載Animator");
            gateAnim = this.gameObject.GetComponent<Animator>();
        }

        string _clipName = isOpen ? "GateOpen" : "GateClose"; //撥放動畫名稱

        gateAnim.Play(_clipName, 0, 0); //撥放動畫

        yield return new WaitUntil(() => ( gateAnim.GetCurrentAnimatorStateInfo(0).IsName(_clipName) && gateAnim.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 )); //等待動畫撥放完畢

        GateState = isOpen; //改變門狀態
    }
}
