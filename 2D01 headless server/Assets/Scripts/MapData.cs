using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;

public class MapData : MonoBehaviour
{
    void Start()
    {
        LoadXml();
        Debug.Log(Maps[1].portal[1].linkedMap);
        Debug.Log(Maps[1].spawnPoint[1].x);
    }

    public class Map
    {
        public Dictionary<int, Portal> portal { get; set; }
        public Dictionary<int, Vector2> spawnPoint { get; set; }

        public Map()
        {
            portal = new Dictionary<int, Portal>();
            spawnPoint = new Dictionary<int, Vector2>();
        }
    }

    public static Vector2 GetRandomSpawnPoint(int mapCode)
    {
        int spawnPointCode = new System.Random().Next(1, Maps[mapCode].spawnPoint.Count + 1);
        return Maps[mapCode].spawnPoint[spawnPointCode];
    }


    public static Dictionary<int, Map> Maps { get; set; }

    public struct Portal
    {
        public Vector2 position; 
        public int linkedMap;
        public int linkedPortal;
        


    }



    public static void LoadXml()
    {
        ReadMap();
    }

    private static void ReadMap()
    {
        Maps = new Dictionary<int, Map>();

        foreach (string file in getXmlFiles("map"))
        {
            using (XmlReader rd = XmlReader.Create(file))
            {
                while (rd.Read())
                {
                    if (rd.IsStartElement())
                    {
                        if (rd.Name == "Map")
                        {
                            int mapCode = int.Parse(rd["code"]);// rd.GetAttribute("code");
                            Maps.Add(mapCode, new Map());

                            while (rd.Read())
                            {
                                if (rd.IsStartElement())
                                {
                                    if (rd.Name == "Portals")
                                    {
                                        while (rd.Read())
                                        {
                                            if (rd.IsStartElement())
                                            {
                                                if (rd.Name == "Portal")
                                                {
                                                    Portal portal = new Portal();
                                                    int portalCode = int.Parse(rd["code"]);
                                                    portal.linkedMap = int.Parse(rd["LinkedMap"]);
                                                    portal.linkedPortal = int.Parse(rd["LinkedPortal"]);
                                                    portal.position = new Vector2(Convert.ToSingle(rd["PosX"]), Convert.ToSingle(rd["PosY"]));

                                                    if (!Maps[mapCode].portal.ContainsKey(portalCode))
                                                        Maps[mapCode].portal.Add(portalCode, portal);
                                                }
                                            }
                                            else break;
                                        }
                                    }
                                    else if (rd.Name == "SpawnPoints")
                                    {
                                        while (rd.Read())
                                        {
                                            if (rd.IsStartElement())
                                            {
                                                if (rd.Name == "SpawnPoint")
                                                {
                                                    int spawnPointCode = int.Parse(rd["code"]);
                                                    Vector2 spawnPoint = new Vector2(Convert.ToSingle(rd["PosX"]), Convert.ToSingle(rd["PosY"]));

                                                    if (!Maps[mapCode].spawnPoint.ContainsKey(spawnPointCode))
                                                        Maps[mapCode].spawnPoint.Add(spawnPointCode, spawnPoint);
                                                }
                                            }
                                            else break;
                                        }
                                    }
                                }
                                else break;
                            }
                        }
                    }
                    else break;
                }
            }
        }
    }

    private static List<string> getXmlFiles(string forder)
    {
        List<string> fileNameList = new List<string>();
        forder = Application.dataPath + "/Resources/Xml/" + forder;
        DirectoryInfo di = new DirectoryInfo(forder);
        foreach (FileInfo File in di.GetFiles())
            if (File.Extension.ToLower().CompareTo(".xml") == 0)
            {
                fileNameList.Add(File.FullName);Debug.Log(File.FullName);
            }
        

        return fileNameList;
    }
}
