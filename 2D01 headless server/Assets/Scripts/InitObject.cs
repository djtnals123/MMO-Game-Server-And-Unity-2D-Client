using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InitObject : MonoBehaviour
{
    private static bool _usePortal = true;
    public static bool MapCheck { get; set; }
    private static GameObject playerPrefab;

    public static Dictionary<string, PlayerScript> UserList { get; set; } // key-remoteEP.toString()
    public static Dictionary<string, PlayerScript> UserListByName { get; set; } // key-name
    public static Dictionary<int, LinkedList<PlayerScript>> Map { get; set; }
    public static Dictionary<int, Scene> SceneMap { get; set; }
    private static LinkedList<PlayerScript> initPlayerList = new LinkedList<PlayerScript>();
    private static readonly LoadSceneParameters loadSceneParameters = new LoadSceneParameters(LoadSceneMode.Additive, LocalPhysicsMode.Physics2D);
    private static Scene tempScene;


    void Start()
    {
        tempScene = SceneManager.LoadScene("Temp", loadSceneParameters);
        UserList = new Dictionary<string, PlayerScript>();
        UserListByName = new Dictionary<string, PlayerScript>();
        Map = new Dictionary<int, LinkedList<PlayerScript>>();
        SceneMap = new Dictionary<int, Scene>();
        playerPrefab = Resources.Load("Prefabs/Player") as GameObject;

        if (_usePortal)
        {/*
            Vector2 newPos = GameObject.Find("Portal " + _nextMapPortal).transform.position;
            player.transform.position = newPos;
            player.CurPos = newPos;
            InitObject.Player.gameObject.SetActive(true); */
        }
        else
        {
      //      player.transform.position = NextPosition;
        }

    }



    public static void InitPlayer(int map, string playerName, List<int> equips, Vector2 position, string remoteEP)
    {
        Scene scene = SceneManager.GetSceneByName("Map_" + map);
        bool isLoaded;
        if (scene.isLoaded)
        {
            SceneManager.SetActiveScene(SceneManager.GetSceneByName("Map_" + map));
            isLoaded = true;
        }
        else
        {
            SceneMap.Add(map, SceneManager.LoadScene("Map_" + map, loadSceneParameters));
            SceneManager.SetActiveScene(tempScene);
            isLoaded = false;
        }

        GameObject playerObject = Instantiate(playerPrefab, position, new Quaternion());
        PlayerScript player = playerObject.GetComponent<PlayerScript>();
        if (!UserList.ContainsKey(remoteEP))
        {
            UserList.Add(remoteEP, player);
            UserListByName.Add(playerName, player);
        }
        if (!Map.ContainsKey(map))
            Map.Add(map, new LinkedList<PlayerScript>());
        Map[map].AddLast(player);
        player.Map = map;
        player.CurPos = position;
        player.transform.position = position;
        player.name = "Player " + playerName;
        player.nicname = playerName;
        player.PutOnEquipment(equips);
        string[] splitRemoteEP = remoteEP.Split(':');
        player.RemoteEP = new IPEndPoint(IPAddress.Parse(splitRemoteEP[0]), int.Parse(splitRemoteEP[1]));

        if (!isLoaded)
        {
            initPlayerList.AddLast(player);
            player.gameObject.SetActive(false);
        }
    }
    public static void WarpMap(PlayerScript player, int portal)
    {
        player.RB.velocity = Vector2.zero;
        Map[player.Map].Remove(player);

        int linkedMap = MapData.Maps[player.Map].portal[portal].linkedMap;
        int linkedPortal = MapData.Maps[player.Map].portal[portal].linkedPortal;

        player.Map = linkedMap;
        player.CheckMap = !player.CheckMap;
        player.CurPos = MapData.Maps[linkedMap].portal[linkedPortal].position;
        player.transform.position = player.CurPos;

        if (!Map.ContainsKey(player.Map))
            Map.Add(player.Map, new LinkedList<PlayerScript>());
        Map[player.Map].AddLast(player);

        Scene scene = SceneManager.GetSceneByName("Map_" + player.Map);
        if (scene.isLoaded)
        {
            SceneManager.MoveGameObjectToScene(player.gameObject, scene);
        }
        else
        {
            SceneMap.Add(player.Map, SceneManager.LoadScene("Map_" + player.Map, loadSceneParameters));
            initPlayerList.AddLast(player);
            player.gameObject.SetActive(false);
        }
    }


    // Update is called once per frame
    void Update()
    {
        var toRemove = new List<PlayerScript>();
        foreach (PlayerScript player in initPlayerList)
        {
            Scene scene = SceneMap[player.Map];
            if (scene.isLoaded)
            {
                Debug.Log("add");
                
                toRemove.Add(player);
                SceneManager.MoveGameObjectToScene(player.gameObject, scene);
                player.gameObject.SetActive(true);
            }
        }
        foreach (PlayerScript remove in toRemove)
        {
            initPlayerList.Remove(remove);
        }
    }
}
