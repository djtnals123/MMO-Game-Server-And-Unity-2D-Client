using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InitObject : MonoBehaviour
{
    public static bool MapCheck { get; set; }
    private static GameObject playerPrefab;

    public static Dictionary<string, PlayerScript> UserList { get; set; } // key-remoteEP.toString()
    public static Dictionary<string, PlayerScript> UserListByName { get; set; } // key-name
    private static LinkedList<RemoteObject> initObjectList = new LinkedList<RemoteObject>();
    private static readonly LoadSceneParameters loadSceneParameters = new LoadSceneParameters(LoadSceneMode.Additive, LocalPhysicsMode.Physics2D);
    private static Scene tempScene;


    void Start()
    {
        tempScene = SceneManager.LoadScene("Temp", loadSceneParameters);
        UserList = new Dictionary<string, PlayerScript>();
        UserListByName = new Dictionary<string, PlayerScript>();
        playerPrefab = Resources.Load("Prefabs/Player") as GameObject;


    }



    public static void InitPlayer(int map, string playerName,int maxHP, int hp, List<int> equips, Vector2 position, string remoteEP)
    {
        if (!MapManager.Instance.Map.ContainsKey(map))
            MapManager.Instance.Map.Add(map, new Map());

        Scene scene = SceneManager.GetSceneByName("Map_" + map);
        bool isLoaded;
        if (scene.isLoaded)
        {
            SceneManager.SetActiveScene(SceneManager.GetSceneByName("Map_" + map));
            isLoaded = true;
        }
        else
        {
            SceneManager.SetActiveScene(tempScene);
            MapManager.Instance.Map[map].SceneMap = SceneManager.LoadScene("Map_" + map, loadSceneParameters);
                InitMob(map);
            isLoaded = false;
        }

        GameObject playerObject = Instantiate(playerPrefab, position, new Quaternion());
        PlayerScript player = playerObject.GetComponent<PlayerScript>();
        if (!UserList.ContainsKey(remoteEP))
        {
            UserList.Add(remoteEP, player);
            UserListByName.Add(playerName, player);
        }
        MapManager.Instance.Map[map].Players.AddLast(player);
        player.Map = map;
        player.CurPos = position;
        player.transform.position = position;
        player.name = "Player " + playerName;
        player.nicname = playerName;
        player.PutOnEquipment(equips);
        player.MaxHP = maxHP;
        player.HP = hp;
        string[] splitRemoteEP = remoteEP.Split(':');
        player.RemoteEP = new IPEndPoint(IPAddress.Parse(splitRemoteEP[0]), int.Parse(splitRemoteEP[1]));

        if (!isLoaded)
        {
            initObjectList.AddLast(player);
            player.gameObject.SetActive(false);
        }
    }

    public static void InitMob(int map)
    {
        Dictionary<int, GameObject> MobPrefabs = new Dictionary<int, GameObject>();
        foreach (var key in GameData.Maps[map].mobs.Keys)
        {
            var mobData = GameData.Maps[map].mobs;
            if (!MobPrefabs.ContainsKey(mobData[key].code))
                MobPrefabs.Add(mobData[key].code, Resources.Load("Prefabs/Mob/Mob_" + mobData[key].code) as GameObject);
            RemoteObject mob = Instantiate(MobPrefabs[mobData[key].code], mobData[key].position, new Quaternion()).GetComponent<RemoteObject>();
            mob.Initialise(mobData[key].id, map, GameData.Mobs[mobData[key].id].MaxHp);

            initObjectList.AddLast(mob);
            MapManager.Instance.Map[map].Mobs.AddLast(mob.GetComponent<RemoteObject>());
        }
    }

    public static void WarpMap(PlayerScript player, int portal)
    {
        player.Rigid.velocity = Vector2.zero;
        MapManager.Instance.Map[player.Map].Players.Remove(player);

        int linkedMap = GameData.Maps[player.Map].portal[portal].linkedMap;
        int linkedPortal = GameData.Maps[player.Map].portal[portal].linkedPortal;

        player.Map = linkedMap;
        player.CheckMap = !player.CheckMap;
        player.CurPos = GameData.Maps[linkedMap].portal[linkedPortal].position;
        player.transform.position = player.CurPos;

        if (!MapManager.Instance.Map.ContainsKey(player.Map))
            MapManager.Instance.Map.Add(player.Map, new Map());

        MapManager.Instance.Map[player.Map].Players.AddLast(player);

        Scene scene = SceneManager.GetSceneByName("Map_" + player.Map);
        if (scene.isLoaded)
        {
            SceneManager.MoveGameObjectToScene(player.gameObject, scene);
        }
        else
        {
            SceneManager.SetActiveScene(tempScene);

            MapManager.Instance.Map[player.Map].SceneMap = SceneManager.LoadScene("Map_" + player.Map, loadSceneParameters);
            InitMob(player.Map);
            initObjectList.AddLast(player);
            player.gameObject.SetActive(false);
        }
    }


    // Update is called once per frame
    void Update()
    {
        var toRemove = new List<RemoteObject>();
        foreach (RemoteObject obj in initObjectList)
        {
            Scene scene = MapManager.Instance.Map[obj.Map].SceneMap;
            if (scene.isLoaded)
            {
                toRemove.Add(obj);
                SceneManager.MoveGameObjectToScene(obj.gameObject, scene);
                obj.gameObject.SetActive(true);
            }
        }
        foreach (RemoteObject remove in toRemove)
        {
            initObjectList.Remove(remove);
        }
    }
}
