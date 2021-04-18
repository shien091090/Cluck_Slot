//(ScriptableObject)
//圖格元素數據
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewElementAttribute", menuName = "SNTools/Create ElementAttribute", order = 1)]
public class ElementAttribute : ScriptableObject
{
    public int maxAmount; //最大圖格元素數量
    public List<ElementAttributeData> m_data;
}

[System.Serializable]
public class ElementAttributeData
{
    public ElementImageType imgType; //圖格種類
    public ElementProperty property; //性質
    public Sprite blockSprite; //圖片(Sprite)
    public Sprite simpleSprite; //簡圖
    //public int amount; //數量
}
