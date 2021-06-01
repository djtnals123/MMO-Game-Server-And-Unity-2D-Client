using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class MobData
{
    public struct Mob
    {
        public int MaxHp { get; set; }

        public Mob(int maxHp)
        {
            MaxHp = maxHp;
        }
    }
    public static Dictionary<int, Mob> LoadXml()
    {
        Dictionary<int, Mob> Mobs = new Dictionary<int, Mob>();
        foreach (string file in XmlManager.getXmlFiles("mob"))
        {
            using (XmlReader rd = XmlReader.Create(file))
            {
                while (rd.Read())
                {
                    if (rd.IsStartElement())
                    {
                        if (rd.Name == "Mob")
                        {
                            int mobCode = int.Parse(rd["code"]);// rd.GetAttribute("code");
                            int maxHp = -1;

                            while (rd.Read())
                            {
                                if (rd.IsStartElement())
                                {
                                    if (rd.Name == "info")
                                    {
                                        while (rd.Read())
                                        {
                                            if (rd.IsStartElement())
                                            {
                                                if (rd.Name == "maxHp")
                                                {
                                                    maxHp = int.Parse(rd["value"]);
                                                }
                                            }
                                            else break;
                                        }
                                    }
                                }
                                else break;
                            }
                            Mobs.Add(mobCode, new Mob(maxHp)) ;

                        }
                    }
                    else break;
                }
            }
        }
        return Mobs;
    }
}
