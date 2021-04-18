//捲軸控制管理腳本
//[Partial]UI布局控制
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class ScrollManager : MonoBehaviour
{
    //設置位置偵測器
    public void SetPostionSensor(Vector2 leftDown, Vector2 rightUp, int column, int row)
    {
        posSensorArray = new List<PositionSensorBehavior>(); //陣列初始化
        Dict_cdnToWorldPos = new Dictionary<Vector2, Vector2>(); //重置字典(從座標查找世界位置)

        Vector2 _offset = new Vector2(( column <= 1 ? 0 : ( rightUp.x - leftDown.x ) / ( column - 1 ) ), ( row <= 1 ? 0 : ( rightUp.y - leftDown.y ) / ( row - 1 ) )); //座標位移量

        for (int i = 0, _total = 0; i < column; i++) //遍歷每欄&每列
        {
            for (int j = 0; j < row; j++, _total++)
            {
                PositionSensorBehavior _sensor;

                if (posSensorHolder.transform.childCount < _total + 1) //若偵測器數量不足, 創立新的偵測器
                {
                    GameObject _go = Instantiate(posSensorPrefab, posSensorHolder);
                    _sensor = _go.GetComponent<PositionSensorBehavior>();
                }

                _sensor = posSensorHolder.GetChild(_total).gameObject.GetComponent<PositionSensorBehavior>(); //取得腳本

                Vector2 _pos = new Vector2(leftDown.x + ( i * _offset.x ), leftDown.y + ( j * _offset.y )); //計算位置
                Vector2 _cdn = new Vector2(i, ( Mathf.FloorToInt(( 1 - row ) / 2f) ) + j); //計算座標編號

                _sensor.SetSensorActive(true, _pos, _cdn); //設定Sensor物件狀態
                Dict_cdnToWorldPos.Add(_cdn, _pos); //將位置對照資訊加入字典

                posSensorArray.Add(_sensor); //加入陣列
            }
        }

        for (int i = posSensorArray.Count; i < posSensorHolder.childCount; i++) //多餘項目隱藏之
        {
            PositionSensorBehavior _sensor = posSensorHolder.GetChild(i).gameObject.GetComponent<PositionSensorBehavior>();
            _sensor.SetSensorActive(false);
        }
    }

    //捲軸設定初始化
    //[param] isAnimation = 是否使用動畫
    public void SetScrollCount(bool isAnimation)
    {
        for (int i = 0; i < scrollCount; i++)
        {
            if (scrollGroup.Count < scrollCount) //捲軸物件不足時, 創建之
            {
                GameObject _go = Instantiate(scrollPrefab, scrollHolder);
                ScrollBehavior _scr = _go.GetComponent<ScrollBehavior>(); //取得腳本

                _go.SetActive(false); //暫時隱藏物件
                scrollGroup.Add(_scr); //加入列表
            }

            scrollGroup[i].Initialize((byte)i, scrollMinWidth, scrollPrefrerrdWidth, isAnimation ? layoutAnimationDuration : 0);
        }

        for (int i = scrollCount; i < scrollGroup.Count; i++) //暫時隱藏多餘捲軸
        {
            if (isAnimation) scrollGroup[i].Invalid(scrollMinWidth, scrollPrefrerrdWidth, layoutAnimationDuration); //有動畫
            else scrollGroup[i].Invalid(scrollMinWidth, scrollPrefrerrdWidth); //無動畫
        }

    }

    //設置捲軸區域(多載1/4) 參數為默認值, 僅改變圖格鍊
    public IEnumerator Cor_SetScrollPanel(List<ElementImageType> elementChain, bool isAnimation, LotteryLineSetting lineSetting = null)
    {
        int _sc = slotLevelLayoutSetting[GameController.Instance.nowSlotLevel].scrollCount; //捲軸數量
        int _vc = slotLevelLayoutSetting[GameController.Instance.nowSlotLevel].visibleCount; //可視數量
        Vector2 _sp = slotLevelLayoutSetting[GameController.Instance.nowSlotLevel].elementSpacing; //可視數量

        List<List<ElementImageType>> _elementChainGroup = new List<List<ElementImageType>>();
        for (int i = 0; i < _sc; i++) //複製捲軸圖格鍊
        {
            _elementChainGroup.Add(elementChain);
        }

        yield return StartCoroutine(Cor_SetScrollAreaLayout(_sc, _vc, _sp, _elementChainGroup, isAnimation, lineSetting));
    }

    //設置捲軸區域(多載2/4) 共同捲軸圖格鍊給所有捲軸複用
    public IEnumerator Cor_SetScrollPanel(int scrollCount, int visibleCount, Vector2 spacing, List<ElementImageType> elementChain, bool isAnimation, LotteryLineSetting lineSetting = null)
    {
        List<List<ElementImageType>> _elementChainGroup = new List<List<ElementImageType>>();
        for (int i = 0; i < scrollCount; i++) //複製捲軸圖格鍊
        {
            _elementChainGroup.Add(elementChain);
        }

        yield return StartCoroutine(Cor_SetScrollAreaLayout(scrollCount, visibleCount, spacing, _elementChainGroup, isAnimation, lineSetting));
    }

    //設置捲軸區域(多載3/4) 個別指定所有捲軸的圖格鍊
    public IEnumerator Cor_SetScrollPanel(int scrollCount, int visibleCount, Vector2 spacing, List<List<ElementImageType>> elementChainGroup, bool isAnimation, LotteryLineSetting lineSetting = null)
    {
        yield return StartCoroutine(Cor_SetScrollAreaLayout(scrollCount, visibleCount, spacing, elementChainGroup, isAnimation, lineSetting));
    }

    //設置捲軸區域(多載4/4) 僅調整顯示介面
    public IEnumerator Cor_SetScrollPanel(int scrollCount, int visibleCount, Vector2 spacing, bool isAnimation, LotteryLineSetting lineSetting = null)
    {
        yield return StartCoroutine(Cor_SetScrollAreaLayout(scrollCount, visibleCount, spacing, null, isAnimation, lineSetting));
    }

    //設置捲軸區域(協程)
    //[param] scrollCount = 捲軸數量, visibleCount = 可視圖格數量, elementChainGroup = 各捲軸圖格鍊, isAnimation = 是否啟用動畫
    private IEnumerator Cor_SetScrollAreaLayout(int scrollCount, int visibleCount, Vector2 spacing, List<List<ElementImageType>> elementChainGroup, bool isAnimation, LotteryLineSetting lineSetting = null)
    {
        //判斷UI介面是否重新調整
        RectTransform panelRect = scrollHolder.GetComponent<RectTransform>();
        HorizontalLayoutGroup panelLg = scrollHolder.GetComponent<HorizontalLayoutGroup>();
        float scrollHeight = panelRect.rect.height - panelLg.padding.top - panelLg.padding.bottom; //取得捲軸區域高度

        //設定更新前後布局資料
        LayoutSheet originData = new LayoutSheet(0, 0, Vector2.zero, 0); //改變前布局資料
        LayoutSheet newData = new LayoutSheet(0, 0, Vector2.zero, 0); //新的布局資料
        bool isLayoutReset = false; //布局參數是否重置
        bool _isAnimation = isAnimation; //布局更新啟用動畫

        if (layoutData.ApplyData == null) //初始化的狀況
        {
            isLayoutReset = true;
            _isAnimation = false; //初始化時強制不使用動畫
        }
        else if (layoutData.ApplyData.MatchTest(scrollCount, visibleCount, spacing, scrollHeight) == false) //更新參數的狀況
        {
            isLayoutReset = true;
            originData = layoutData.ApplyData;
        }

        if (isLayoutReset) layoutData.CalculateLayoutData(scrollCount, visibleCount, spacing, scrollHeight, true); //如果有改變則套用新的參數資料
        newData = layoutData.ApplyData;

        //設定捲軸數量
        if (isLayoutReset)
        {
            this.scrollCount = scrollCount; //校正Editor參數

            //重新設定捲軸數量
            SetScrollCount(_isAnimation);
        }

        //設定捲軸圖格鍊
        if (elementChainGroup == null) //不指定圖格鍊時
        {
            for (int i = 0; i < scrollGroup.Count; i++)
            {
                if (( scrollGroup[i].elementChain == null || scrollGroup[i].elementChain.Count == 0 ) && defaultElementChain != null) //若捲軸沒有設定圖格鍊, 則套用預設圖格鍊
                {
                    scrollGroup[i].SetElementChain(defaultElementChain, visibleCount);
                }

                if (isLayoutReset) //重新調整布局
                {
                    scrollGroup[i].SetElementChain(null, visibleCount); //若可視圖格長度有變動, 則捲軸的尾部也會跟著變動
                }
            }
        }
        else //有指定圖格鍊時
        {
            if (elementChainGroup.Count != scrollCount) throw new System.Exception("[ERROR]圖格鍊數量與捲軸數量不符");
            for (int i = 0; i < scrollGroup.Count; i++) //設定各捲軸的圖格鍊
            {
                scrollGroup[i].SetElementChain(elementChainGroup[i], visibleCount);
            }
        }

        //設置位置偵測器
        SetPostionSensor(layoutData.ApplyData.elementPos_leftDown, layoutData.ApplyData.elementPos_rightUp, scrollCount, visibleCount);

        //中獎線重新設置
        LotteryLineManager.Instance.SettingInitialize(lineSetting);

        //設定可視圖格數&調整UI
        if (isLayoutReset)
        {
            this.visibleElementCount = visibleCount; //校正Editor參數
            this.elementSpacing = spacing; //校正Editor參數

            for (int i = 0; i < scrollGroup.Count; i++)
            {
                if (_isAnimation) //有動畫
                {
                    if (( previousSpinResult != null && previousSpinResult.Count > 0 ) && ( originData.VisibleCount != newData.VisibleCount )) //若拉霸結果有儲存紀錄, 且可視圖格數量有改變, 則將slider.value設為0(拉至最下方), 以防動畫演示出現Bug
                    {
                        List<ElementImageType> _nowElements = new List<ElementImageType>(); //該捲軸目前的顯示圖格
                        for (int j = 0; j < previousSpinResult.Count; j++)
                        {
                            if ((int)previousSpinResult[j].coordinate.x == i) _nowElements.Add(previousSpinResult[j].elementType); //將目前顯示圖格中X軸(列號)符合的項目加入列表
                        }

                        scrollGroup[i].ResetScrollPosition(_nowElements, visibleCount); //重置捲軸
                    }

                    if (i == scrollGroup.Count - 1) //等待最後一個捲軸的動畫撥放完成
                    {
                        yield return StartCoroutine(scrollGroup[i].Cor_SetUILayout(originData, newData, layoutAnimationDuration));
                    }
                    else
                    {
                        StartCoroutine(scrollGroup[i].Cor_SetUILayout(originData, newData, layoutAnimationDuration));
                    }

                }
                else StartCoroutine(scrollGroup[i].Cor_SetUILayout(null, layoutData.ApplyData)); //無動畫
            }
        }

        yield return null;
    }
}
