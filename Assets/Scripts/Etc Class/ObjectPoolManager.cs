//物件池管理
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    private static ObjectPoolManager _instance;
    public static ObjectPoolManager Instance { get { return _instance; } } //取得單例物件

    [System.Serializable]
    public struct ObjectPoolUnit //物件池元素
    {
        public string gameObjectName; //遊戲物件名稱
        public GameObject prefabReference; //預置體參考
        public Transform parentHolder; //父物件
        public List<GameObject> ObjectPoolList; //物件池列表

        //加入遊戲物件至列表
        public void AddElement(GameObject go)
        {
            if (ObjectPoolList == null) ObjectPoolList = new List<GameObject>();
            ObjectPoolList.Add(go);
        }
    }

    public List<ObjectPoolUnit> objectPoolSetting; //物件池設定
    public Dictionary<string, ObjectPoolUnit> Dict_ObjectPoolTag { private set; get; } //(字典)從物件名稱查找ObjectPoolUnit

    //---------------------------------------------------------------------------------------------------------------------------------------------

    void Awake()
    {
        if (_instance == null) _instance = this; //設定單例模式
    }

    void Start()
    {
        if (objectPoolSetting != null && objectPoolSetting.Count > 0) //若有設定物件池
        {
            Dict_ObjectPoolTag = new Dictionary<string, ObjectPoolUnit>();

            for (int i = 0; i < objectPoolSetting.Count; i++)
            {
                if (Dict_ObjectPoolTag.ContainsKey(objectPoolSetting[i].gameObjectName)) throw new System.Exception("[ERROR]物件名稱重複");

                Dict_ObjectPoolTag.Add(objectPoolSetting[i].gameObjectName, objectPoolSetting[i]); //建立字典
            }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------

    //從物件池中取得指定物件
    public GameObject PickUpObject(string goName)
    {
        if (!Dict_ObjectPoolTag.ContainsKey(goName)) return null; //查無物件

        GameObject _result = null;
        ObjectPoolUnit _unit = Dict_ObjectPoolTag[goName];

        //創立新物件(Lambda)
        System.Action<ObjectPoolUnit> CreateNew = (ObjectPoolUnit u) =>
        {
            GameObject _go = Instantiate(u.prefabReference, u.parentHolder);
            u.AddElement(_go);

            _result = _go;
        };

        if (_unit.ObjectPoolList == null || _unit.ObjectPoolList.Count == 0) //若物件池中無物件
        {
            CreateNew(_unit); //創立新物件
        }
        else //若物件池中存在物件
        {
            for (int i = 0; i < _unit.ObjectPoolList.Count; i++) //尋找隱藏中(active = false)物件
            {
                if (!_unit.ObjectPoolList[i].activeSelf)
                {
                    _result = _unit.ObjectPoolList[i];
                    break;
                }
            }

            if (_result == null) CreateNew(_unit); //若所有物件皆在非隱藏狀態, 則建立新物件
        }

        return _result;
    }
}
