using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameData
{
    public static Dictionary<int, MapData.Map> Maps { get; set; }
    public static Dictionary<int, MobData.Mob> Mobs { get; set; }
}
