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

    private static UdpClient srv = new UdpClient(7778); // 포트 7777
    private ConcurrentQueue<UdpReceiveResult> dgramQueue = new ConcurrentQueue<UdpReceiveResult>();
    GameObject p = null;

    private static readonly IPEndPoint sRemoteEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7777);

    void Start()
    {
        Task.Run(ReceivePacket);

        //     SceneManager.UnloadSceneAsync("Map_1");

    }

    void Update()
    {
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

                Debug.Log(deserialize.packetType.ToString("g"));
                switch (deserialize.packetType)
                {
                    case ServerPacket.PacketType.HInitPlayer:
                        ServerPacket.HInitPlayer initPlayer = (ServerPacket.HInitPlayer)deserialize;
                        Vector2 spawnPoint = MapData.GetRandomSpawnPoint(initPlayer.Map);

                        InitObject.InitPlayer(initPlayer.Map, initPlayer.Player, initPlayer.Equips, spawnPoint, initPlayer.RemoteEP);
                        player = InitObject.UserListByName[initPlayer.Player];

                        hServerPacket = new HServerPacket.LoginSuccess { Map = initPlayer.Map, PositionX = spawnPoint.x, PositionY = spawnPoint.y, Equips = initPlayer.Equips };
                        dgram = ZeroFormatterSerializer.Serialize<HServerPacket.Packet>(hServerPacket);
                        Send(dgram, player.RemoteEP);

                        InitPlayers(player);
                        InitPlayer(player);

                        break;
                    case ServerPacket.PacketType.HpSynchronization:
                        ServerPacket.HpSynchronization hpSyn = (ServerPacket.HpSynchronization)deserialize;
                        player = GameObject.Find("Player " + hpSyn.Id).GetComponent<PlayerScript>(); //
                        if (player != null)
                        {
                            PlayerScript playerComponent = player.GetComponent<PlayerScript>();
                            playerComponent.HealthImage.fillAmount = (float)hpSyn.Hp / hpSyn.MaxHp;
                            if (playerComponent.HealthImage.fillAmount <= 0)
                                Destroy(playerComponent);
                        }
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
                player = InitObject.UserList[result.RemoteEndPoint.ToString()];
                switch (deserialize.packetType)
                {
                    case HClientPacket.PacketType.ObjectSynchronization:
                        {
                            HClientPacket.ObjectSynchronization objSynPacket = (HClientPacket.ObjectSynchronization)deserialize;
                            Debug.Log(result.RemoteEndPoint.ToString() + "aa");
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
                                    dgram = ZeroFormatterSerializer.Serialize<HServerPacket.Packet>(hServerPacket);
                                    SendThisMap(dgram, result.RemoteEndPoint, false);
                                }
                            }

                            break;
                        }

                    case HClientPacket.PacketType.AttackPlayer:
                        {
                            hServerPacket = new HServerPacket.AttackPlayer() { Id = player.nicname };
                            dgram = ZeroFormatterSerializer.Serialize<HServerPacket.Packet>(hServerPacket);
                            SendThisMap(dgram, result.RemoteEndPoint, false);
                            player.KeyDownSpace = true;
                            break;
                        }
                    case HClientPacket.PacketType.EnableSpace:
                        {

                            HClientPacket.EnableSpace enableSpace = (HClientPacket.EnableSpace)deserialize;
                            hServerPacket = new HServerPacket.EnableSpace { Player = player.nicname, IsEnabled = enableSpace.IsEnabled };
                            dgram = ZeroFormatterSerializer.Serialize<HServerPacket.Packet>(hServerPacket);
                            SendThisMap(dgram, result.RemoteEndPoint, false);
                            player.EnableSpace = enableSpace.IsEnabled;

                            break;
                        }
                    case HClientPacket.PacketType.WarpMap:
                        {
                            HClientPacket.WarpMap warpMap = (HClientPacket.WarpMap)deserialize;
                            InitObject.WarpMap(player, warpMap.Portal);

                            hServerPacket = new HServerPacket.WarpMap { LinkedMap = player.Map, PosX = player.CurPos.x, PosY = player.CurPos.y };
                            dgram = ZeroFormatterSerializer.Serialize<HServerPacket.Packet>(hServerPacket);
                            Send(dgram, player.RemoteEP);

                            InitPlayers(player);
                            InitPlayer(player);

                            ClientPacket.Packet clientPacket = new ClientPacket.HWarpMap { Map = player.Map, RemoteEP = player.RemoteEP.ToString() }; 
                            dgram = ZeroFormatterSerializer.Serialize<ClientPacket.Packet>(clientPacket);
                            Send(dgram, sRemoteEP);

                            // 이닛플레이어스 추가, 맵체크 추가
                            break;
                        }
                }
            }
        }
    }

    private static void InitPlayer(PlayerScript player)
    {
        HServerPacket.InitPlayer hServerPacket = new HServerPacket.InitPlayer
        {
            Player = player.nicname,
            Equips = player.Equips,
            PositionX = player.CurPos.x,
            PositionY = player.CurPos.y,
            FlipX = player.SR.flipX,
        };
        foreach (var user in InitObject.Map[InitObject.UserList[player.RemoteEP.ToString()].Map])
        {
            if (user != player)
            {
                hServerPacket.MapCheck = user.CheckMap;
                byte[] dgram = ZeroFormatterSerializer.Serialize<HServerPacket.Packet>(hServerPacket);
                srv.Send(dgram, dgram.Length, user.RemoteEP); // 데이터 송신
            }
        }
    }

    private static void InitPlayers(PlayerScript player)
    {
        List<string> otherPlayers = new List<string>();
        List<List<int>> equips = new List<List<int>>();
        List<float> positionX = new List<float>();
        List<float> positionY = new List<float>();
        List<float> velocityX = new List<float>();
        List<float> velocityY = new List<float>();
        List<bool> flipX = new List<bool>();

        foreach (var p in InitObject.Map[player.Map])
        {
            if (p != player)
            {
                otherPlayers.Add(p.nicname);
                equips.Add(p.Equips);
                positionX.Add(p.CurPos.x);
                positionY.Add(p.CurPos.y);
                velocityX.Add(p.RB.velocity.x);
                velocityY.Add(p.RB.velocity.y);
                flipX.Add(p.SR.flipX);
            }
        }
        HServerPacket.Packet hServerPacket = new HServerPacket.InitPlayers
        {
            Players = otherPlayers,
            Equips = equips,
            PositionX = positionX,
            PositionY = positionY,
            VelocityX = velocityX,
            VelocityY = velocityY,
            FlipX = flipX
        };
        byte[] dgram = ZeroFormatterSerializer.Serialize<HServerPacket.Packet>(hServerPacket);
        Send(dgram, player.RemoteEP);
    }


    private static void Send(byte[] dgram, IPEndPoint remoteEP)
    {
        srv.Send(dgram, dgram.Length, remoteEP);
    }

    private static void SendThisMap(byte[] dgram, IPEndPoint remoteEP, bool willSendSelf)
    {
        LinkedList<PlayerScript> mapUserList = InitObject.Map[InitObject.UserList[remoteEP.ToString()].Map];

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
}
