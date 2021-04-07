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
        if (Client.Available != 0)
        {
            IPEndPoint epRemote = new IPEndPoint(IPAddress.Any, 0); // 데이터 수신
            byte[] responsePacket = Client.Receive(ref epRemote);
            ServerPacket.Packet deserialize = ZeroFormatterSerializer.Deserialize<ServerPacket.Packet>(responsePacket);

            switch (deserialize.packetType)
            {
                case ServerPacket.PacketType.ObjectSynchronization:
                    ServerPacket.ObjectSynchronization objInfo = (ServerPacket.ObjectSynchronization)deserialize;
          //          Debug.Log(" : " + objInfo.PositionX + " " + objInfo.PositionY + "Player " + objInfo.Name);
                    GameObject playerObject = GameObject.Find("Player " + objInfo.Name);
                    if (playerObject != null)
                        playerObject.GetComponent<PlayerScript>().SyncPlayer(objInfo.Name, objInfo.PositionX, objInfo.PositionY,
                            objInfo.VelocityX, objInfo.VelocityY, objInfo.Rotation, objInfo.AngularVelocity, objInfo.FlipX);
                    break;
                case ServerPacket.PacketType.AttackPlayer:
                    ServerPacket.AttackPlayer atkPlayer = (ServerPacket.AttackPlayer)deserialize;
                    PlayerScript player = GameObject.Find("Player " + atkPlayer.Id).GetComponent<PlayerScript>();
                    player.KeyDownSpace = true;
                    break;
                case ServerPacket.PacketType.LoginSuccess:
                    UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene");
                    break;
                case ServerPacket.PacketType.InitPlayer:
                    ServerPacket.InitPlayer initPlayer = (ServerPacket.InitPlayer)deserialize;
                    InitObject.InitPlayer(initPlayer.Player, initPlayer.Equips);
                    break;
                case ServerPacket.PacketType.InitPlayers:
                    ServerPacket.InitPlayers initPlayers = (ServerPacket.InitPlayers)deserialize;
                    InitObject.InitPlayers(initPlayers.Players, initPlayers.Equips);
                    break;
                case ServerPacket.PacketType.PlayerSetting:
                    ServerPacket.PlayerSetting playersetting = (ServerPacket.PlayerSetting)deserialize;
                    Inventory.SetInventories(playersetting.Item, playersetting.Type, playersetting.Count, playersetting.Slot, playersetting.MaxEquipmentSlot, playersetting.MaxUseableSlot, playersetting.MaxEtcSlot, playersetting.MaxEnhancementSlot);
                    Equipment.SetEquipment(playersetting.Equip);
                    player = GameObject.Find("Player(Clone)").GetComponent<PlayerScript>();
                    player.PutOnEquipment(playersetting.Equip);

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
                case ServerPacket.PacketType.EnableSpace:
                    ServerPacket.EnableSpace enableSpace = (ServerPacket.EnableSpace)deserialize;
                    player = GameObject.Find("Player " + enableSpace.Player).GetComponent<PlayerScript>();
                    player.EnableSpace = enableSpace.Enable;
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
    }

    public static void ConnectionAck() =>
        SendPacket(new ClientPacket.ConnectionAck());

    public static void Login(string id, string pass) =>
        SendPacket(new ClientPacket.Login { Id = id, Pass = pass });

    public static void PlayerSetting(string id) =>
        SendPacket(new ClientPacket.PlayerSetting { Id = id });
    public static void AttackPlayer(string id) =>
        SendPacket(new ClientPacket.AttackPlayer { Id = id });

    public static void RequestPlayerList() =>
        SendPacket(new ClientPacket.RequestPlayerList() { Id = InitObject.playerNicname });

    public static void HpSynchronization(int hp) =>
        SendPacket(new ClientPacket.HpSynchronization { Hp = (short)hp });

    public static void ObjectSynchronization(string name, float posX, float posY, float velX, float velY, float rotation, float angularVel, bool flipX) =>
        SendPacket(new ClientPacket.ObjectSynchronization
        {
            Name = name,
            PositionX = posX,
            PositionY = posY,
            VelocityX = velX,
            VelocityY = velY,
            Rotation = rotation,
            AngularVelocity = angularVel,
            FlipX = flipX
        });


    public static void ChangeItemSlot(sbyte tab, short slot1, short slot2) =>
        SendPacket(new ClientPacket.ChangeItemSlot
        {
            Type = tab,
            Slot1 = slot1,
            Slot2 = slot2
        });

    public static void UseItem(int tab, int slot) =>
        SendPacket(new ClientPacket.UseItem { Type = (sbyte)tab, Slot = (sbyte)slot });

    public static void Disconnected() =>
        SendPacket(new ClientPacket.Disconnected());

    public static void EnableSpace(bool enable) =>
        SendPacket(new ClientPacket.EnableSpace { Enable = enable });

    public static void TakeOffEquipment(int equipSlot, int inventorySlot) =>
        SendPacket(new ClientPacket.TakeOffEquipment { SubType = (sbyte)equipSlot, InventorySlot = (short)inventorySlot });

    ~PacketManager()
    {
        Debug.Log("~PacketManager()");
    }
}
