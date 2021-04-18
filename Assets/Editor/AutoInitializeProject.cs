//自動初始化專案
//主要用途為剛開新專案時, 快速將基本的東西布置好(例如說Asset底下創建各種資料夾)
//請注意一定得放在"Asset/Editor"底下否則會報錯

using UnityEditor;
using System.IO;

public class AutoInitializeProject : Editor
{
    [MenuItem("SNTools/AutoInitializeProject/CreateFolder")]
    public static void CreateFolder() //自動建立資料夾
    {
        string rootPath = "Assets/"; //母資料夾名

        //自定義資料夾名稱
        string[] paths = new string[]
        {
            "Scenes", //場景
            "Scripts", //腳本
            "Textures", //貼圖
            "Sounds/BGM", //音訊(音樂)
            "Sounds/SE", //音訊(音效)
            "Materials", //材質
            "AssetBundles", //資源包
            "Prefabs", //預置物
            "AttributeData", //數據資料(Scriptable Objects)
            "VisualEffects/Animators", //視覺特效(動畫器)
            "VisualEffects/Animations", //視覺特效(動畫剪輯)
            "VisualEffects/Particle", //視覺特效(粒子特效)
            "Fonts" //字型
        };

        for (int i = 0; i < paths.Length; i++) //建立資料夾
        {
            Directory.CreateDirectory(rootPath + paths[i]);
        }

        AssetDatabase.Refresh(); //刷新(使之顯示)
    }
}
