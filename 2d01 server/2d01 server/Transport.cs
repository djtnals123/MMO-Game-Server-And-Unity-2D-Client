using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Timers;
using MySql.Data.MySqlClient;
using ZeroFormatter;

namespace _2d01_server
{

    class Transport
    {
        private static UdpClient srv = new UdpClient(7777); // 포트 7777
        private static IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0); // UDP 클라이언트 IP

        private static MySqlConnection connection = new MySqlConnection("Server=localhost;Port=3306;Database=2d01;Uid=root;Pwd=root");


        public static void StartServer()
        {
            connection.Open();
            setTimer();

            while (true)
            {
                try
                {
                    byte[] dgram = ReceivePacket();
                    if(dgram != null)
                        PacketHandler(Deserialize(dgram));
                }
                catch (SocketException e)
                {
                    Console.WriteLine("Socket Error. {0} {1}", e.ErrorCode, e.Message);
                }
            }
        }

        private static byte[] ReceivePacket()
        {
            if (srv.Available != 0)
            {
                byte[] dgram = srv.Receive(ref remoteEP);
                Console.WriteLine("[Receive] {0} 로부터 {1} 바이트 수신 {2}", remoteEP.ToString(), dgram.Length, dgram[0]);

                return dgram;
            }
            else return null;
        }




        private static ClientPacket.Packet Deserialize(byte[] dgram)
        {
            return ZeroFormatterSerializer.Deserialize<ClientPacket.Packet>(dgram);
        }


        private static void PacketHandler(ClientPacket.Packet deserialize)
        {
            byte[] dgram = null;
            string sql;
            MySqlDataReader table = null;
            List<string> excepts = null;
            ServerPacket.Packet serverPacket = null;
            string playerName = ClientInfo.FindPlayerName(remoteEP);


            switch (deserialize.packetType)
            {

                case ClientPacket.PacketType.ObjectSynchronization:
                    ClientPacket.ObjectSynchronization objSynPacket = (ClientPacket.ObjectSynchronization)deserialize;
                    serverPacket = new ServerPacket.ObjectSynchronization()
                    {
                        name = objSynPacket.name,
                        positionX = objSynPacket.positionX,
                        positionY = objSynPacket.positionY,
                        velocityX = objSynPacket.velocityX,
                        velocityY = objSynPacket.velocityY,
                        rotation = objSynPacket.rotation,
                        angularVelocity = objSynPacket.angularVelocity,
                        flipX = objSynPacket.flipX
                    };
                    excepts = new List<string>();
                    excepts.Add(objSynPacket.name);
                    dgram = ZeroFormatterSerializer.Serialize<ServerPacket.Packet>(serverPacket);
                    Multicast(dgram, excepts);
                    break;
                case ClientPacket.PacketType.Login:
                    ClientPacket.Login loginPacket = (ClientPacket.Login)deserialize;
                    sql = "SELECT * FROM account WHERE id = '" + loginPacket.id + "' AND password = '" + loginPacket.pass + "'";
                    table = new MySqlCommand(sql, connection).ExecuteReader();
                    if (table.HasRows)
                    {
                        ClientInfo.ClientList.Add(new ClientInfo(remoteEP, loginPacket.id));
                        dgram = ZeroFormatterSerializer.Serialize<ServerPacket.Packet>(new ServerPacket.LoginSuccess());
                        Unicast(dgram);

                        excepts = new List<string>();
                        excepts.Add(loginPacket.id);
                        dgram = ZeroFormatterSerializer.Serialize<ServerPacket.Packet>(new ServerPacket.InitPlayer { player = loginPacket.id });
                        Multicast(dgram, excepts);
                    }
                    else serverPacket = new ServerPacket.LoginFailure();
                    break;
                case ClientPacket.PacketType.RequestPlayerList:
                    List<string> otherPlayers = new List<string>();
                    foreach (ClientInfo c in ClientInfo.ClientList)
                        if (c.Player != ((ClientPacket.RequestPlayerList)deserialize).id)
                            otherPlayers.Add(c.Player);

                    dgram = ZeroFormatterSerializer.Serialize<ServerPacket.Packet>(new ServerPacket.InitPlayers { players = otherPlayers });
                    Unicast(dgram);

                    break;
                case ClientPacket.PacketType.AttackPlayer:
                    ClientPacket.AttackPlayer atkPacket = (ClientPacket.AttackPlayer)deserialize;
                    serverPacket = new ServerPacket.AttackPlayer() { id = atkPacket.id };

                    excepts = new List<string>();
                    excepts.Add(atkPacket.id);
                    dgram = ZeroFormatterSerializer.Serialize<ServerPacket.Packet>(serverPacket);
                    Multicast(dgram, excepts);

                    break;


                case ClientPacket.PacketType.HpSynchronization:
                    ClientPacket.HpSynchronization C_hpSyn = (ClientPacket.HpSynchronization)deserialize;
                    sql = "UPDATE account SET hp = hp " + C_hpSyn.hp + " WHERE " + "id = '" + playerName + "'";
                    table = new MySqlCommand(sql, connection).ExecuteReader();

                    if (table.RecordsAffected > 0)
                    {
                        table.Close();
                        sql = "SELECT maxhp, hp FROM account WHERE id = '" + playerName + "'";
                        table = new MySqlCommand(sql, connection).ExecuteReader();
                        if(table.Read())
                        {
                            ServerPacket.HpSynchronization S_hpSyn = new ServerPacket.HpSynchronization
                            {
                                id = playerName,
                                hp = table.GetInt16("hp"),
                                maxHp = table.GetInt16("maxhp")
                            };
                            dgram = ZeroFormatterSerializer.Serialize<ServerPacket.Packet>(S_hpSyn);
                            Broadcast(dgram);
                        }
                    }
                    else ;
                    break;
                
                case ClientPacket.PacketType.GetInventories:
                    ClientPacket.GetInventories getInventoriesPacket = (ClientPacket.GetInventories)deserialize;
                    sql = "SELECT max_inventory, account FROM account WHERE id = '" + getInventoriesPacket.id + "'";
                    table = new MySqlCommand(sql, connection).ExecuteReader();
                    table.Read();
                    if (table.HasRows)
                    {
                        int max_inventory = table.GetInt16("max_inventory");
                        string accout = table["account"].ToString();
                        table.Close();
                        sql = "SELECT slot, item, count FROM inventories WHERE account = '" + accout + "'";
                        table = new MySqlCommand(sql, connection).ExecuteReader();

                        List<int> slot = new List<int>();
                        List<int> item = new List<int>();
                        List<int> count = new List<int>();

                        while (table.Read())
                        {
                            slot.Add(table.GetInt32("slot"));
                            item.Add(table.GetInt32("item"));
                            count.Add(table.GetInt32("count"));
                        }

                        ServerPacket.SetInventories setInventoriesPacket = new ServerPacket.SetInventories
                        {
                            maxInventorySlot = max_inventory,
                            slot = slot,
                            item = item,
                            count = count
                        };
                        dgram = ZeroFormatterSerializer.Serialize<ServerPacket.Packet>(setInventoriesPacket);
                        Unicast(dgram);
                    }
                    break;
                case ClientPacket.PacketType.ConnectionAck:
                    ClientInfo client = ClientInfo.Find(remoteEP);
                    if (client != null)
                        client.ConnectionState = 5;
                    break;
                case ClientPacket.PacketType.Disconnected:
                    ClientInfo disconnected = ClientInfo.Find(remoteEP);
                    if(disconnected != null)
                    {
                        dgram = ZeroFormatterSerializer.Serialize<ServerPacket.Packet>(new ServerPacket.DisconnectedPlayer { id = disconnected.Player });
                        ClientInfo.ClientList.Remove(disconnected);
                        Broadcast(dgram);
                    }
                    break;
                default:
                    break;
            }
            if (table != null)
                table.Close();
        }
        private static void Unicast(byte[] dgram)
        {
            srv.Send(dgram, dgram.Length, remoteEP); 
            Console.WriteLine("[Send] {0} 로 {1} 바이트 송신", remoteEP.ToString(), dgram.Length);
        }

