//背景小雞隨機生成行為
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChickenPropsBehavior : MonoBehaviour
{
    [System.Serializable]
    public struct ChickenInfo
    {
        public Transform bornPos; //出生地點
        public Transform targetPos; //目的地
        public float duration; //花費時間
        public int lineCount; //隊伍隻數
        public float lineSpacingTime; //隊伍間隔時間差
        public Vector2 randomScaleRange; //隨機尺寸範圍
    }

    [Header("可自訂參數")]
    public List<ChickenInfo> chickenSetting; //小雞行為設定
    public float bornFrequency; //出生頻率

    [Header("參考物件")]
    public Transform parentHolder; //父物件區域

    [Header("Prefab")]
    public GameObject chickenPrefab; //小雞預置體

    public event System.EventHandler<IdleChickenEventArgs> GetIdleChicken; //取得閒置小雞

    //---------------------------------------------------------------------------------------------------------------------------------------------

    void Start()
    {
        if (chickenSetting == null || chickenSetting.Count == 0) throw new System.Exception("[ERROR]未設定背景小雞行為");

        StartCoroutine(Cor_Performance());
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------

    //小雞演出
    private IEnumerator Cor_Performance()
    {
        while (true)
        {
            yield return new WaitForSeconds(bornFrequency); //等待指定時間

            int _dice = Random.Range(0, chickenSetting.Count);
            ChickenInfo _info = chickenSetting[_dice]; //隨機抽取一個設定

            StartCoroutine(Cor_Born(_info)); //產生小雞
        }
    }

    //產生小雞
    private IEnumerator Cor_Born(ChickenInfo info)
    {
        for (int i = 0; i < info.lineCount; i++)
        {
            GameObject _go = GetChickenObject();
            ChickenMove _moving = _go.GetComponent<ChickenMove>(); //取得腳本

            float randomScale = Random.Range(info.randomScaleRange.x, info.randomScaleRange.y); //尺寸隨機

            _moving.Move(randomScale, info.bornPos, info.targetPos, info.duration); //開始移動程序

            yield return new WaitForSeconds(info.lineSpacingTime); //間隔時間差
        }
    }

    //取得可用的小雞物件
    private GameObject GetChickenObject()
    {
        GameObject result;

        if (GetIdleChicken != null) //若事件內容不為空
        {
            IdleChickenEventArgs _eventArgs = new IdleChickenEventArgs();
            GetIdleChicken.Invoke(this, _eventArgs);

            if (_eventArgs.idleChickenObject != null) //有取得閒置中的小雞物件
            {
                result = _eventArgs.idleChickenObject;

                return result;
            }
        }

        result = Instantiate(chickenPrefab, parentHolder); //找不到閒置中的小雞時, 創建一隻新的

        ChickenMove _moving = result.GetComponent<ChickenMove>(); //取得腳本
        _moving.Initialize(this); //小雞行為初始化

        return result;
    }
}

//(事件參數類別)取得閒置小雞物件
public class IdleChickenEventArgs : System.EventArgs
{
    public GameObject idleChickenObject; //閒置小雞物件

    public IdleChickenEventArgs()
    {
        idleChickenObject = null;
    }
}