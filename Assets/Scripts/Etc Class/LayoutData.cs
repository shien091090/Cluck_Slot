//UI布局資料儲存
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LayoutData : MonoBehaviour
{
    public LayoutSheet ApplyData { private set; get; } //套用中資料

    private RectTransform panelRect; //捲軸區域
    private HorizontalLayoutGroup panelLg;

    //---------------------------------------------------------------------------------------------------------------------------------------------

    void Start()
    {
        panelRect = ScrollManager.Instance.scrollHolder.GetComponent<RectTransform>();
        panelLg = ScrollManager.Instance.scrollHolder.GetComponent<HorizontalLayoutGroup>();
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------

    //追加並自動計算UI布局資料
    //[param] sc = 捲軸長度 , vc = 可視圖格長度 , es = 圖格間隔 , sh = 捲軸高度 , isApply = 是否套用
    public LayoutSheet CalculateLayoutData(int sc, int vc, Vector2 es, float sh, bool isApply)
    {
        if (panelRect == null) panelRect = ScrollManager.Instance.scrollHolder.GetComponent<RectTransform>();
        if (panelLg == null) panelLg = ScrollManager.Instance.scrollHolder.GetComponent<HorizontalLayoutGroup>();

        LayoutSheet _data = new LayoutSheet(sc, vc, es, sh);

        float _pw = panelRect.rect.width; //面板區域寬度
        float _ph = panelRect.rect.height; //面板區域高度
        float _pl = panelLg.padding.left; //面板間隔(左)
        float _pr = panelLg.padding.right; //面板間隔(右)
        float _pb = panelLg.padding.bottom; //面板間隔(下)
        float _pt = panelLg.padding.top; //面板間隔(上)
        float _ps = panelLg.spacing; //面板中間間隔寬度
        Vector2 _ld = ScrollManager.Instance.anchor_leftDown.position; //面板左下角位置
        Vector2 _ru = ScrollManager.Instance.anchor_rightUp.position; //面板右上角位置

        //計算捲軸寬度
        float scrollWidth = ( _pw - _pl - _pr - ( ( sc - 1 ) * _ps ) ) / sc;

        //計算圖格尺寸
        float _esz_x = scrollWidth - ( es.x * 2 );
        float _esz_y = ( sh - ( es.y * ( vc + 1 ) ) ) / vc;
        Vector2 elementSize = new Vector2(_esz_x, _esz_y);

        //計算圖格位置陣列
        float leftDownPos_x = ( ( ( _ru.x - _ld.x ) * ( _pl + es.x + ( _esz_x / 2 ) ) ) / _pw ) + _ld.x; //左下圖格位置(X軸)
        float leftDownPos_y = ( ( ( _ru.y - _ld.y ) * ( _pb + es.y + ( _esz_y / 2 ) ) ) / _ph ) + _ld.y; //左下圖格位置(Y軸)
        Vector2 leftDownPos = new Vector2(leftDownPos_x, leftDownPos_y);

        float rightUpPos_x = ( ( ( _ld.x - _ru.x ) * ( _pr + es.x + ( _esz_x / 2 ) ) ) / _pw ) + _ru.x; //右上圖格位置(X軸)
        float rightUpPos_y = ( ( ( _ld.y - _ru.y ) * ( _pt + es.y + ( _esz_y / 2 ) ) ) / _ph ) + _ru.y; //右上圖格位置(Y軸)
        Vector2 rightUpPos = new Vector2(rightUpPos_x, rightUpPos_y);

        //套用計算結果
        _data.scrollWidth = scrollWidth;
        _data.elementSize = elementSize;
        _data.elementPos_leftDown = leftDownPos;
        _data.elementPos_rightUp = rightUpPos;

        if (isApply) ApplyData = _data;
        return _data;
    }
}

//一筆Layout資訊
[System.Serializable]
public class LayoutSheet
{
    public int ScrollCount { private set; get; } //捲軸長度
    public int VisibleCount { private set; get; } //可視圖格長度
    public Vector2 ElementSpacing { private set; get; } //圖格間隔
    public float ScrollHeight { private set; get; } //捲軸高度

    public float scrollWidth; //捲軸寬度
    public Vector2 elementSize; //圖格尺寸
    public Vector2 elementPos_leftDown; //圖格位置(左下)
    public Vector2 elementPos_rightUp; //圖格位置(右下)

    //建構子
    public LayoutSheet(int sc, int vc, Vector2 es, float sh)
    {
        ScrollCount = sc;
        VisibleCount = vc;
        ElementSpacing = es;
        ScrollHeight = sh;
    }

    //比照是否符合
    public bool MatchTest(int sc, int vc, Vector2 es, float sh)
    {
        if (ScrollCount == sc && VisibleCount == vc && ElementSpacing == es && ScrollHeight == sh) return true;
        else return false;
    }
}