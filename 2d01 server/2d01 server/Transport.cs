using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Timers;
using MySql.Data.MySqlClient;
using ZeroFormatter;

namespace _2d01_server
{
    class Transport
    {
        private static UdpClient srv = new UdpClient(7777); // 포트 7777

        private static MySqlConnection connection = new MySqlConnection("Server=localhost;Port=3306;Database=2d01;Uid=root;Pwd=root");
        private static MySqlCommand command = connection.CreateCommand();
        private static MySqlTransaction transaction;

        private static Dictionary<string, ClientInfo> userList = new Dictionary<string, ClientInfo>();
        private static Dictionary<string, ClientInfo> userListByName = new Dictionary<string, ClientInfo>();
        private static Dictionary<int, Map> map = new Dictionary<int, Map>();

        private static ServerPacket.Packet serverPacket = null;
        private static int receivePacketLength; //디버그용

        private static readonly IPEndPoint hRemoteEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7778);

        public static void StartServer()
        {
            SetMySql();
            SetTimer();

            while (true)
            {
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0); // UDP 클라이언트 IP
                Console.WriteLine();
                byte[] dgram = ReceivePacket(ref remoteEP);
                try
                {
                    Task.Run(() => PacketHandler(Deserialize(dgram), remoteEP));
                }
                catch (SocketException e)
                {
                    Console.WriteLine("Socket Error. {0} {1}", e.ErrorCode, e.Message);
                }
            }
        }

        private static byte[] ReceivePacket(ref IPEndPoint remoteEP)
        {
 //           if (srv.Available != 0)
 //           {
                byte[] dgram = srv.Receive(ref remoteEP);
                receivePacketLength = dgram.Length;

                return dgram;
//            }
//            else return null;
        }

        private static void SetMySql()
        {
            connection.Open();
            command.Connection = connection;
            command.Transaction = transaction;

        }


        private static ClientPacket.Packet Deserialize(byte[] dgram)
        {
            return ZeroFormatterSerializer.Deserialize<ClientPacket.Packet>(dgram);
        }


        private static void PacketHandler(ClientPacket.Packet deserialize, IPEndPoint remoteEP)
        {
            byte[] dgram = null;
            string sql;
            MySqlDataReader table = null;
            ClientInfo client = null;

            Console.WriteLine("[Receive] {0} 로부터 {1} 바이트 수신\tPacketType : {2}", remoteEP.ToString(), receivePacketLength, deserialize.packetType.ToString("g"));

            if (remoteEP.ToString() == "127.0.0.1:7778")
            {
                switch (deserialize.packetType)
                {
                    case ClientPacket.PacketType.HWarpMap:
                        {
                            ClientPacket.HWarpMap warpMap = (ClientPacket.HWarpMap)deserialize;

                            if (userList.ContainsKey(warpMap.RemoteEP))
                                client = userList[warpMap.RemoteEP];
                            DisconnectPlayer(client.RemoteEP, false);

                            if (!map.ContainsKey(warpMap.Map))
                                map.Add(warpMap.Map, new Map(warpMap.Map));
                            client.Map = map[warpMap.Map];
                            map[warpMap.Map].UserList.Add(client.RemoteEP.ToString(), client);


                            sql = "UPDATE account SET map = " + warpMap.Map + " WHERE account = '" + client.Account + "'";
                            table = new MySqlCommand(sql, connection).ExecuteReader();
                            table.Close();
                            break;
                        }
                    case ClientPacket.PacketType.HPlayerHpSynchronization:
                        {
                            ClientPacket.HPlayerHpSynchronization C_hpSyn = (ClientPacket.HPlayerHpSynchronization)deserialize;
                            string hp = C_hpSyn.VariationHP.ToString();
                            if (C_hpSyn.VariationHP >= 0)
                                hp = hp.Insert(0, "+");
                            sql = "UPDATE account SET hp = hp " + hp + " WHERE id = '" + C_hpSyn.Player + "'";
                            table = new MySqlCommand(sql, connection).ExecuteReader();
                            table.Close();
                            break;
                        }
                    default:
                        break;
                }
            }
            else
            {
                if (userList.ContainsKey(remoteEP.ToString()))
                    client = userList[remoteEP.ToString()];

                switch (deserialize.packetType)
                {
                    case ClientPacket.PacketType.Login:
                        {
                            ClientPacket.Login loginPacket = (ClientPacket.Login)deserialize;
                            sql = "SELECT * FROM account WHERE id = '" + loginPacket.Id + "' AND password = '" + loginPacket.Pass + "'";
                            table = new MySqlCommand(sql, connection).ExecuteReader();
                            if (table.Read())
                            {
                                int maxHP = table.GetInt16("max_hp");
                                int hp = table.GetInt16("hp");
                                int mapCode = table.GetInt32("map");
                                if (!map.ContainsKey(mapCode))
                                    map.Add(mapCode, new Map(mapCode));

                                var newClinet = new ClientInfo(remoteEP, loginPacket.Id, table.GetInt32("account"), map[mapCode]); //
                                table.Close();
                                userList.Add(remoteEP.ToString(), newClinet);
                                userListByName.Add(newClinet.Player, newClinet);
                                map[mapCode].UserList.Add(remoteEP.ToString(), newClinet);


                                sql = "SELECT item FROM wearing_equipment WHERE account = " + newClinet.Account;
                                table = new MySqlCommand(sql, connection).ExecuteReader();
                                List<int> equips = new List<int>();
                                while (table.Read())
                                {
                                    equips.Add(table.GetInt32("item"));
                                }
                                table.Close();
                                serverPacket = new ServerPacket.HInitPlayer
                                {
                                    Player = newClinet.Player,
                                    Equips = equips,
                                    Map = mapCode,
                                    MaxHP = (short)maxHP,
                                    HP = (short)hp,
                                    RemoteEP = remoteEP.ToString()
                                };
                                dgram = ZeroFormatterSerializer.Serialize<ServerPacket.Packet>(serverPacket);
                                ServerPacket.Packet d = ZeroFormatterSerializer.Deserialize<ServerPacket.Packet>(dgram);
                                Send(dgram, hRemoteEP);
                            }
                            else serverPacket = new ServerPacket.LoginFailure();
                            break;
                        }
                    case ClientPacket.PacketType.PlayerSetting:
                        {
                            ClientPacket.PlayerSetting getInventoriesPacket = (ClientPacket.PlayerSetting)deserialize;
                            sql = "SELECT max_equ, max_use, max_etc, max_enh FROM account WHERE account = " + client.Account;
                            table = new MySqlCommand(sql, connection).ExecuteReader();
                            if (table.Read())
                            {
                                short maxEquipmentSlot = table.GetInt16("max_equ");
                                short maxUseableSlot = table.GetInt16("max_use");
                                short maxEtcSlot = table.GetInt16("max_etc");
                                short maxEnhancementSlot = table.GetInt16("max_enh");
                                table.Close();
                                sql = "SELECT slot, type, item, count FROM inventories WHERE account = " + client.Account;
                                table = new MySqlCommand(sql, connection).ExecuteReader();

                                List<short> slot = new List<short>();
                                List<sbyte> type = new List<sbyte>();
                                List<int> item = new List<int>();
                                List<short> count = new List<short>();

                                while (table.Read())
                                {
                                    slot.Add(table.GetInt16("slot"));
                                    type.Add((sbyte)table.GetInt16("type"));
                                    item.Add(table.GetInt32("item"));
                                    count.Add(table.GetInt16("count"));
                                }
                                table.Close();
                                serverPacket = new ServerPacket.PlayerSetting
                                {
                                    MaxEquipmentSlot = maxEquipmentSlot,
                                    MaxUseableSlot = maxUseableSlot,
                                    MaxEtcSlot = maxEtcSlot,
                                    MaxEnhancementSlot = maxEnhancementSlot,
                                    Slot = slot,
                                    Type = type,
                                    Item = item,
                                    Count = count
                                };
                                dgram = ZeroFormatterSerializer.Serialize(serverPacket);
                                Send(dgram, remoteEP);
                            }
                            break;
                        }
                    case ClientPacket.PacketType.ConnectionAck:
                        {
                            if (client != null)
                                client.ConnectionState = 5;
                            else Console.WriteLine("null");
                            break;
                        }
                    case ClientPacket.PacketType.Disconnected:
                        {
                            DisconnectPlayer(remoteEP, true);
                            userListByName.Remove(userList[remoteEP.ToString()].Player);
                            userList.Remove(remoteEP.ToString());
                            break;
                        }
                    case ClientPacket.PacketType.ChangeItemSlot:
                        {
                            ClientPacket.ChangeItemSlot slotChange = (ClientPacket.ChangeItemSlot)deserialize;

                            sql = "SELECT slot FROM inventories WHERE account = '" + client.Account + "' And type = " + slotChange.Type + " And (slot = " + slotChange.Slot1 + " Or slot = " + slotChange.Slot2 + ") ORDER BY slot";
                            table = new MySqlCommand(sql, connection).ExecuteReader();

                            List<int> slots = new List<int>();
                            while (table.Read())
                            {
                                slots.Add(table.GetInt32("slot"));
                            }
                            table.Close();
                            slots.Sort();
                            bool success = false;
                            if (slots.Count == 1)
                            {
                                if (slotChange.Slot1 == slots[0])
                                {
                                    sql = "UPDATE inventories SET slot = " + slotChange.Slot2 + " WHERE account = '" + client.Account + "' AND slot = " + slotChange.Slot1 + " AND type = " + slotChange.Type;
                                    success = true;
                                }
                            }
                            else if (slots.Count == 2)
                            {
                                if (slotChange.Slot1 == slots[0] && slotChange.Slot2 == slots[1] || slotChange.Slot1 == slots[1] && slotChange.Slot2 == slots[0])
                                {
                                    sql = "UPDATE inventories SET slot = " + slotChange.Slot2 + " WHERE account = '" + client.Account + "' AND slot = " + slotChange.Slot1 + " AND type = " + slotChange.Type;
                                    table = new MySqlCommand(sql, connection).ExecuteReader();
                                    table.Close();
                                    sql = "UPDATE inventories SET slot = " + slotChange.Slot1 + " WHERE account = '" + client.Account + "' AND slot = " + slotChange.Slot2 + " AND type = " + slotChange.Type;
                                    success = true;
                                }
                            }
                            if (success)
                            {
                                table = new MySqlCommand(sql, connection).ExecuteReader();
                                table.Close();
                                Console.WriteLine("scc");
                                serverPacket = new ServerPacket.ChangeItemSlot
                                {
                                    Type = slotChange.Type,
                                    Slot1 = slotChange.Slot1,
                                    Slot2 = slotChange.Slot2
                                };
                                dgram = ZeroFormatterSerializer.Serialize(serverPacket);
                                Send(dgram, remoteEP);
                            }
                            break;
                        }
                    case ClientPacket.PacketType.UseItem:
                        {
                            ClientPacket.UseItem useItem = (ClientPacket.UseItem)deserialize;
                            transaction = connection.BeginTransaction();

                            bool result = false;
                            if (useItem.Type == 0)
                            {
                                sql = "SELECT item, equipment_number FROM inventories WHERE account = " + client.Account + " AND type = 0 AND slot = " + useItem.Slot;
                                table = new MySqlCommand(sql, connection).ExecuteReader();
                                int iv_item = -1;

                                if (table.Read())
                                {
                                    iv_item = table.GetInt32("item");
                                    int iv_EquipmentNumber = table.GetInt32("equipment_number");
                                    table.Close();



                                    sql = "SELECT equipment_number, item FROM wearing_equipment WHERE account = " + client.Account + " AND slot = 0";
                                    table = new MySqlCommand(sql, connection).ExecuteReader();
                                    if (table.Read()) // 이미 착용중일 경우 벗음
                                    {
                                        int eq_EquipmentNumber = table.GetInt32("equipment_number");
                                        int eq_item = table.GetInt32("item");
                                        table.Close();
                                        sql = "DELETE FROM inventories WHERE account = " + client.Account + " AND type = 0 AND slot = " + useItem.Slot;
                                        command.CommandText = sql;
                                        command.ExecuteNonQuery();

                                        sql = "DELETE FROM wearing_equipment WHERE account = " + client.Account + " AND slot = 0";
                                        command.CommandText = sql;
                                        command.ExecuteNonQuery();

                                        sql = "INSERT INTO wearing_equipment(account, slot, equipment_number, item) " +
                                            "VALUES (" + client.Account + ", " + iv_item / 10000 + ", " + iv_EquipmentNumber + ", " + iv_item + ")";
                                        command.CommandText = sql;
                                        command.ExecuteNonQuery();

                                        sql = "INSERT INTO inventories(account, type, slot, item, count, equipment_number) " +
                                            "VALUES(" + client.Account + ", 0, " + useItem.Slot + ", " + eq_item + ", 1, " + eq_EquipmentNumber + ")";
                                        command.CommandText = sql;
                                        command.ExecuteNonQuery();
                                    }
                                    else
                                    {
                                        table.Close();
                                        sql = "DELETE FROM inventories WHERE account = " + client.Account + " AND type = 0 AND slot = " + useItem.Slot;
                                        command.CommandText = sql;
                                        command.ExecuteNonQuery();

                                        sql = "INSERT INTO wearing_equipment(account, slot, equipment_number, item) " +
                                            "VALUES (" + client.Account + ", " + iv_item / 10000 + ", " + iv_EquipmentNumber + ", " + iv_item + ")";
                                        command.CommandText = sql;
                                        command.ExecuteNonQuery();
                                    }
                                    transaction.Commit();
                                    result = true;

                                }
                                table.Close();
                                if (result)
                                {
                                    serverPacket = new ServerPacket.PutOnPlayer { Player = client.Player, Item = iv_item };
                                    dgram = ZeroFormatterSerializer.Serialize(serverPacket);
                                    SendThisMap(dgram, remoteEP, true, false);
                                    serverPacket = new ServerPacket.UseItem { Type = useItem.Type, Slot = useItem.Slot, Item = iv_item };
                                    dgram = ZeroFormatterSerializer.Serialize(serverPacket);
                                    Send(dgram, remoteEP);
                                }
                            }
                            transaction.Dispose();
                            break;
                        }
                    case ClientPacket.PacketType.TakeOffEquipment:
                        {
                            ClientPacket.TakeOffEquipment takeOffEquipment = (ClientPacket.TakeOffEquipment)deserialize;

                            sql = "SELECT equipment_number, item FROM wearing_equipment WHERE account = " + client.Account + " AND slot = " + takeOffEquipment.SubType;
                            table = new MySqlCommand(sql, connection).ExecuteReader();
                            if (table.Read())
                            {
                                int equipment_number = table.GetInt32("equipment_number");
                                int item = table.GetInt32("item");
                                table.Close();

                                transaction = connection.BeginTransaction();
                                sql = "DELETE FROM wearing_equipment WHERE account = " + client.Account + " AND slot = " + takeOffEquipment.SubType;
                                command.CommandText = sql;
                                if (0 < command.ExecuteNonQuery())
                                {
                                    if (takeOffEquipment.InventorySlot == -1)
                                    {
                                        sql = "SELECT max_equ FROM account WHERE account = " + client.Account;
                                        table = new MySqlCommand(sql, connection).ExecuteReader();
                                        int max_equ = 0;
                                        if (table.Read())
                                            max_equ = table.GetInt32("max_equ");
                                        table.Close();
                                        bool fullInventory = false;
                                        sql = "SELECT slot FROM inventories WHERE account = " + client.Account + " AND type = 0 ORDER BY SLOT";
                                        table = new MySqlCommand(sql, connection).ExecuteReader();
                                        int i = 0;
                                        while (table.Read())
                                        {
                                            if (table.GetInt32("slot") != i)
                                                break;
                                            if (i == max_equ)
                                                fullInventory = true;
                                            i++;
                                        }
                                        table.Close();
                                        if (!fullInventory)
                                        {
                                            sql = "INSERT INTO inventories(account, type, slot, item, count, equipment_number) " +
                                                "VALUES(" + client.Account + ", 0, " + i + ", " + item + ", 1, " + equipment_number + ")";
                                        }
                                        else sql = null;
                                    }
                                    else
                                    {
                                        sql = "INSERT INTO inventories(account, type, slot, item, count, equipment_number) " +
                                            "VALUES(" + client.Account + ", 0, " + takeOffEquipment.InventorySlot + ", " + item + ", 1, " + equipment_number + ")";
                                    }
                                    if (sql != null)
                                    {
                                        command.CommandText = sql;
                                        if (0 < command.ExecuteNonQuery())
                                        {
                                            transaction.Commit();
                                            serverPacket = new ServerPacket.TakeOffPlayer { Player = client.Player, SubType = takeOffEquipment.SubType };
                                            dgram = ZeroFormatterSerializer.Serialize(serverPacket);
                                            SendThisMap(dgram, remoteEP, true, false);

                                            serverPacket = new ServerPacket.TakeOffEquipment { SubType = takeOffEquipment.SubType, InventorySlot = takeOffEquipment.InventorySlot };
                                            dgram = ZeroFormatterSerializer.Serialize(serverPacket);
                                            Send(dgram, remoteEP);
                                        }
                                        else transaction.Rollback();
                                    }
                                    else transaction.Rollback();
                                }
                                else transaction.Rollback();
                                transaction.Dispose();
                            }
                            break;
                        }
                    default:
                        break;
                }
                if (table != null)
                    table.Close();
            }
        }

        private static void DisconnectPlayer(IPEndPoint remoteEP, bool willSendHeadlessServer)
        {
            ClientInfo client = userList[remoteEP.ToString()];
            serverPacket = new ServerPacket.DisconnectedPlayer { Id = client.Player };
            byte[] dgram = ZeroFormatterSerializer.Serialize(serverPacket);
            Console.WriteLine("삭제2");
            client.Map.UserList.Remove(remoteEP.ToString());
            SendThisMap(dgram, remoteEP, willSendHeadlessServer, true);
        }

        private static void Send(byte[] dgram, IPEndPoint remoteEP)
        {
            srv.Send(dgram, dgram.Length, remoteEP);
            Console.WriteLine("[Send] {0} 로 {1} 바이트 송신\tPacketType : {2}", remoteEP.ToString(), dgram.Length, serverPacket.packetType.ToString("g"));
        }

        private static void SendThisMap(byte[] dgram, IPEndPoint remoteEP, bool willSendHeadlessServer, List<string> excepts)
        {
            Console.WriteLine(remoteEP.ToString());
            Dictionary<string, ClientInfo> mapUserList = userList[remoteEP.ToString()].Map.UserList;
            if (willSendHeadlessServer)
            {
                srv.Send(dgram, dgram.Length, hRemoteEP); // 데이터 송신
                Console.WriteLine("[Send] {0} 로 {1} 바이트 송신\tPacketType : {2}", hRemoteEP.ToString(), dgram.Length, serverPacket.packetType.ToString("g"));
            }
            foreach (var key in mapUserList.Keys.ToList())
            {
                bool isExcept = false;
                foreach (string except in excepts)
                {
                    if (mapUserList[key].Player == except)
                    {
                        isExcept = true;
                        break;
                    }
                }
                if (!isExcept)
                {
                    srv.Send(dgram, dgram.Length, mapUserList[key].RemoteEP); // 데이터 송신
                    Console.WriteLine("[Send] {0} 로 {1} 바이트 송신\tPacketType : {2}", mapUserList[key].RemoteEP.ToString(), dgram.Length, serverPacket.packetType.ToString("g"));
                }
            }
        }

        private static void SendThisMap(byte[] dgram, IPEndPoint remoteEP, bool willSendHeadlessServer, bool willSendSelf)
        {
            Dictionary<string, ClientInfo> mapUserList = userList[remoteEP.ToString()].Map.UserList;
            if (willSendHeadlessServer)
            {
                srv.Send(dgram, dgram.Length, hRemoteEP); // 데이터 송신
                Console.WriteLine("[Send] {0} 로 {1} 바이트 송신\tPacketType : {2}", hRemoteEP.ToString(), dgram.Length, serverPacket.packetType.ToString("g"));
            }
            if(willSendSelf)
            {
                foreach (var key in mapUserList.Keys.ToList())
                {
                    srv.Send(dgram, dgram.Length, mapUserList[key].RemoteEP); // 데이터 송신
                    Console.WriteLine("[Send] {0} 로 {1} 바이트 송신\tPacketType : {2}", mapUserList[key].RemoteEP.ToString(), dgram.Length, serverPacket.packetType.ToString("g"));
                }
            }
            else
            {
                foreach (var key in mapUserList.Keys.ToList())
                {
                    if(mapUserList[key].RemoteEP.ToString() != remoteEP.ToString())
                    {
                        srv.Send(dgram, dgram.Length, mapUserList[key].RemoteEP); // 데이터 송신
                        Console.WriteLine("[Send] {0} 로 {1} 바이트 송신\tPacketType : {2}", mapUserList[key].RemoteEP.ToString(), dgram.Length, serverPacket.packetType.ToString("g"));
                    }
                }
            }
        }
        static private void SetTimer()
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

            foreach (var key in userList.Keys.ToList())
            {
                if (userList[key].ConnectionState-- > 0)
                {
                    srv.Send(dgram, dgram.Length, userList[key].RemoteEP); // 데이터 송신
                }
                else
                {
                    DisconnectPlayer(userList[key].RemoteEP, true);
                    userListByName.Remove(userList[key].Player);
                    userList.Remove(key);

                    Console.WriteLine("삭제1");
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