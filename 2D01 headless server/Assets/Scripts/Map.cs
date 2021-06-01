using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Map
{

    public LinkedList<PlayerScript> Players { get; set; }
    public LinkedList<RemoteObject> Mobs { get; set; }
    public LinkedList<RemoteObject> DeadMobs { get; set; } 
    public Scene SceneMap { get; set; }
    public bool WillSpawnMobs { get; set; }

    public Map()
    {
        Players = new LinkedList<PlayerScript>();
        Mobs = new LinkedList<RemoteObject>();
        DeadMobs = new LinkedList<RemoteObject>();
        WillSpawnMobs = false;
    }

}
