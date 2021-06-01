using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class XmlManager : MonoBehaviour
{
    void Start()
    {
        System.Console.WriteLine("st");
        LoadXml();
    }

    private void LoadXml()
    {
        GameData.Maps = MapData.LoadXml();
        GameData.Mobs = MobData.LoadXml();
    }



    public static List<string> getXmlFiles(string forder)
    {
        List<string> fileNameList = new List<string>();
        forder = Application.dataPath + "/Resources/Xml/" + forder;
        DirectoryInfo di = new DirectoryInfo(forder);
        foreach (FileInfo File in di.GetFiles())
        {
            if (File.Extension.ToLower().CompareTo(".xml") == 0)
            {
                fileNameList.Add(File.FullName);
                Debug.Log(File.FullName);
            }
        }

        return fileNameList;
    }
}
