//中獎線等級燈號管理腳本
//※LevelLampManager儲存多個LevelLampWindow(窗格), 一個LevelLampWindow又儲存了多個LevelLamp(燈號)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelLampManager : MonoBehaviour
{
    private static LevelLampManager _instance;
    public static LevelLampManager Instance { get { return _instance; } } //取得單例物件

    [Header("可自訂參數")]
    public float defaultHeight; //燈號預設高度

    [Header("遊戲進行狀態")]
    public List<LevelLampWindow> lampWindowList = new List<LevelLampWindow>(); //燈號窗格列表

    [Header("參考物件")]
    public Transform leftSideHolder; //左側燈號父物件區域
    public Transform rightSideHolder; //左側燈號父物件區域

    [Header("Prefab")]
    public GameObject lampWindowPrefab; //燈號窗格預置體
    public GameObject lampPrefab; //燈號預置體

    public event System.EventHandler<BuildLampsEventArgs> LampsBuilt; //創建燈號事件

    //---------------------------------------------------------------------------------------------------------------------------------------------

    void Awake()
    {
        if (_instance == null) _instance = this; //設定單例模式
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------

    //設置等級燈號
    //[param] yAxisList = Y軸位置(世界座標)列表(依可視圖格) , rowList = 列數編號列表 , height = 圖格高度 , setting = 中獎線設定 , activeList = 有效線段
    public void SetLevelLamp(List<float> yAxisList, List<int> rowList, float height, LotteryLineSetting setting, List<LineDrawer> activeList)
    {
        for (int i = 0; i < yAxisList.Count; i++) //遍歷Y軸位置數量(列數)
        {
            if (lampWindowList.Count < ( i * 2 ) + 1) //中獎線物件不足時, 創建之
            {
                GameObject _leftGo = Instantiate(lampWindowPrefab, leftSideHolder); //左側創立物件
                GameObject _rightGo = Instantiate(lampWindowPrefab, rightSideHolder); //右側創立物件

                LevelLampWindow _leftWindow = _leftGo.GetComponent<LevelLampWindow>();
                LevelLampWindow _rightWindow = _rightGo.GetComponent<LevelLampWindow>();

                //事件訂閱
                _leftWindow.EventAdd();
                _rightWindow.EventAdd();

                lampWindowList.Add(_leftWindow); //加入列表(左側窗格)
                lampWindowList.Add(_rightWindow); //加入列表(右側窗格))
            }

            lampWindowList[i * 2].gameObject.SetActive(true); //激活左側窗格
            lampWindowList[( i * 2 ) + 1].gameObject.SetActive(true); //激活右側窗格

            lampWindowList[i * 2].Initialize(WindowSide.左側, rowList[i], yAxisList[i], height); //設定左側窗格位置與尺寸
            lampWindowList[( i * 2 ) + 1].Initialize(WindowSide.右側, rowList[i], yAxisList[i], height); //設定右側窗格位置與尺寸
        }

        for (int i = yAxisList.Count * 2; i < lampWindowList.Count; i++) //隱藏多餘窗格
        {
            lampWindowList[i].gameObject.SetActive(false); //隱藏窗格
        }

        if (LampsBuilt != null) LampsBuilt.Invoke(this, new BuildLampsEventArgs(setting, activeList)); //通知各窗格創立燈號
    }

    //取得窗格位置
    //[param] side = 窗格方向, row = 列數編號
    public Vector3 GetWindowPosition(WindowSide side, float row)
    {
        Vector3 _result = new Vector3();

        for (int i = 0; i < lampWindowList.Count; i++) //搜尋符合條件的物件
        {
            if (lampWindowList[i].columnSide == side && lampWindowList[i].rowNumber == row && lampWindowList[i].gameObject.activeSelf)
            {
                _result = lampWindowList[i].transform.position;
            }
        }

        return _result;
    }
}
