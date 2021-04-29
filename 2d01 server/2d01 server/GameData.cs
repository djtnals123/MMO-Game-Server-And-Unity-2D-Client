using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace _2d01_server
{
    class GameData
    {
        private static Dictionary<int, Map> maps = new Dictionary<int, Map>();

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

        public struct Vector2
        {
            public float x;
            public float y;

            public Vector2(float x, float y)
            {
                this.x = x;
                this.y = y;
            }
        }

        public static Dictionary<int, Map> Maps 
        {
            get
            {
                return maps;
            }
            set
            {
                maps = value;
            }
        }

        public struct Portal
        {
            public int linkedMap;
            public int linkedPortal;


        }



        public static void LoadXml()
        {
            ReadMap();
        }

        private static void ReadMap()
        {


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
                                        if (rd.Name == "Portal")
                                        {
                                            int portalCode = int.Parse(rd["code"]);
                                            Portal portal = new Portal();
                                            while (rd.Read())
                                            {
                                                if (rd.IsStartElement())
                                                {
                                                    if (rd.Name == "LinkedMap")
                                                    {
                                                        portal.linkedMap = int.Parse(rd["code"]);
                                                    }
                                                    else if (rd.Name == "LinkedPortal")
                                                    {
                                                        portal.linkedPortal = int.Parse(rd["code"]);
                                                    }
                                                }
                                                else break;
                                            }
                                            if (!Maps[mapCode].portal.ContainsKey(portalCode))
                                                Maps[mapCode].portal.Add(portalCode, portal);
                                        }
                                        else if (rd.Name == "SpawnPoint")
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
                    Console.WriteLine("Load File: \"" + file + "\"");
                }
            }
        }

        private static List<string> getXmlFiles(string forder)
        {
            List<string> fileNameList = new List<string>();
            forder = AppDomain.CurrentDomain.BaseDirectory + @"\xml\" + forder;
            DirectoryInfo di = new DirectoryInfo(forder);
            foreach (FileInfo File in di.GetFiles())
                if (File.Extension.ToLower().CompareTo(".xml") == 0)
                    fileNameList.Add(File.FullName);

            return fileNameList;
        }

    }
}
