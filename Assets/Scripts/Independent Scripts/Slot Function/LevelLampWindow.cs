//中獎線等級燈號窗格腳本
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelLampWindow : MonoBehaviour
{
    [Header("遊戲進行狀態")]
    public int rowNumber; //列數編號
    public WindowSide columnSide; //窗格方向
    public List<LevelLamp> lampList = new List<LevelLamp>(); //燈號物件列表

    //---------------------------------------------------------------------------------------------------------------------------------------------

    //設定位置與尺寸
    public void Initialize(WindowSide side, int rowNum, float pos_y, float height)
    {
        RectTransform _rt = this.GetComponent<RectTransform>();

        columnSide = side; //設定左側or右側
        rowNumber = rowNum; //設定列編號
        _rt.position = new Vector2(_rt.position.x, pos_y); //設定位置
        _rt.sizeDelta = new Vector2(_rt.sizeDelta.x, height); //設定高度
    }

    //事件訂閱
    public void EventAdd()
    {
        LevelLampManager.Instance.LampsBuilt += BuildLamps;
    }

    //創建燈號
    private void BuildLamps(object sender, BuildLampsEventArgs eventArgs)
    {
        GameObject _prefab = LevelLampManager.Instance.lampPrefab; //取得燈號物件預置體
        LotteryLineSetting _setting = eventArgs.setting; //取得設定集
        List<LineDrawer> _activeLineList = eventArgs.activeLines; //取得有效線段列表

        List<int> _preBuildList = new List<int>(); //待創建等級列表

        for (int i = 0; i < _activeLineList.Count; i++) //遍歷所有線段
        {
            if (( columnSide == WindowSide.左側 && _activeLineList[i].coordinates[0].y == rowNumber ) || //窗格在左側 且 中獎線的左端點列數與窗格列數相符何時
                ( columnSide == WindowSide.右側 && _activeLineList[i].coordinates[_activeLineList[i].coordinates.Count - 1].y == rowNumber )) //窗格在右側 且 中獎線的右端點列數與窗格列數相符何時
            {
                _preBuildList.Add(_activeLineList[i].level); //加入等級列表
            }
        }

        _preBuildList = ListExtensibleScript<int>.RepetitionFilter(_preBuildList); //篩掉重複值
        _preBuildList.Sort(); //排序

        if (rowNumber < 0) _preBuildList.Reverse(); //列號為負值時(下方列), 倒轉燈號等級順序(使之對稱)

        float _height = LevelLampManager.Instance.defaultHeight; //取得燈號預設高度
        for (int i = 0; i < _preBuildList.Count; i++) //逐一設定燈號
        {
            if (lampList.Count < i + 1) //若燈號物件不足時, 創立新物件
            {
                GameObject _go = Instantiate(_prefab, this.transform); //創立物件
                LevelLamp _lamp = _go.GetComponent<LevelLamp>(); //取得腳本

                lampList.Add(_lamp); //加入列表
            }

            lampList[i].Initialize(_preBuildList[i], _setting.lotteryLineLevelList[_preBuildList[i] - 1].lineColor, _setting.lotteryLineLevelList[_preBuildList[i] - 1].inactiveColor, _height); //設定燈號
        }

        for (int i = _preBuildList.Count; i < lampList.Count; i++) //暫時隱藏多餘的燈號
        {
            lampList[i].Invalid();
        }
    }
}
