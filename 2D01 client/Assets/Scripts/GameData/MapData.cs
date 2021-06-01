using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;

public class MapData
{

    public class Map
    {
        public Dictionary<int, Portal> portal { get; set; }
        public Dictionary<int, Vector2> spawnPoint { get; set; }
        public List<Mob> mobs { get; set; }


        public Map()
        {
            portal = new Dictionary<int, Portal>();
            spawnPoint = new Dictionary<int, Vector2>();
            mobs = new List<Mob>();
        }
    }

    public static Vector2 GetRandomSpawnPoint(int mapCode)
    {
        int spawnPointCode = new System.Random().Next(1, GameData.Maps[mapCode].spawnPoint.Count + 1);
        return GameData.Maps[mapCode].spawnPoint[spawnPointCode];
    }



    public struct Portal
    {
        public Vector2 position;
        public int linkedMap;
        public int linkedPortal;



    }
    public struct Mob
    {
        public int id;
        public int code;
        public Vector2 position;
    }




    public static Dictionary<int, Map> LoadXml()
    {
        Dictionary<int, Map> Maps = new Dictionary<int, Map>();

        foreach (string file in XmlManager.getXmlFiles("map"))
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
                                    else if (rd.Name == "Mobs")
                                    {
                                        while (rd.Read())
                                        {
                                            if (rd.IsStartElement())
                                            {
                                                if (rd.Name == "Mob")
                                                {
                                                    Vector2 spawnPoint = new Vector2(Convert.ToSingle(rd["PosX"]), Convert.ToSingle(rd["PosY"]));
                                                    Mob mob = new Mob();
                                                    mob.id = int.Parse(rd["id"]);
                                                    mob.code = int.Parse(rd["code"]);
                                                    mob.position = spawnPoint;


                                                    Maps[mapCode].mobs.Add(mob);
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
        return Maps;
    }
}
