//測試工具
//(Paritial)機率設定
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public partial class TestPanelManager : MonoBehaviour
{
    //重新讀取圖格元素機率設定
    public void ReloadProbabilitySetting()
    {
        List<Roulette> _roulette = ScrollManager.Instance.applyRoulette; //取得輪盤
        int maxAmount = elementAttribute.maxAmount; //圖格元素最大數量

        for (int i = 0; i < probUnitList.Count; i++)
        {
            probUnitList[i].Initialize(elementAttribute.m_data[i].blockSprite, i, _roulette[i].amount, maxAmount);
        }
    }

    //修改元素數量
    public void ModifyElementCount(int index, int count)
    {
        ScrollManager.Instance.applyRoulette[index].amount = count; //修改元素數量

        totalAmount = 0;
        if (ElementCountCollected != null) ElementCountCollected.Invoke(this, System.EventArgs.Empty); //重新計算元素總數量

        for (int i = 0; i < probUnitList.Count; i++) //調整其中一個元素數量後, 重新計算全部元素的所佔比例%數
        {
            probUnitList[i].ShowPercentage(totalAmount);
        }
    }

    //儲存機率設定(新建Scriptable Obeject將設定值儲存進去)
    public void ApplyProbabilitySetting()
    {

#if UNITY_EDITOR

        RouletteSetting _asset = ScriptableObject.CreateInstance<RouletteSetting>();
        _asset.SaveRouletteData(ScrollManager.Instance.applyRoulette);

        for (int i = 1; true; i++)
        {
            if (i > 100) break; //防卡死
            if (AssetDatabase.AssetPathToGUID("Assets/AttributeData/RouletteSetting" + i + ".asset") == string.Empty)
            {
                AssetDatabase.CreateAsset(_asset, "Assets/AttributeData/RouletteSetting" + i + ".asset");
                break;
            }
        }

        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = _asset;

#endif

    }

    //儲存設定按鈕
    public void SaveSettingButton()
    {
        string _c = "是否將機率設定儲存至新建的設定檔案(Scriptable Object)?";

        confirmWindowScript.WindowPopup(_c, ApplyProbabilitySetting);
    }
}
