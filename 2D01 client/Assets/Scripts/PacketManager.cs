using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using ClientPacket;
using ZeroFormatter;

public class PacketManager : MonoBehaviour
{
    private static UdpClient Client = new UdpClient();
    

    void Start()
    {
   //     InvokeRepeating("ReceivePacket", 0f, 0.01f);
        DontDestroyOnLoad(gameObject);
    }
    private void Update()
    {
        ReceivePacket();
    }

    public static void SendPacket(ClientPacket.Packet clientPacket)
    {
        var serialize = ZeroFormatterSerializer.Serialize<ClientPacket.Packet>(clientPacket);
        Client.Send(serialize, serialize.Length, "127.0.0.1", 7777); // 데이터 송신
    }

    public void ReceivePacket()
    {
        if(Client.Available != 0)
        {
            IPEndPoint epRemote = new IPEndPoint(IPAddress.Any, 0); // 데이터 수신
            byte[] responsePacket = Client.Receive(ref epRemote);
            ServerPacket.Packet deserialize = ZeroFormatterSerializer.Deserialize<ServerPacket.Packet>(responsePacket);
            
            switch (deserialize.packetType)
            {
                case ServerPacket.PacketType.ObjectSynchronization:
                    ServerPacket.ObjectSynchronization objInfo = (ServerPacket.ObjectSynchronization)deserialize;
                    GameObject player = GameObject.Find("Player " + objInfo.name);
                    Debug.Log(objInfo.name);
                    if (player != null)
                    {
                        player.GetComponent<PlayerScript>().SyncPlayer(objInfo.name, objInfo.positionX, objInfo.positionY, 
                            objInfo.velocityX, objInfo.velocityY, objInfo.rotation, objInfo.angularVelocity, objInfo.flipX);
                    }
                    break;
                case ServerPacket.PacketType.AttackPlayer:
                    ServerPacket.AttackPlayer atkPlayer = (ServerPacket.AttackPlayer)deserialize;
                    player = GameObject.Find("Player " + atkPlayer.id);
                    if (player != null)
                    {
                        player.GetComponent<PlayerScript>().Attack();
                    }

                    break;
                case ServerPacket.PacketType.LoginSuccess:
                    UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene");
                    break;
                case ServerPacket.PacketType.InitPlayer:
                    Debug.Log("ff");
                    InitObject.InitPlayer(((ServerPacket.InitPlayer)deserialize).player);
                    break;
                case ServerPacket.PacketType.InitPlayers:
                    Debug.Log("ffs");
                    InitObject.InitPlayers(((ServerPacket.InitPlayers)deserialize).players);
                    break;
                case ServerPacket.PacketType.SetInventories:
                    Debug.Log("dse");
                    ServerPacket.SetInventories inventories = (ServerPacket.SetInventories)deserialize;
                    List<RectTransform> list = new List<RectTransform>();
                    GameObject Slot = Resources.Load("Slot") as GameObject;
                    RectTransform r = Slot.GetComponent<RectTransform>();
                    Transform parent = GameObject.Find("Slots").transform;

                    for (int i = 0; inventories.maxInventorySlot > i; i++)
                    {
                        list.Add(MonoBehaviour.Instantiate(r));
                        list[i].transform.SetParent(parent);
                        list[i].transform.localScale = new Vector2(1f, 1f);
                    }
                    for (int i = 0; i < inventories.item.Count; i++)
                    {
                        list[i].GetComponent<SlotScript>().SetItem(inventories.item[i]);
                    }
                    break;
                case ServerPacket.PacketType.HpSynchronization:
                    ServerPacket.HpSynchronization hpSyn = (ServerPacket.HpSynchronization)deserialize;
                    if (hpSyn.id == InitObject.playerNicname)
                        player = GameObject.Find("Player(Clone)");
                    else player = GameObject.Find("Player " + hpSyn.id);
                    if (player != null)
                    {
                        PlayerScript playerComponent = player.GetComponent<PlayerScript>();
                        playerComponent.HealthImage.fillAmount = (float)hpSyn.hp/hpSyn.maxHp;
                        if (playerComponent.HealthImage.fillAmount <= 0)
                            Destroy(playerComponent);
                    }
                    break;
                case ServerPacket.PacketType.ConnectionCheck:
                    ConnectionAck();
                    break;
                case ServerPacket.PacketType.DisconnectedPlayer:
                    ServerPacket.DisconnectedPlayer removePlayer = (ServerPacket.DisconnectedPlayer)deserialize;
                    Debug.Log(removePlayer.id);
                    Destroy(GameObject.Find("Player " + removePlayer.id));
                    break;
                default:
                    break;
            }
        }
    }

    public static void ConnectionAck() 
        => SendPacket(new ClientPacket.ConnectionAck());

    public static void Login(string id, string pass) 
        => SendPacket(new ClientPacket.Login { id = id, pass = pass });

    public static void getInventory(string id) 
        => SendPacket(new ClientPacket.GetInventories { id = id });
    public static void AttackPlayer(string id)
        => SendPacket(new ClientPacket.AttackPlayer { id = id });

    public static void RequestPlayerList()
        => SendPacket(new ClientPacket.RequestPlayerList() { id = InitObject.playerNicname });
   
    public static void HpSynchronization(int hp)
        => SendPacket(new ClientPacket.HpSynchronization { hp = hp });
    
    public static void ObjectSynchronization(string name, float posX, float posY, float velX, float velY, float rotation, float angularVel, bool flipX)
    {
        var clientPacket = new ClientPacket.ObjectSynchronization
        {
            name = name,
            positionX = posX,
            positionY = posY,
            velocityX = velX,
            velocityY = velY,
            rotation = rotation,
            angularVelocity = angularVel,
            flipX = flipX
        };
        SendPacket(clientPacket);
    }

    ~PacketManager()
    {
        SendPacket(new ClientPacket.Disconnected());
    }
}
