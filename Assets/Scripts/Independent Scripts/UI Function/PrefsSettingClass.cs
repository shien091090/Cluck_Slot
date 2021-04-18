using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//遊戲儲存值資料
[System.Serializable]
public class PrefsSettingData
{
    public string paramKey; //參數鍵字串
    public object paramValue; //參數值
    public string paramDefault; //參數初始值
    public PlayerPrefsDataType dataType; //值類型

    //建構子
    public PrefsSettingData(string key, string defaultValue, PlayerPrefsDataType type)
    {
        paramKey = key;
        paramDefault = defaultValue;
        dataType = type;

        paramValue = GetPrefsData();
    }

    //取得遊戲儲存值
    public object GetPrefsData()
    {
        try
        {
            switch (dataType)
            {
                case PlayerPrefsDataType.Int:
                    int intV = int.Parse(paramDefault);
                    return PlayerPrefs.GetInt(paramKey, intV);

                case PlayerPrefsDataType.Float:
                    float singleV = float.Parse(paramDefault);
                    return PlayerPrefs.GetFloat(paramKey, singleV);

                case PlayerPrefsDataType.String:
                    string strV = paramDefault;
                    return PlayerPrefs.GetString(paramKey, strV);
            }
        }
        catch (System.FormatException)
        {
            Debug.Log("[ERROR]輸入參數型別轉換失敗");
        }
        catch (UnityException)
        {
            Debug.Log("[ERROR]Unity Exception");
        }

        return null;
    }

    //存入遊戲儲存值
    public void SetPrefsData(object v)
    {
        //Debug.Log("[" + paramKey + "] SetPrefsData : " + v);

        try
        {
            switch (dataType)
            {
                case PlayerPrefsDataType.Int:
                    int intV = (int)v;
                    PlayerPrefs.SetInt(paramKey, intV);
                    break;

                case PlayerPrefsDataType.Float:
                    float singleV = (float)v;
                    PlayerPrefs.SetFloat(paramKey, singleV);
                    break;

                case PlayerPrefsDataType.String:
                    string strV = (string)v;
                    PlayerPrefs.SetString(paramKey, strV);
                    break;
            }
        }
        catch (System.FormatException)
        {
            Debug.Log("[ERROR]輸入參數型別轉換失敗");
        }
        catch (UnityException)
        {
            Debug.Log("[ERROR]Unity Exception");
        }

        PlayerPrefs.Save();
    }
}

//遊戲儲存值管理類別
[System.Serializable]
public class PrefsSettingDataManager
{
    public List<PrefsSettingData> prefsDataList; //遊戲儲存值列表
    private Dictionary<string, PrefsSettingData> dict_Data; //(字典)從鍵值查找儲存值資料

    //建構子
    public PrefsSettingDataManager(List<PrefsSettingData> dataList)
    {
        prefsDataList = dataList;

        //建立字典
        dict_Data = new Dictionary<string, PrefsSettingData>();
        for (int i = 0; i < dataList.Count; i++)
        {
            dict_Data.Add(dataList[i].paramKey, dataList[i]);
        }
    }

    //存入儲存值
    public void SetDataValue(string key, object v)
    {
        dict_Data[key].SetPrefsData(v);
    }

    //讀取儲存值
    public object GetDataValue(string key)
    {
        return dict_Data[key].GetPrefsData();
    }
}

//遊戲儲存值類型列舉值
public enum PlayerPrefsDataType
{
    Int, Float, String
}