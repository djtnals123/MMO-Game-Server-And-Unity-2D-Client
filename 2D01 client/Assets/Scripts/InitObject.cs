using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitObject : MonoBehaviour
{
    // Start is called before the first frame update
    public static string playerNicname = null;
    void Start()
    {
        GameObject playerPrefab = Resources.Load("Prefabs/Player") as GameObject;
        GameObject Player = MonoBehaviour.Instantiate(playerPrefab) as GameObject;
        Player.GetComponent<PlayerScript>().ItsMe();
        Player.GetComponent<PlayerScript>().NicNameText.text = playerNicname;
        PacketManager.RequestPlayerList();
    }

    public static void InitPlayers(List<string> players, List<List<int>> equips)
    {
        GameObject playerPrefab = Resources.Load("Prefabs/Player") as GameObject;
        for (int i = 0; i < players.Count; i++)
        {
            GameObject playerObject = MonoBehaviour.Instantiate(playerPrefab) as GameObject;
            PlayerScript player = playerObject.GetComponent<PlayerScript>();
            player.name = "Player " + players[i];
            player.GetComponent<PlayerScript>().NicNameText.text = players[i];
            player.PutOnEquipment(equips[i]);
        }

        // add other info sync
    }

    public static void InitPlayer(string playerName, List<int> equips)
    {
        Debug.Log("ip " + playerName);
        GameObject playerPrefab = Resources.Load("Prefabs/Player") as GameObject;
        GameObject playerObject = MonoBehaviour.Instantiate(playerPrefab) as GameObject;
        PlayerScript player = playerObject.GetComponent<PlayerScript>();
        player.name = "Player " + playerName;
        player.GetComponent<PlayerScript>().NicNameText.text = playerName;
        player.PutOnEquipment(equips);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
