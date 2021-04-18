//中獎線管理腳本
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LotteryLineManager : MonoBehaviour
{
    private static LotteryLineManager _instance;
    public static LotteryLineManager Instance { get { return _instance; } } //取得單例物件

    [Header("遊戲進行狀態")]
    public List<LineDrawer> lines = new List<LineDrawer>(); //中獎線列表

    [Header("參考物件")]
    public Canvas referenceLayer; //參照層(會讀取此Canvas的Order In Layer)

    [Header("Prefab")]
    public GameObject linePrefab; //線段預置體

    private LotteryLineSetting applyingSetting; //套用中的中獎線設定

    public Dictionary<Vector2, Vector3> Dict_CoordinateToWorldPosition { private set; get; } //(字典)從圖格座標編號取得圖格中心點位置(World Postion) 

    public event System.EventHandler<LineRenderEventArgs> LineRendered; //畫線事件

    //---------------------------------------------------------------------------------------------------------------------------------------------

    void Awake()
    {
        if (_instance == null) _instance = this; //設定單例模式
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------

    //初始化中獎線設定
    //[param] setting = 中獎線設定集 , sensors = 位置偵測器集合
    public void SettingInitialize(LotteryLineSetting lineSetting)
    {
        //LotteryLineSetting setting;

        List<PositionSensorBehavior> sensors = ScrollManager.Instance.posSensorArray; //取得偵測器物件列表

        if (lineSetting != null) applyingSetting = lineSetting; //若輸入值不為空, 則更新中獎線設定

        //if (settingName == string.Empty) //若輸入的設定集名稱為空, 則視同不更新設定集
        //{
        //    if (applyingSetting == null) setting = BetController.Instance.Dict_LotteryLineSettingTable[BetController.Instance.initialSettingName]; //若原本沒有套用設定集, 則使用預設設定集
        //    else setting = applyingSetting;
        //}
        //else if (!BetController.Instance.Dict_LotteryLineSettingTable.ContainsKey(settingName)) return; //若輸入的中獎線設定集名稱不存在時, 直接結束程序
        //else setting = BetController.Instance.Dict_LotteryLineSettingTable[settingName]; //取得設定集

            //建立座標編號與世界位置對應的字典
        Dict_CoordinateToWorldPosition = new Dictionary<Vector2, Vector3>();
        for (int i = 0; i < sensors.Count; i++)
        {
            Dict_CoordinateToWorldPosition.Add(sensors[i].coordinate, sensors[i].transform.position);
        }

        //中獎線設定
        int _index = 0; //列表索引值
        List<LineDrawer> _activeLineList = new List<LineDrawer>(); //有效線段列表
        for (int i = 0; i < applyingSetting.lotteryLineLevelList.Count; i++) //遍歷所有中獎線等級
        {
            int _level = applyingSetting.lotteryLineLevelList[i].levelNumber; //等級
            Color _style = applyingSetting.lotteryLineLevelList[i].lineColor; //線段顏色

            for (int j = 0; j < applyingSetting.lotteryLineLevelList[i].lines.Count; j++, _index++) //遍歷所有等級的每一條中獎線
            {
                if (lines.Count < _index + 1) //中獎線物件不足時, 創建之
                {
                    GameObject _go = Instantiate(linePrefab, this.transform);
                    LineDrawer _lineDrawer = _go.GetComponent<LineDrawer>(); //取得腳本
                    _go.gameObject.name = "Line " + _index;

                    lines.Add(_lineDrawer); //加入列表
                }

                //初始化線段設定
                if (lines[_index].Initialize(_level, applyingSetting.lotteryLineLevelList[i].thickness, _style, referenceLayer.sortingOrder, applyingSetting.lotteryLineLevelList[i].lines[j].coordinates, Dict_CoordinateToWorldPosition))
                {
                    _activeLineList.Add(lines[_index]); //若為有效線段則加入列表
                }
            }
        }

        for (int i = _index; i < lines.Count; i++) //暫時隱藏多餘的線條
        {
            lines[i].Invalid();
        }

        //中獎線等級燈號設定
        float _row = sensors.Count / ScrollManager.Instance.scrollCount; //列數
        List<float> _yAxisList = new List<float>(); //Y軸位置列表
        List<int> _rowList = new List<int>(); //列數編號列表
        for (int i = 0; i < _row; i++)
        {
            _yAxisList.Add(sensors[i].transform.position.y);
            _rowList.Add((int)sensors[i].coordinate.y);
        }

        LevelLampManager.Instance.SetLevelLamp(_yAxisList, _rowList, ScrollManager.Instance.scrollGroup[0].elementHolder.cellSize.y, applyingSetting, _activeLineList); //等級燈號設置

        //追加兩端窗格位置至線段兩端
        for (int i = 0; i < _activeLineList.Count; i++)
        {
            _activeLineList[i].AddWindowPosition(new Vector3[] { LevelLampManager.Instance.GetWindowPosition(WindowSide.左側, _activeLineList[i].coordinates[0].y), LevelLampManager.Instance.GetWindowPosition(WindowSide.右側, _activeLineList[i].coordinates[_activeLineList[i].coordinates.Count - 1].y) });
        }
    }

    //顯示線段並在指定時間後隱藏
    public void DisplayLineCountDown(float sec, List<int> levels)
    {
        StopAllCoroutines();
        StartCoroutine(Cor_DisplayLineCountDown(sec, levels));
    }

    //顯示線段(多載1/2) 指定單一等級
    //[param] level = 等級
    public void DisplayLines(int level)
    {
        if (LineRendered != null)
        {
            StopAllCoroutines();
            LineRendered.Invoke(this, new LineRenderEventArgs(new List<int>() { level }));
        }
    }

    //顯示線段(多載2/2) 指定多個等級
    //[param] levels = 等級(複數)
    public void DisplayLines(List<int> levels)
    {
        if (LineRendered != null)
        {
            StopAllCoroutines();
            LineRendered.Invoke(this, new LineRenderEventArgs(levels));
        }
    }

    //隱藏所有線段
    public void HideAll()
    {
        if (LineRendered != null)
        {
            StopAllCoroutines();
            LineRendered.Invoke(this, new LineRenderEventArgs(null));
        }
    }

    //取得有效線段(計算中獎)
    public List<LineDrawer> GetWorkingLines()
    {
        List<LineDrawer> _result = new List<LineDrawer>();

        for (int i = 0; i < lines.Count; i++)
        {
            if (lines[i].isWorking) _result.Add(lines[i]);
        }

        return _result;
    }

    //(協程)顯示線段並在指定時間後隱藏
    private IEnumerator Cor_DisplayLineCountDown(float sec, List<int> levels)
    {
        if (LineRendered != null) LineRendered.Invoke(this, new LineRenderEventArgs(levels)); //顯示

        yield return new WaitForSeconds(sec); //等待時間

        if (LineRendered != null) LineRendered.Invoke(this, new LineRenderEventArgs(null)); //隱藏
    }
}