        private static void Multicast(byte[] dgram, List<string> excepts)
        {

            foreach (ClientInfo client in ClientInfo.ClientList)
            {
                bool isExcept = false;
                foreach (string except in excepts)
                {
                    if (client.Player == except)
                    {
                        isExcept = true;
                        break;
                    }
                }
                if (!isExcept)
                {
                    srv.Send(dgram, dgram.Length, client.RemoteEP); // 데이터 송신
                    Console.WriteLine("[Send] {0} 로 {1} 바이트 송신 ", client.RemoteEP.ToString(), dgram.Length);
                }

            }
        }

        private static void Broadcast(byte[] dgram)
        {
            foreach (ClientInfo client in ClientInfo.ClientList)
            {
                    srv.Send(dgram, dgram.Length, client.RemoteEP); // 데이터 송신
                Console.WriteLine("[Send] {0} 로 {1} 바이트 송신 ", client.RemoteEP.ToString(), dgram.Length);
            }
        }
        static private void setTimer()
        {
            Timer timer = new Timer();
            timer.Interval = 1 * 1000;
            timer.Elapsed += new ElapsedEventHandler(ConnectionCheck);
            timer.Start();
        }

        private static void ConnectionCheck(object sender, ElapsedEventArgs e)
        {
            ServerPacket.ConnectionCheck connectionCheck = new ServerPacket.ConnectionCheck();
            byte[] dgram = ZeroFormatterSerializer.Serialize<ServerPacket.Packet>(connectionCheck);
            List<ClientInfo> clientList = ClientInfo.ClientList;

            for (int i = 0; i < clientList.Count; i++)
            {
                if (clientList[i].ConnectionState-- > 0)
                {
                    srv.Send(dgram, dgram.Length, clientList[i].RemoteEP); // 데이터 송신
                    Console.WriteLine("[Send] {0} 로 {1} 바이트 송신 ", clientList[i].RemoteEP.ToString(), dgram.Length);
                }
                else
                {
                    byte[] disc = ZeroFormatterSerializer.Serialize<ServerPacket.Packet>(new ServerPacket.DisconnectedPlayer { id = clientList[i].Player });
                    Broadcast(disc);
                    clientList.Remove(clientList[i]);
                    i--;
                }    
            }
        }

        public static void Close()
        {
            srv.Close();
            connection.Close();
        }
    }
}