using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InitObject : MonoBehaviour
{
    // Start is called before the first frame update
    public static string playerNicname = null;
    private static GameObject playerPrefab;
    public static GameObject HpBarPrefab { get; set; }
    private static PlayerScript player = null;
    public static Vector2 NextPosition { get; set; }
    //   private static int m_nextMap = -1;
    private static int _nextMapPortal = -1;
    public static bool MapCheck { get; set; }
    private static Scene remoteScene;
    public static List<RemoteObject> Mobs { get; set; } = null;
    public static int Map { get; set; }

    void Start()
    {
        playerPrefab = Resources.Load("Prefabs/Player") as GameObject;
        HpBarPrefab = Resources.Load("Prefabs/Hp Bar") as GameObject;
        remoteScene = SceneManager.GetSceneByName("Remote");
    }

    public static void WarpMap(int map, Vector2 position)
    {
   //     InitMob(map);
        LoadingSceneManager.LoadScene("Map_" + map.ToString());
        player.transform.position = position;
        player.CurPos = position;
        player.Rigid.velocity = Vector2.zero;
        MapCheck = !MapCheck;
        Map = map;
    }


    public static int NextMapPortal
    {
        get
        {
            return _nextMapPortal;
        }
        set
        {
            _nextMapPortal = value;
        }
    }

    public static void InitMob(int map)
    {
        Mobs = new List<RemoteObject>();
        Dictionary<int, GameObject> MobPrefabs = new Dictionary<int, GameObject>();
        foreach (var mobData in GameData.Maps[map].mobs)
        {
            if (!MobPrefabs.ContainsKey(mobData.code))
                MobPrefabs.Add(mobData.code, Resources.Load("Prefabs/Mob/Mob_" + mobData.code) as GameObject);
            SceneManager.SetActiveScene(remoteScene);
            GameObject mobObj = Instantiate(MobPrefabs[mobData.code], mobData.position, new Quaternion());
            var mob = mobObj.GetComponent<MobAI_1>();
            mob.MaxHP = GameData.Mobs[mobData.code].MaxHp;
            Mobs.Add(mob.GetComponent<RemoteObject>());
            mob.gameObject.SetActive(false);
        }
    }

    public static void InitMinePlayer(int map, Vector2 position, List<int> Equips, int maxHP, int hp)
    {
        player = Instantiate(playerPrefab).GetComponent<PlayerScript>();
        player.ItsMe();
        player.GetComponent<PlayerScript>().NicNameText.text = playerNicname;
        player.CurPos = position;
        player.transform.position = position;
        player.PutOnEquipment(Equips);
        player.MaxHP = maxHP;
        player.HP = hp;
        Map = map;
        Equipment.SetEquipment(Equips);
   //     InitMob(map);
        LoadingSceneManager.LoadScene("Map_" + map);
        MapCheck = false;
    }


    public static void InitObjects(List<string> players, List<List<int>> equips, List<float> posX, List<float> posY, List<float> velX, List<float> velY,
        List<bool> flipX, List<int> maxHP, List<int> hp, LinkedList<int> deadMobs, LinkedList<sbyte> nextMoves, LinkedList<float> mobPosX, LinkedList<float> mobPosY
        , LinkedList<short> mobHP)
    {
        InitMob(Map);
        LinkedList<RemoteObject> LiveMobs = new LinkedList<RemoteObject>(Mobs);
        foreach (var mob in deadMobs)
            LiveMobs.Remove(Mobs[mob-1]);

        for (int i = 0; i < players.Count; i++)
            InitPlayer(players[i], equips[i], new Vector2(posX[i], posY[i]), new Vector2(velX[i], velY[i]), flipX[i], maxHP[i], hp[i]);

        var LiveMobsIT = LiveMobs.GetEnumerator();
        var nextMovesIT = nextMoves.GetEnumerator();
        var mobPosXIT = mobPosX.GetEnumerator();
        var mobPosYIT = mobPosY.GetEnumerator();
        var mobHpIT = mobHP.GetEnumerator();
        while (LiveMobsIT.MoveNext() && nextMovesIT.MoveNext() && mobPosXIT.MoveNext() && mobPosYIT.MoveNext() && mobHpIT.MoveNext())
        {
            LiveMobsIT.Current.gameObject.SetActive(true);
            LiveMobsIT.Current.GetComponent<MobAI_1>().NextMove = nextMovesIT.Current;
            Debug.Log(nextMovesIT.Current);
            LiveMobsIT.Current.transform.position = new Vector2(mobPosXIT.Current, mobPosYIT.Current);
            LiveMobsIT.Current.HP = mobHpIT.Current;
        }


     //   SceneManager.UnloadSceneAsync("Loading");
        // add other info sync
    }

    public static PlayerScript InitPlayer(string playerName, List<int> equips, Vector2 position, Vector2 velocity, bool flipX, int maxHP, int hp)
    {
        GameObject playerObject = MonoBehaviour.Instantiate(playerPrefab, position, new Quaternion()) as GameObject;
        PlayerScript player = playerObject.GetComponent<PlayerScript>();
        player.CurPos = position;
        player.transform.position = position;
        player.name = "Player " + playerName;
        player.NicNameText.text = playerName;
        player.PutOnEquipment(equips);
        player.SR.flipX = flipX;
        player.MaxHP = maxHP;
        player.HP = hp;
        SceneManager.MoveGameObjectToScene(player.gameObject, remoteScene);
        return player;
    }

    public static void ReSpawnMobs()
    {
        foreach(var mob in Mobs)
        {
            mob.gameObject.SetActive(true);
            mob.transform.position = GameData.Maps[Map].mobs[mob.ID].position;
            mob.HpRecovery();
        }
    }

    public static PlayerScript Player
    {
        get
        {
            return player;
        }
        set
        {
            player = value;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
