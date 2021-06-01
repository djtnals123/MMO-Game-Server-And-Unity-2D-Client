using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using ZeroFormatter;

public class PacketManager : MonoBehaviour
{

    private UdpClient srv; // 포트 7777
    private ConcurrentQueue<UdpReceiveResult> dgramQueue = new ConcurrentQueue<UdpReceiveResult>();

    private readonly IPEndPoint sRemoteEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7777);

    public static PacketManager instance;

    void Start()
    {
        instance = this;
           srv = new UdpClient(7778);
        Task.Run(ReceivePacket);

    }

    void Update()
    {
        //      GameObject.Find("New Sprite").SetActive(false);
        
        PacketHandler();
        for (int i = 2; i < SceneManager.sceneCount; i++)
            SceneManager.GetSceneAt(i).GetPhysicsScene2D().Simulate(Time.deltaTime);


    }
    private void ReceivePacket()
    {
        //     p.transform.position = new Vector2(11f, 11f);
        while (true)
            dgramQueue.Enqueue(srv.ReceiveAsync().Result);

    }

    public void PacketHandler()
    {
        UdpReceiveResult result;
        HServerPacket.Packet hServerPacket;
        byte[] dgram;
        PlayerScript player;
        while (dgramQueue.TryDequeue(out result))
        {
            if (result.RemoteEndPoint.ToString() == "127.0.0.1:7777")
            {
                ServerPacket.Packet deserialize = ZeroFormatterSerializer.Deserialize<ServerPacket.Packet>(result.Buffer);
                Debug.Log(deserialize.packetType.ToString("g") + " " + result.Buffer.Length);
                switch (deserialize.packetType)
                {
                    case ServerPacket.PacketType.HInitPlayer:
                        ServerPacket.HInitPlayer initPlayer = (ServerPacket.HInitPlayer)deserialize;
                        Vector2 spawnPoint = MapData.GetRandomSpawnPoint(initPlayer.Map);

                        InitObject.InitPlayer(initPlayer.Map, initPlayer.Player,initPlayer.MaxHP, initPlayer.HP, initPlayer.Equips, spawnPoint, initPlayer.RemoteEP);
                        player = InitObject.UserListByName[initPlayer.Player];

                        hServerPacket = new HServerPacket.LoginSuccess { Map = initPlayer.Map, PositionX = spawnPoint.x, PositionY = spawnPoint.y,
                            Equips = initPlayer.Equips, MaxHP = initPlayer.MaxHP, HP = initPlayer.HP };
                        dgram = ZeroFormatterSerializer.Serialize(hServerPacket);
                        Send(dgram, player.RemoteEP);
                  //      InitObjects(player);
                        InitPlayer(player);

                        break;
                    case ServerPacket.PacketType.DisconnectedPlayer:
                        ServerPacket.DisconnectedPlayer removePlayer = (ServerPacket.DisconnectedPlayer)deserialize;
                        Destroy(InitObject.UserListByName[removePlayer.Id].gameObject);
                        break;
                    case ServerPacket.PacketType.PutOnPlayer:
                        ServerPacket.PutOnPlayer putOnPlayer = (ServerPacket.PutOnPlayer)deserialize;
                        InitObject.UserListByName[putOnPlayer.Player].PutOnEquipment(putOnPlayer.Item);
                        break;
                    case ServerPacket.PacketType.TakeOffPlayer:
                        ServerPacket.TakeOffPlayer takeOffPlayer = (ServerPacket.TakeOffPlayer)deserialize;
                        InitObject.UserListByName[takeOffPlayer.Player].TakeOffEquipment(takeOffPlayer.SubType);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                HClientPacket.Packet deserialize = ZeroFormatterSerializer.Deserialize<HClientPacket.Packet>(result.Buffer);
         //       Debug.Log(deserialize.packetType.ToString("g") + " " + result.Buffer.Length);
                player = InitObject.UserList[result.RemoteEndPoint.ToString()];
                switch (deserialize.packetType)
                {
                    case HClientPacket.PacketType.ObjectSynchronization:
                        {
                            HClientPacket.ObjectSynchronization objSynPacket = (HClientPacket.ObjectSynchronization)deserialize;
                            if (InitObject.UserList.ContainsKey(result.RemoteEndPoint.ToString()))
                            {
                                if (player.CheckMap == objSynPacket.MapCheck)
                                {
                                    player.SyncPlayer(objSynPacket.PositionX, objSynPacket.PositionY, objSynPacket.VelocityX, objSynPacket.VelocityY,
                                        objSynPacket.Rotation, objSynPacket.AngularVelocity, objSynPacket.FlipX);

                                    hServerPacket = new HServerPacket.ObjectSynchronization()
                                    {
                                        Name = InitObject.UserList[result.RemoteEndPoint.ToString()].nicname,
                                        PositionX = objSynPacket.PositionX,
                                        PositionY = objSynPacket.PositionY,
                                        VelocityX = objSynPacket.VelocityX,
                                        VelocityY = objSynPacket.VelocityY,
                                        Rotation = objSynPacket.Rotation,
                                        AngularVelocity = objSynPacket.AngularVelocity,
                                        FlipX = objSynPacket.FlipX
                                    };
                                    dgram = ZeroFormatterSerializer.Serialize(hServerPacket);
                                    SendThisMap(dgram, result.RemoteEndPoint, false);
                                }
                            }

                            break;
                        }

                    case HClientPacket.PacketType.AttackPlayer:
                        {
                            hServerPacket = new HServerPacket.AttackPlayer() { Id = player.nicname };
                            dgram = ZeroFormatterSerializer.Serialize(hServerPacket);
                            SendThisMap(dgram, result.RemoteEndPoint, false);
                            player.KeyDownSpace = true;
                            break;
                        }
                    case HClientPacket.PacketType.EnableSpace:
                        {

                            HClientPacket.EnableSpace enableSpace = (HClientPacket.EnableSpace)deserialize;
                            hServerPacket = new HServerPacket.EnableSpace { Player = player.nicname, IsEnabled = enableSpace.IsEnabled };
                            dgram = ZeroFormatterSerializer.Serialize(hServerPacket);
                            SendThisMap(dgram, result.RemoteEndPoint, false);
                            player.EnableSpace = enableSpace.IsEnabled;

                            break;
                        }
                    case HClientPacket.PacketType.WarpMap:
                        {
                            HClientPacket.WarpMap warpMap = (HClientPacket.WarpMap)deserialize;
                            InitObject.WarpMap(player, warpMap.Portal);

                            Debug.Log("asdfsdfd");
                            hServerPacket = new HServerPacket.WarpMap { LinkedMap = player.Map, PosX = player.CurPos.x, PosY = player.CurPos.y };
                            dgram = ZeroFormatterSerializer.Serialize(hServerPacket);
                            Send(dgram, player.RemoteEP);

                          //  InitObjects(player);
                            InitPlayer(player);

                            ClientPacket.Packet clientPacket = new ClientPacket.HWarpMap { Map = player.Map, RemoteEP = player.RemoteEP.ToString() };
                            dgram = ZeroFormatterSerializer.Serialize(clientPacket);
                            Send(dgram, sRemoteEP);

                            break;
                        }
                    case HClientPacket.PacketType.RequestObjects:
                        {
                            InitObjects(player);
                            break;
                        }
                }
            }
        }
    }

    private void InitPlayer(PlayerScript player)
    {
        HServerPacket.InitPlayer hServerPacket = new HServerPacket.InitPlayer
        {
            Player = player.nicname,
            Equips = player.Equips,
            PositionX = player.CurPos.x,
            PositionY = player.CurPos.y,
            FlipX = player.SR.flipX,
            MaxHP = (short)player.MaxHP,
            HP = (short)player.HP
        };
        hServerPacket.MapCheck = true;
        byte[] dgramMapCheckTrue = ZeroFormatterSerializer.Serialize<HServerPacket.Packet>(hServerPacket);
        hServerPacket.MapCheck = false;
        byte[] dgramMapCheckFalse = ZeroFormatterSerializer.Serialize<HServerPacket.Packet>(hServerPacket);

        SendThisMap(dgramMapCheckTrue, dgramMapCheckFalse, player.RemoteEP, false);
    }

    public void PlayerHpSynchronization(string player, int variationHP)
    {
        ClientPacket.Packet clientPacket = new ClientPacket.HPlayerHpSynchronization { Player = player, VariationHP = (short)variationHP };
        byte[] dgram = ZeroFormatterSerializer.Serialize<ClientPacket.Packet>(clientPacket);
        Send(dgram, sRemoteEP);

        HServerPacket.Packet hServerPacket = new HServerPacket.PlayerHpSynchronization { Player = player, VariationHP = (short)variationHP };
        dgram = ZeroFormatterSerializer.Serialize<HServerPacket.Packet>(hServerPacket);
        SendThisMap(dgram, InitObject.UserListByName[player].RemoteEP, true);
    }
    public void MobHpSynchronization(int mobID, int variationHP, int map)
    {
        HServerPacket.MobHpSynchronization hServerPacket = new HServerPacket.MobHpSynchronization { MobID = (sbyte)mobID, VariationHP = (short)variationHP, MapCheck = true };
        byte[] dgramMapCheckTrue = ZeroFormatterSerializer.Serialize<HServerPacket.Packet>(hServerPacket);
        hServerPacket.MapCheck = false;
        byte[] dgramMapCheckFalse = ZeroFormatterSerializer.Serialize<HServerPacket.Packet>(hServerPacket);

        SendThisMap(dgramMapCheckTrue, dgramMapCheckFalse, map);
    }

    private void InitObjects(PlayerScript player)
    {
        List<string> otherPlayers = new List<string>();
        List<List<int>> equips = new List<List<int>>();
        List<float> positionX = new List<float>();
        List<float> positionY = new List<float>();
        List<float> velocityX = new List<float>();
        List<float> velocityY = new List<float>();
        List<bool> flipX = new List<bool>();
        List<int> maxHP = new List<int>();
        List<int> hp = new List<int>();

        foreach (var p in MapManager.Instance.Map[player.Map].Players)
        {
            if (p != player)
            {
                otherPlayers.Add(p.nicname);
                equips.Add(p.Equips);
                positionX.Add(p.CurPos.x);
                positionY.Add(p.CurPos.y);
                velocityX.Add(p.Rigid.velocity.x);
                velocityY.Add(p.Rigid.velocity.y);
                flipX.Add(p.SR.flipX);
                maxHP.Add(p.MaxHP);
                hp.Add(p.HP);
            }
        }
        //      LinkedList<int> i = new LinkedList<int>(); i.AddLast(1);
        LinkedList<RemoteObject> LiveMobs = new LinkedList<RemoteObject>(MapManager.Instance.Map[player.Map].Mobs);
        List<RemoteObject> ListMobs = new List<RemoteObject>(MapManager.Instance.Map[player.Map].Mobs);
        LinkedList<int> deadMobs = new LinkedList<int>();
        foreach (var mob in MapManager.Instance.Map[player.Map].DeadMobs)
            LiveMobs.Remove(ListMobs[mob.ID - 1]);
        foreach (var mob in MapManager.Instance.Map[player.Map].DeadMobs)
            deadMobs.AddLast(mob.ID);


        LinkedList<sbyte> nextMove = new LinkedList<sbyte>();
        LinkedList<float> posX = new LinkedList<float>();
        LinkedList<float> posY = new LinkedList<float>();
        LinkedList<short> mobHP = new LinkedList<short>();
        foreach (var mobObj in LiveMobs)
        {
            var mob = mobObj.GetComponent<MobAI_1>();
            nextMove.AddLast((sbyte)mob.NextMove);
            posX.AddLast(mob.transform.position.x);
            posY.AddLast(mob.transform.position.y);
            mobHP.AddLast((short)mob.HP);
            Debug.Log(mob.transform.position.x + " " + mob.transform.position.y);
        }

        HServerPacket.Packet hServerPacket = new HServerPacket.InitObjects
        {
            Players = otherPlayers,
            Equips = equips,
            PositionX = positionX,
            PositionY = positionY,
            VelocityX = velocityX,
            VelocityY = velocityY,
            FlipX = flipX,
            MaxHP = maxHP,
            HP = hp,
            DeadMobs = deadMobs,
            NextMoves = nextMove,
            MobPosX = posX,
            MobPosY = posY,
            MobHP = mobHP
        };
        byte[] dgram = ZeroFormatterSerializer.Serialize(hServerPacket);
        Send(dgram, player.RemoteEP);
    }

    public void MoveMob(int map, int mobID, int nextMove, float posX, float posY, float velY)
    {
        HServerPacket.Packet hServerPacket = new HServerPacket.MoveMob { MobID = mobID, NextMove = (sbyte)nextMove, PosX = posX, PosY = posY, VelY = velY };
        byte[] dgram = ZeroFormatterSerializer.Serialize(hServerPacket);
        SendThisMap(dgram, map);
    }
    public void ReSpawnMobs(int map)
    {
        HServerPacket.Packet hServerPacket = new HServerPacket.ReSpawnMobs();
        byte[] dgram = ZeroFormatterSerializer.Serialize(hServerPacket);
        SendThisMap(dgram, map);
    }


    private void Send(byte[] dgram, IPEndPoint remoteEP)
    {
        srv.Send(dgram, dgram.Length, remoteEP);
    }

    private void SendThisMap(byte[] dgram, IPEndPoint remoteEP, bool willSendSelf)
    {
        LinkedList<PlayerScript> mapUserList = MapManager.Instance.Map[InitObject.UserList[remoteEP.ToString()].Map].Players;

        if (willSendSelf)
        {
            foreach (var user in mapUserList)
            {
                srv.Send(dgram, dgram.Length, user.RemoteEP); // 데이터 송신
            }
        }
        else
        {
            foreach (var user in mapUserList)
            {
                if (user.RemoteEP.ToString() != remoteEP.ToString())
                {
                    srv.Send(dgram, dgram.Length, user.RemoteEP); // 데이터 송신
                }
            }
        }
    }
    private void SendThisMap(byte[] dgram, int map)
    {
        foreach (var user in MapManager.Instance.Map[map].Players)
            Send(dgram, user.RemoteEP);
    }
    private void SendThisMap(byte[] dgramMapCheckTrue, byte[] dgramMapCheckFalse, int map)
    {
        foreach (var user in MapManager.Instance.Map[map].Players)
        {
            if (user.CheckMap)
                Send(dgramMapCheckTrue, user.RemoteEP);
            else
                Send(dgramMapCheckFalse, user.RemoteEP);
        }
    }
    private void SendThisMap(byte[] dgramMapCheckTrue, byte[] dgramMapCheckFalse, IPEndPoint remoteEP, bool willSendSelf)
    {
        if(willSendSelf)
        {
            foreach (var user in MapManager.Instance.Map[InitObject.UserList[remoteEP.ToString()].Map].Players)
            {
                if (user.CheckMap)
                    Send(dgramMapCheckTrue, user.RemoteEP);
                else
                    Send(dgramMapCheckFalse, user.RemoteEP);
            }
        }
        else
        {
            foreach (var user in MapManager.Instance.Map[InitObject.UserList[remoteEP.ToString()].Map].Players)
            {
                if (user.RemoteEP != remoteEP)
                {
                    if (user.CheckMap)
                        Send(dgramMapCheckTrue, user.RemoteEP);
                    else
                        Send(dgramMapCheckFalse, user.RemoteEP);
                }
            }
        }
    }
}
