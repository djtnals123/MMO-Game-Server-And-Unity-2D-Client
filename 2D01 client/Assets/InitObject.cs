using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitObject : MonoBehaviour
{
    // Start is called before the first frame update
    public static string playerNicname = null;
    void Start()
    {
        GameObject playerPrefab = Resources.Load("Player") as GameObject;
        GameObject Player = MonoBehaviour.Instantiate(playerPrefab) as GameObject;
        Player.GetComponent<PlayerScript>().ItsMe();
        Player.GetComponent<PlayerScript>().NicNameText.text = playerNicname;
        
        PacketManager.InitPlayers();

    }

    public static void InitPlayer(string[] players)
    {
        foreach (string p in players)
        {
            if (p == "INIT_PLAYERS")
                continue;
            GameObject playerPrefab = Resources.Load("Player") as GameObject;
            GameObject Player = MonoBehaviour.Instantiate(playerPrefab) as GameObject;
            Player.name = "Player " + p;
            Player.GetComponent<PlayerScript>().NicNameText.text = p;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
