//捲軸行為
//※掛在Scroll Prefab上
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ScrollBehavior : MonoBehaviour
{
    [Header("遊戲進行狀態")]
    public bool isRolling = false; //是否在旋轉中
    public byte number; //編號(最左邊為0)
    public List<ElementCellBehavior> elementChain; //圖格鍊
    public int chainTailLength; //圖格鍊尾部長度
    public bool invalidAnimation = false; //無效化動畫進行中

    public static bool[] s_scrollRollingStates; //捲軸旋轉狀態

    [Header("參考物件")]
    public RectTransform sliderHandleArea; //隱藏拉條區域(Rect)
    public GridLayoutGroup elementHolder; //圖格布局(LayoutGroup)
    public LayoutElement layoutElement; //布局元素(LayoutElement)

    public Slider sld { private set; get; } //捲軸拉條(控制旋轉)
    public CanvasGroup cg; //控制透明度用
    private Sequence tween_invalid; //(DoTween動畫)無效化

    //---------------------------------------------------------------------------------------------------------------------------------------------

    void Awake()
    {
        sld = this.GetComponent<Slider>();
        cg = this.GetComponent<CanvasGroup>();
        elementChain = new List<ElementCellBehavior>(); //初始化圖格鍊
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------

    //初始化
    //[param] num = 編號 , minWidth = 預設捲軸最小寬度, prefrerrdWidth = 預設捲軸優先寬度 , duration = 動畫作動時間(預設為0, 不執行動畫)
    public void Initialize(byte num, float minWidth, float preferredWidth, float duration = 0)
    {
        number = num; //套用編號

        if (this.gameObject.activeSelf) return; //原本就在激活狀態則直接結束程序

        this.gameObject.SetActive(true); //激活物件

        if (duration <= 0) //不使用動畫時
        {
            layoutElement.minWidth = minWidth;
            layoutElement.preferredWidth = preferredWidth;
        }
        else //使用動畫時
        {
            Sequence sq_initialize = DOTween.Sequence()
                .OnStart(() =>
                {
                    layoutElement.minWidth = 0;
                    layoutElement.preferredWidth = 0;
                })
                .Append(DOTween.To(() => layoutElement.minWidth, (x) => layoutElement.minWidth = x, minWidth, duration))
                .Join(DOTween.To(() => layoutElement.preferredWidth, (x) => layoutElement.preferredWidth = x, preferredWidth, duration))
                .SetEase(Ease.Unset);
        }
    }

    //無效化
    //[param] minWidth = 預設捲軸最小寬度, prefrerrdWidth = 預設捲軸優先寬度 , duration = 動畫作動時間(預設為0, 不執行動畫)
    public void Invalid(float minWidth, float prefrerrdWidth, float duration = 0)
    {
        if (!this.gameObject.activeSelf) return; //原本就在未激活狀態時, 直接結束程序

        if (duration <= 0) //不使用動畫時
        {
            layoutElement.minWidth = 0;
            layoutElement.preferredWidth = 0;
            this.gameObject.SetActive(false);
        }
        else //使用動畫時
        {
            if (tween_invalid == null)
            {
                tween_invalid = DOTween.Sequence()
                    .OnStart(() =>
                    {
                        layoutElement.minWidth = minWidth;
                        layoutElement.preferredWidth = prefrerrdWidth;
                        invalidAnimation = true;
                    })
                    .Append(DOTween.To(() => layoutElement.minWidth, (x) => layoutElement.minWidth = x, 0, duration))
                    .Join(DOTween.To(() => layoutElement.preferredWidth, (x) => layoutElement.preferredWidth = x, 0, duration))
                    .SetEase(Ease.Unset)
                    .SetAutoKill(false);
            }
            else
            {
                tween_invalid.Restart();
            }
        }
    }

    //設置圖格鍊
    //[param] imgTypeList = 圖格鍊
    //[param] tailLength = 尾巴的長度(=可視圖格數量, 需要留一段與可視圖格等長的尾巴, 使得Slider在0、1之間切換時不會有跳格現象)
    public void SetElementChain(List<ElementImageType> imgTypeList, int tailLength)
    {
        if (imgTypeList == null) //不指定圖格鍊時, 使用原有的圖格鍊
        {
            imgTypeList = new List<ElementImageType>();

            for (int i = 0; i < elementChain.Count - chainTailLength; i++) //圖格鍊 = 原有圖格鍊 - 尾巴部分
            {
                imgTypeList.Add(elementChain[i].elementType);
            }
        }

        GameObject elementPrefab = ScrollManager.Instance.elementPrefab; //取得圖格預置體

        elementChain = new List<ElementCellBehavior>(); //初始化圖格鍊

        for (int i = 0; i < imgTypeList.Count + tailLength; i++) //逐一設定圖格
        {
            ElementCellBehavior _element;

            if (elementHolder.transform.childCount < i + 1) //若圖格鍊長度不足, 創立新圖格
            {
                GameObject _go = Instantiate(elementPrefab, elementHolder.transform);
                _element = _go.GetComponent<ElementCellBehavior>();
            }

            if (!elementHolder.transform.GetChild(i).gameObject.activeSelf) elementHolder.transform.GetChild(i).gameObject.SetActive(true); //激活物件

            _element = elementHolder.transform.GetChild(i).gameObject.GetComponent<ElementCellBehavior>(); //取得腳本

            int _index = 0; //欲套用的圖格鍊索引

            if (i >= imgTypeList.Count) _index = i - imgTypeList.Count; //圖格鍊尾巴的設定(複製頭部)
            else _index = i;

            _element.SetType(imgTypeList[_index]); //設定圖格類型

            elementChain.Add(_element); //加入列表
        }

        for (int i = imgTypeList.Count + tailLength; i < elementHolder.transform.childCount; i++) //多餘項目隱藏之
        {
            elementHolder.transform.GetChild(i).gameObject.SetActive(false);
        }

        chainTailLength = tailLength; //紀錄尾巴長度
    }

    //重置捲軸位置至最底部(Slider.Value = 0)
    //[param] nowElements = 目前拉霸結果 , visibleCount = 可視圖格數量
    public void ResetScrollPosition(List<ElementImageType> nowElements, int visibleCount)
    {
        for (int i = 0; i < nowElements.Count; i++)
        {
            elementChain[i].SetType(nowElements[i]); //頭部設定
        }

        for (int i = 0; i < visibleCount; i++)
        {
            elementChain[elementChain.Count - 1 - i].SetType(elementChain[visibleCount - 1 - i].elementType); //尾部設定
        }

        sld.value = 0;
    }

    //(協程)UI布局設定
    //[param] originData = 原資料 , newData = 欲套用資料 , duration = 動畫時間
    public IEnumerator Cor_SetUILayout(LayoutSheet originData, LayoutSheet newData, float duration = 0)
    {
        float _scrollLength = elementChain.Count * ( newData.elementSize.y + newData.ElementSpacing.y );
        float _handleAreaHeight = _scrollLength - ( newData.ScrollHeight - newData.ElementSpacing.y );

        if (duration <= 0 || !this.gameObject.activeSelf) //無動畫
        {
            elementHolder.spacing = newData.ElementSpacing; //設定圖格間隔
            elementHolder.cellSize = newData.elementSize; //設定圖格尺寸
            sliderHandleArea.offsetMax = new Vector2(sliderHandleArea.offsetMax.x, newData.ElementSpacing.y); //設定捲軸滾動區域位移量
        }
        else //有動畫
        {
            Vector2 _sizeTarget = invalidAnimation ? new Vector2(0, newData.elementSize.y) : newData.elementSize;

            Sequence sq_layoutVariation = DOTween.Sequence()
                .OnStart(() =>
                {
                    elementHolder.spacing = originData.ElementSpacing; //圖格間隔初始化
                    elementHolder.cellSize = originData.elementSize; //圖格尺寸初始化
                })
                .Append(DOTween.To(() => elementHolder.spacing, (x) => elementHolder.spacing = x, newData.ElementSpacing, duration))
                .Join(DOTween.To(() => elementHolder.cellSize, (x) => elementHolder.cellSize = x, _sizeTarget, duration))
                .Join(DOTween.To(() => sliderHandleArea.offsetMax, (x) => sliderHandleArea.offsetMax = x, new Vector2(sliderHandleArea.offsetMax.x, newData.ElementSpacing.y), duration))
                .SetEase(Ease.Unset);

            yield return sq_layoutVariation.WaitForCompletion();
        }

        sliderHandleArea.sizeDelta = new Vector2(sliderHandleArea.sizeDelta.x, _handleAreaHeight);

        //重設圖格碰撞體大小
        for (int i = 0; i < elementChain.Count; i++)
        {
            elementChain[i].SetColliderSize(newData.elementSize);
        }

        if (invalidAnimation) //無效化動畫執行中
        {
            yield return tween_invalid.WaitForCompletion(); //等待無效化動畫執行完畢
            invalidAnimation = false;

            this.gameObject.SetActive(false); //隱藏物件
        }
    }

    //捲軸旋轉
    //[param] sliderSpeed = 旋轉速度 , spinTime = 旋轉時間 , resultList = 捲軸結果, startingCurve = 起步動畫曲線
    public IEnumerator Cor_ScrollSpin(float sliderSpeed, float spinTime, List<ElementImageType> resultList, AnimationCurve startingCurve)
    {
        //初始化
        float _timer = 0; //計時器
        float _soundFreq = 0; //音效頻率
        bool _snap = false; //玩家中斷訊號
        isRolling = true;

        //旋轉程序(起步動畫時間強制執行, 不可中斷)
        while (_timer < startingCurve.keys[startingCurve.length - 1].time + spinTime)
        {
            float _accRate = startingCurve.Evaluate(_timer); //加速度
            float _final = ( sld.value + ( sliderSpeed * _accRate ) ) % 1; //最終值
            if (_final < 0) _final = 1 + _final; //若值小於0則從1開始往回減
            sld.value = _final;

            _soundFreq += sliderSpeed * _accRate;
            if (Mathf.Abs(_soundFreq) >= ScrollManager.Instance.scrollSoundFrequency) //撥放旋轉音效
            {
                AudioManagerScript.Instance.PlayAudioClip("SE捲軸旋轉");
                _soundFreq = 0;
            }

            _timer += Time.fixedDeltaTime; //計時器推進

            if (_timer >= startingCurve.keys[startingCurve.length - 1].time) //起步動畫執行完畢後, 旋轉狀態On, 中斷訊號On, 若由外部干涉使旋轉狀態Off, 則中斷旋轉
            {
                if (!s_scrollRollingStates[number])
                {
                    if (_snap) break;
                    else
                    {
                        s_scrollRollingStates[number] = true; //旋轉狀態 = On
                        _snap = true;
                    }
                }
            }

            yield return new WaitForFixedUpdate();
        }

        if (s_scrollRollingStates[number]) s_scrollRollingStates[number] = false; //旋轉狀態 = Off

        //旋轉結束, 計算欲煞停的指定位置
        float vc = ScrollManager.Instance.visibleElementCount; //可視區域長度
        float t = elementChain.Count; //總長度
        float u = 1f / ( t - vc ); //Slider.Value最小單位
        float i = sld.value / u; //第一項索引近似值
        float target = 0; //Slider.Value目標值
        Vector2 resultArea = new Vector2(); //最終圖格索引範圍
        bool dir = ( sliderSpeed > 0 ); //旋轉方向(true : 正方向 , false : 反方向))

        if (dir) //正方向旋轉
        {
            i = Mathf.Ceil(i);
            i = ( i + vc ) % ( t - vc );
        }
        else //反方向旋轉
        {
            i = Mathf.Floor(i);
            i = ( i - vc ) < 0 ? ( ( t - vc ) + ( i - vc ) ) : ( i - vc );
        }

        resultArea = new Vector2(i, i + vc - 1); //計算最終圖格索引範圍
        target = i * u; //計算Slider.Value目標值

        Dictionary<int, int> dict_headTailIndex = new Dictionary<int, int>(); //頭尾索引對照
        for (int j = 0; j < vc; j++)
        {
            dict_headTailIndex.Add(j, (int)( t - vc + j ));
            dict_headTailIndex.Add((int)( t - vc + j ), j);
        }

        for (int j = 0; j < resultArea.y - resultArea.x + 1; j++) //將捲軸結果套用於最終煞停之捲軸
        {
            elementChain[(int)resultArea.x + j].SetType(resultList[j]);

            if (dict_headTailIndex.ContainsKey((int)resultArea.x + j)) elementChain[dict_headTailIndex[(int)resultArea.x + j]].SetType(resultList[j]); //當欲設定之圖格在頭尾區域時, 同步設定之

        }

        float _extTarget = target; //目標延展值(使目標值無視邊緣的數值跳躍, 將數值往同一方向延展)
        if (dir && sld.value > target) _extTarget += 1;
        else if (!dir && sld.value < target) _extTarget -= 1;

        //Debug.Log(string.Format("Slider.Value = {0} // ExtTarget = {1}", Slider.value, _extTarget));

        //捲軸煞停程序
        float _sum = sld.value; //Slider.Value累加值
        while (true)
        {
            _sum += sliderSpeed;

            if (( dir && _sum > _extTarget ) || ( !dir && _sum < _extTarget )) //若捲軸旋轉值在下一個瞬間動作會超過目標值, 則完成捲軸煞停
            {
                //Debug.Log("捲軸停止");
                sld.value = target;
                break;
            }

            //持續旋轉
            if (dir) sld.value = _sum % 1;
            else if (!dir) sld.value = _sum < 0 ? ( 1 + _sum ) : _sum;

            yield return new WaitForFixedUpdate();
        }

        AudioManagerScript.Instance.PlayAudioClip("SE煞停");

        isRolling = false; //結束旋轉狀態
        //Debug.Log("捲軸旋轉結束");
    }
}
