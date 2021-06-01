using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public Dictionary<int, Map> Map { get; set; }
    public static MapManager Instance { get; set; }
    void Start()
    {
        Map = new Dictionary<int, Map>();
        Instance = this;
    }

    public void SpawnMobsIfPossible(int map)
    {
        if (!Map[map].WillSpawnMobs)
        {
            Map[map].WillSpawnMobs = true;
            StartCoroutine(SpawnMobs(map));
        }
    }

    IEnumerator SpawnMobs(int map)
    {
        yield return new WaitForSeconds(10f);
        Map[map].WillSpawnMobs = false;
        foreach (var mob in Map[map].DeadMobs)
        {
            mob.gameObject.SetActive(true);
            mob.transform.position = GameData.Maps[map].mobs[mob.ID].position;
            mob.HpRecovery();
            PacketManager.instance.ReSpawnMobs(map);
        }
        Map[map].DeadMobs.Clear();
    }
}
