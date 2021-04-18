//中獎線等級方塊滑鼠事件
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class LineLevelPointerEvent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    //滑鼠移入時UI變透明
    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        BetController.Instance.isPointerEnter = true;

        List<int> _levelList = new List<int>();
        for (int i = 1; i <= BetController.Instance.betSlider.value; i++)
        {
            _levelList.Add(i);
        }
        LotteryLineManager.Instance.DisplayLines(_levelList);
    }

    //滑鼠移出時UI不透明
    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        BetController.Instance.isPointerEnter = false;

        LotteryLineManager.Instance.HideAll();
    }
}
