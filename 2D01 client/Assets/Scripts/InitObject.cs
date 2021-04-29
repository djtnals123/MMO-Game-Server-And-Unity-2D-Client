using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitObject : MonoBehaviour
{
    // Start is called before the first frame update
    public static string playerNicname = null;
    private static GameObject playerPrefab;
    private static PlayerScript player = null;
    private static bool isFirst = true;
    private static bool _usePortal = true;
    public static Vector2 NextPosition { get; set; }
    //   private static int m_nextMap = -1;
    private static int _nextMapPortal = -1;
    public static bool MapCheck { get; set; }

    void Start()
    {
        if (isFirst)
        {
            playerPrefab = Resources.Load("Prefabs/Player") as GameObject;
            player = Instantiate(playerPrefab).GetComponent<PlayerScript>();
            player.GetComponent<PlayerScript>().ItsMe();
            player.GetComponent<PlayerScript>().NicNameText.text = playerNicname;

            GameObject uiPrefab = Resources.Load("Prefabs/UI/UI") as GameObject;
            GameObject ui = Instantiate(uiPrefab);
            DontDestroyOnLoad(ui);
            ui.name = "UI";

            MapCheck = false;
            isFirst = false;
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
        PacketManager.Instance.RequestPlayerList();

    }

    public static void WarpMap(int portal)
    {
        _nextMapPortal = portal;
        _usePortal = true;
        MapCheck = !MapCheck;
        Player.gameObject.SetActive(false);
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

    public static void InitPlayers(List<string> players, List<List<int>> equips, List<float> posX, List<float> posY, List<float> velX, List<float> velY, List<bool> flipX)
    {
        for (int i = 0; i < players.Count; i++)
        {
            GameObject playerObject = MonoBehaviour.Instantiate(playerPrefab) as GameObject;
            PlayerScript player = playerObject.GetComponent<PlayerScript>();
            Vector2 newPos = new Vector2(posX[i], posY[i]);
            player.CurPos = newPos;
            player.transform.position = newPos;
            playerObject.GetComponent<Rigidbody2D>().velocity = new Vector2(velX[i], velY[i]);
            player.name = "Player " + players[i];
            player.GetComponent<PlayerScript>().NicNameText.text = players[i];
            player.PutOnEquipment(equips[i]);
            player.GetComponent<SpriteRenderer>().flipX = flipX[i];
        }

        // add other info sync
    }

    public static void InitPlayer(string playerName, List<int> equips, float posX, float posY, bool flipX)
    {
        Debug.Log(flipX);
        Vector2 newPos = new Vector2(posX, posY);
        GameObject playerObject = MonoBehaviour.Instantiate(playerPrefab, newPos, new Quaternion()) as GameObject;
        PlayerScript player = playerObject.GetComponent<PlayerScript>();
        player.CurPos = newPos;
        player.transform.position = newPos;
        player.name = "Player " + playerName;
        player.GetComponent<PlayerScript>().NicNameText.text = playerName;
        player.PutOnEquipment(equips);
        player.GetComponent<SpriteRenderer>().flipX = flipX;
    }
    public static void InitPlayer(string playerName, List<int> equips, int portal, bool flipX)
    {
        Debug.Log("Portal " + portal);

        Vector2 newPos = GameObject.Find("Portal " + portal).transform.position;
        InitPlayer(playerName, equips, newPos.x, newPos.y, flipX);
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
