using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using ClientPacket;
using ZeroFormatter;
using System.Collections.Concurrent;
using System.Threading.Tasks;

public class PacketManager : MonoBehaviour
{
    private UdpClient Client = new UdpClient();
    ConcurrentQueue<UdpReceiveResult> DgramQueue = new ConcurrentQueue<UdpReceiveResult>();

    private static PacketManager _instance = null;
    public static PacketManager Instance { 
        get
        {
            return _instance; 
        } 
        private set
        {
            _instance = value;
        }
    }

    private readonly IPEndPoint hRemoteEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7778);
    private readonly IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7777);

    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Task.Run(ReceivePacket);
        }
        else Destroy(this);
    }
    private void Update()
    {
        PacketHandler();
    }

    public void SendPacket(ClientPacket.Packet clientPacket)
    {
        var serialize = ZeroFormatterSerializer.Serialize<ClientPacket.Packet>(clientPacket);
        Client.Send(serialize, serialize.Length, remoteEP); // 데이터 송신
    }
    public void SendPacket(HClientPacket.Packet clientPacket)
    {
        var serialize = ZeroFormatterSerializer.Serialize<HClientPacket.Packet>(clientPacket);
        Client.Send(serialize, serialize.Length, hRemoteEP); // 데이터 송신
    }


    private void ReceivePacket()
    {
        while(true)
        {
            var responsePacket = Client.ReceiveAsync();
            DgramQueue.Enqueue(responsePacket.Result);
        }
    }

    public void PacketHandler()
    {
        UdpReceiveResult result;
        while (DgramQueue.TryDequeue(out result))
        {
            Debug.Log(result.RemoteEndPoint.Address + ":" + result.RemoteEndPoint.Port);
            if (result.RemoteEndPoint.ToString() == "127.0.0.1:7777")
            {
                ServerPacket.Packet deserialize = ZeroFormatterSerializer.Deserialize<ServerPacket.Packet>(result.Buffer);
                PlayerScript player;
                switch (deserialize.packetType)
                {
                    case ServerPacket.PacketType.PlayerSetting:
                        ServerPacket.PlayerSetting playersetting = (ServerPacket.PlayerSetting)deserialize;

                        Inventory.SetInventories(playersetting.Item, playersetting.Type, playersetting.Count, playersetting.Slot, playersetting.MaxEquipmentSlot, playersetting.MaxUseableSlot, playersetting.MaxEtcSlot, playersetting.MaxEnhancementSlot);


                        break;
                    case ServerPacket.PacketType.HpSynchronization:
                        ServerPacket.HpSynchronization hpSyn = (ServerPacket.HpSynchronization)deserialize;
                        if (hpSyn.Id == InitObject.playerNicname)
                            player = GameObject.Find("Player(Clone)").GetComponent<PlayerScript>();
                        else player = GameObject.Find("Player " + hpSyn.Id).GetComponent<PlayerScript>();
                        if (player != null)
                        {
                            PlayerScript playerComponent = player.GetComponent<PlayerScript>();
                            playerComponent.HealthImage.fillAmount = (float)hpSyn.Hp / hpSyn.MaxHp;
                            if (playerComponent.HealthImage.fillAmount <= 0)
                                Destroy(playerComponent);
                        }
                        break;
                    case ServerPacket.PacketType.ConnectionCheck:
                        ConnectionAck();
                        break;
                    case ServerPacket.PacketType.DisconnectedPlayer:
                        ServerPacket.DisconnectedPlayer removePlayer = (ServerPacket.DisconnectedPlayer)deserialize;
                        Debug.Log(removePlayer.Id);
                        Destroy(GameObject.Find("Player " + removePlayer.Id));
                        break;
                    case ServerPacket.PacketType.ChangeItemSlot:
                        ServerPacket.ChangeItemSlot changeItemSlot = (ServerPacket.ChangeItemSlot)deserialize;
                        Inventory.SwapSlot(changeItemSlot.Type, changeItemSlot.Slot1, changeItemSlot.Slot2);

                        break;
                    case ServerPacket.PacketType.UseItem:
                        ServerPacket.UseItem useItem = (ServerPacket.UseItem)deserialize;
                        Inventory.UseItem(useItem.Type, useItem.Slot, useItem.Item);

                        break;
                    case ServerPacket.PacketType.PutOnPlayer:
                        ServerPacket.PutOnPlayer putOnPlayer = (ServerPacket.PutOnPlayer)deserialize;
                        player = GameObject.Find("Player " + putOnPlayer.Player).GetComponent<PlayerScript>();
                        player.PutOnEquipment(putOnPlayer.Item);

                        break;
                    case ServerPacket.PacketType.TakeOffEquipment:
                        ServerPacket.TakeOffEquipment takeOffEquipment = (ServerPacket.TakeOffEquipment)deserialize;
                        Equipment.TakeOff(takeOffEquipment.SubType, takeOffEquipment.InventorySlot);
                        player = GameObject.Find("Player(Clone)").GetComponent<PlayerScript>();
                        player.TakeOffEquipment(takeOffEquipment.SubType);
                        break;
                    case ServerPacket.PacketType.TakeOffPlayer:
                        ServerPacket.TakeOffPlayer takeOffPlayer = (ServerPacket.TakeOffPlayer)deserialize;
                        player = GameObject.Find("Player " + takeOffPlayer.Player).GetComponent<PlayerScript>();
                        player.TakeOffEquipment(takeOffPlayer.SubType);
                        break;
                    default:
                        break;
                }
            }
            else if(result.RemoteEndPoint.ToString() == "127.0.0.1:7778")
            {
                HServerPacket.Packet deserialize = ZeroFormatterSerializer.Deserialize<HServerPacket.Packet>(result.Buffer);

                switch (deserialize.packetType)
                {
                case HServerPacket.PacketType.ObjectSynchronization:
                        HServerPacket.ObjectSynchronization objInfo = (HServerPacket.ObjectSynchronization)deserialize;
              //          Debug.Log(" : " + objInfo.PositionX + " " + objInfo.PositionY + "Player " + objInfo.Name);
                        GameObject playerObject = GameObject.Find("Player " + objInfo.Name);
                        if (playerObject != null)
                            playerObject.GetComponent<PlayerScript>().SyncPlayer(objInfo.PositionX, objInfo.PositionY,
                                objInfo.VelocityX, objInfo.VelocityY, objInfo.Rotation, objInfo.AngularVelocity, objInfo.FlipX);
                        break;
                    case HServerPacket.PacketType.AttackPlayer:
                        HServerPacket.AttackPlayer atkPlayer = (HServerPacket.AttackPlayer)deserialize;
                        PlayerScript player = GameObject.Find("Player " + atkPlayer.Id).GetComponent<PlayerScript>();
                        player.KeyDownSpace = true;
                        break;
                    case HServerPacket.PacketType.EnableSpace:
                        HServerPacket.EnableSpace enableSpace = (HServerPacket.EnableSpace)deserialize;
                        player = GameObject.Find("Player " + enableSpace.Player).GetComponent<PlayerScript>();
                        player.EnableSpace = enableSpace.IsEnabled;
                        break;
                    case HServerPacket.PacketType.LoginSuccess:
                        HServerPacket.LoginSuccess loginSuccess = (HServerPacket.LoginSuccess)deserialize;
                        InitObject.InitMinePlayer(new Vector2(loginSuccess.PositionX, loginSuccess.PositionY), loginSuccess.Equips);
                        LoadingSceneManager.LoadScene("Map_" + loginSuccess.Map.ToString());
                        break;
                    case HServerPacket.PacketType.InitPlayer:
                        HServerPacket.InitPlayer initPlayer = (HServerPacket.InitPlayer)deserialize;
                        if (initPlayer.MapCheck == InitObject.MapCheck)
                            InitObject.InitPlayer(initPlayer.Player, initPlayer.Equips, new Vector2(initPlayer.PositionX, initPlayer.PositionY), Vector2.zero, initPlayer.FlipX);
                        break;

                    case HServerPacket.PacketType.InitPlayers:
                        HServerPacket.InitPlayers initPlayers = (HServerPacket.InitPlayers)deserialize;
                        InitObject.InitPlayers(initPlayers.Players, initPlayers.Equips, initPlayers.PositionX, initPlayers.PositionY, initPlayers.VelocityX, initPlayers.VelocityY, initPlayers.FlipX);
                        break;
                    case HServerPacket.PacketType.WarpMap:
                        HServerPacket.WarpMap warpMap = (HServerPacket.WarpMap)deserialize;
                        InitObject.WarpMap(warpMap.LinkedMap, new Vector2(warpMap.PosX, warpMap.PosY));

                        break;
                    default:
                        break;
                }

            }
        }
    }

    public void ConnectionAck() =>
        SendPacket(new ClientPacket.ConnectionAck());

    public void Login(string id, string pass) =>
        SendPacket(new ClientPacket.Login { Id = id, Pass = pass });

    public void PlayerSetting(string id) =>
        SendPacket(new ClientPacket.PlayerSetting { Id = id });
    public void AttackPlayer() =>
        SendPacket(new HClientPacket.AttackPlayer());


    public void HpSynchronization(int hp) =>
        SendPacket(new ClientPacket.HpSynchronization { Hp = (short)hp });

    public void ObjectSynchronization(string name, float posX, float posY, float velX, float velY, float rotation, float angularVel, bool flipX, bool mapCheck) =>
        SendPacket(new HClientPacket.ObjectSynchronization
        {
            PositionX = posX,
            PositionY = posY,
            VelocityX = velX,
            VelocityY = velY,
            Rotation = rotation,
            AngularVelocity = angularVel,
            FlipX = flipX,
            MapCheck = mapCheck
        });


    public void ChangeItemSlot(sbyte tab, short slot1, short slot2) =>
        SendPacket(new ClientPacket.ChangeItemSlot
        {
            Type = tab,
            Slot1 = slot1,
            Slot2 = slot2
        });

    public void UseItem(int tab, int slot) =>
        SendPacket(new ClientPacket.UseItem { Type = (sbyte)tab, Slot = (sbyte)slot });

    public void Disconnected() =>
        SendPacket(new ClientPacket.Disconnected());

    public void EnableSpace(bool isEnabled) =>
        SendPacket(new HClientPacket.EnableSpace { IsEnabled = isEnabled });

    public void TakeOffEquipment(int equipSlot, int inventorySlot) =>
        SendPacket(new ClientPacket.TakeOffEquipment { SubType = (sbyte)equipSlot, InventorySlot = (short)inventorySlot });

    public void WarpMap(int portal) => 
        SendPacket(new HClientPacket.WarpMap { Portal = portal });

    ~PacketManager()
    {
        Debug.Log("~PacketManager()");
    }
}
