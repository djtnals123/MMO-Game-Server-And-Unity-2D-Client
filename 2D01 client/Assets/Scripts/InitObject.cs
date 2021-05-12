using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InitObject : MonoBehaviour
{
    // Start is called before the first frame update
    public static string playerNicname = null;
    private static GameObject playerPrefab;
    private static PlayerScript player = null;
    private static bool isFirst = true;
    private static bool _usePortal = false;
    public static Vector2 NextPosition { get; set; }
    //   private static int m_nextMap = -1;
    private static int _nextMapPortal = -1;
    public static bool MapCheck { get; set; }
    private static Scene remoteScene;
    public static LinkedList<GameObject> DisabledRemoteObjectList { get; set; } = null;

    void Start()
    {
        if (isFirst)
        {
            playerPrefab = Resources.Load("Prefabs/Player") as GameObject;
            isFirst = false;
            remoteScene = SceneManager.GetSceneByName("Remote");
        }
        else
        {
            if (_usePortal)
            {
                Vector2 newPos = GameObject.Find("Portal " + _nextMapPortal).transform.position;
                player.transform.position = newPos;
                player.CurPos = newPos;
                InitObject.Player.gameObject.SetActive(true);
            }
            else
            {
                player.transform.position = NextPosition;
            }
        }
   //     PacketManager.Instance.RequestPlayerList();

    }

    public static void WarpMap(int map, Vector2 position)
    {
        LoadingSceneManager.LoadScene("Map_" + map.ToString());
        player.transform.position = position;
        player.CurPos = position;
        player.RB.velocity = Vector2.zero;
        MapCheck = !MapCheck;
    }

    public static void SpawnMap(Vector2 position)
    {
        NextPosition = position;
        _usePortal = false;
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

    public static void InitMinePlayer(Vector2 position, List<int> Equips)
    {
        player = Instantiate(playerPrefab).GetComponent<PlayerScript>();
        player.ItsMe();
        player.GetComponent<PlayerScript>().NicNameText.text = playerNicname;
        player.CurPos = position;
        player.transform.position = position;
        player.PutOnEquipment(Equips);
        Equipment.SetEquipment(Equips);

        MapCheck = false;
    }


    public static void InitPlayers(List<string> players, List<List<int>> equips, List<float> posX, List<float> posY, List<float> velX, List<float> velY, List<bool> flipX)
    {

        if (LoadingSceneManager.IsLoading)
        {
            DisabledRemoteObjectList = new LinkedList<GameObject>();

            for (int i = 0; i < players.Count; i++)
            {
                PlayerScript player = InitPlayer(players[i], equips[i], new Vector2(posX[i], posY[i]), new Vector2(velX[i], velY[i]), flipX[i]);
                DisabledRemoteObjectList.AddLast(player.gameObject);
                player.gameObject.SetActive(false);
            }
        }
        else
        {
            for (int i = 0; i < players.Count; i++)
                InitPlayer(players[i], equips[i], new Vector2(posX[i], posY[i]), new Vector2(velX[i], velY[i]), flipX[i]);
        }


        // add other info sync
    }

    public static PlayerScript InitPlayer(string playerName, List<int> equips, Vector2 position, Vector2 velocity, bool flipX)
    {
        GameObject playerObject = MonoBehaviour.Instantiate(playerPrefab, position, new Quaternion()) as GameObject;
        PlayerScript player = playerObject.GetComponent<PlayerScript>();
        player.CurPos = position;
        player.transform.position = position;
        player.RB.velocity = velocity;
        player.name = "Player " + playerName;
        player.NicNameText.text = playerName;
        player.PutOnEquipment(equips);
        player.SR.flipX = flipX;
        SceneManager.MoveGameObjectToScene(player.gameObject, remoteScene);
        return player;
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
