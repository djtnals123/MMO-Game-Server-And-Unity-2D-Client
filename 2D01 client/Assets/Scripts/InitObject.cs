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
        PacketManager.RequestPlayerList();
    }

    public static void InitPlayers(List<string> players)
    {
        foreach (string p in players)
        {
            Debug.Log("ips " + p);
            GameObject playerPrefab = Resources.Load("Player") as GameObject;
            GameObject Player = MonoBehaviour.Instantiate(playerPrefab) as GameObject;
            Player.name = "Player " + p;
            Player.GetComponent<PlayerScript>().NicNameText.text = p;
        }
    }

    public static void InitPlayer(string player)
    {
        Debug.Log("ip " + player);
        GameObject playerPrefab = Resources.Load("Player") as GameObject;
        GameObject Player = MonoBehaviour.Instantiate(playerPrefab) as GameObject;
        Player.name = "Player " + player;
        Player.GetComponent<PlayerScript>().NicNameText.text = player;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
