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
        private static MySqlCommand command = connection.CreateCommand();
        private static MySqlTransaction transaction;

        public static void StartServer()
        {
            connection.Open();
            command.Connection = connection;
            command.Transaction = transaction;
            setTimer();

            while (true)
            {
                try
                {
                    byte[] dgram = ReceivePacket();
                    if (dgram != null)
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
            ClientInfo client = ClientInfo.Find(remoteEP);


            switch (deserialize.packetType)
            {

                case ClientPacket.PacketType.ObjectSynchronization:
                    ClientPacket.ObjectSynchronization objSynPacket = (ClientPacket.ObjectSynchronization)deserialize;
                    serverPacket = new ServerPacket.ObjectSynchronization()
                    {
                        Name = objSynPacket.Name,
                        PositionX = objSynPacket.PositionX,
                        PositionY = objSynPacket.PositionY,
                        VelocityX = objSynPacket.VelocityX,
                        VelocityY = objSynPacket.VelocityY,
                        Rotation = objSynPacket.Rotation,
                        AngularVelocity = objSynPacket.AngularVelocity,
                        FlipX = objSynPacket.FlipX
                    };
                    excepts = new List<string>();
                    excepts.Add(objSynPacket.Name);
                    dgram = ZeroFormatterSerializer.Serialize<ServerPacket.Packet>(serverPacket);
                    Multicast(dgram, excepts);
                    break;
                case ClientPacket.PacketType.Login:
                    ClientPacket.Login loginPacket = (ClientPacket.Login)deserialize;
                    sql = "SELECT * FROM account WHERE id = '" + loginPacket.Id + "' AND password = '" + loginPacket.Pass + "'";
                    table = new MySqlCommand(sql, connection).ExecuteReader();
                    if (table.Read())
                    {
                        ClientInfo.ClientList.Add(new ClientInfo(remoteEP, loginPacket.Id, table.GetInt32("account")));
                        table.Close();
                        client = ClientInfo.ClientList[ClientInfo.ClientList.Count - 1];
                        dgram = ZeroFormatterSerializer.Serialize<ServerPacket.Packet>(new ServerPacket.LoginSuccess());
                        Unicast(dgram);

                        sql = "SELECT item FROM wearing_equipment WHERE account = " + client.Account;
                        table = new MySqlCommand(sql, connection).ExecuteReader();
                        List<int> equips = new List<int>();
                        while(table.Read())
                        {
                            equips.Add(table.GetInt32("item"));
                        }

                        excepts = new List<string>();
                        excepts.Add(loginPacket.Id);
                        dgram = ZeroFormatterSerializer.Serialize<ServerPacket.Packet>(new ServerPacket.InitPlayer { Player = loginPacket.Id, Equips = equips});
                        Multicast(dgram, excepts);
                    }
                    else serverPacket = new ServerPacket.LoginFailure();
                    break;
                case ClientPacket.PacketType.RequestPlayerList:
                    List<string> otherPlayers = new List<string>();
                    List<List<int>> equipss = new List<List<int>>();
                    int i = 0;
                    foreach (ClientInfo c in ClientInfo.ClientList)
                    {
                        if (c.Player != client.Player)
                        {
                            equipss.Add(new List<int>());
                            otherPlayers.Add(c.Player);

                            sql = "SELECT item FROM wearing_equipment WHERE account = " + c.Account;
                            table = new MySqlCommand(sql, connection).ExecuteReader();
                            while (table.Read())
                                equipss[i].Add(table.GetInt32("item"));
                            table.Close();
                            i++;
                        }
                    }
                    dgram = ZeroFormatterSerializer.Serialize<ServerPacket.Packet>(new ServerPacket.InitPlayers { Players = otherPlayers, Equips = equipss });
                    Unicast(dgram);

                    break;
                case ClientPacket.PacketType.AttackPlayer:
                    ClientPacket.AttackPlayer atkPacket = (ClientPacket.AttackPlayer)deserialize;
                    serverPacket = new ServerPacket.AttackPlayer() { Id = atkPacket.Id };

                    excepts = new List<string>();
                    excepts.Add(atkPacket.Id);
                    dgram = ZeroFormatterSerializer.Serialize<ServerPacket.Packet>(serverPacket);
                    Multicast(dgram, excepts);

                    break;


                case ClientPacket.PacketType.HpSynchronization:
                    ClientPacket.HpSynchronization C_hpSyn = (ClientPacket.HpSynchronization)deserialize;
                    string hp = C_hpSyn.Hp.ToString();
                    if(C_hpSyn.Hp >= 0)
                        hp = hp.Insert(0, "+");
                    sql = "UPDATE account SET hp = hp " + hp + " WHERE id = '" + playerName + "'";
                    table = new MySqlCommand(sql, connection).ExecuteReader();

                    if (table.RecordsAffected > 0)
                    {
                        table.Close();
                        sql = "SELECT max_hp, hp FROM account WHERE id = '" + playerName + "'";
                        table = new MySqlCommand(sql, connection).ExecuteReader();
                        if (table.Read())
                        {
                            ServerPacket.HpSynchronization S_hpSyn = new ServerPacket.HpSynchronization
                            {
                                Id = playerName,
                                Hp = table.GetInt16("hp"),
                                MaxHp = table.GetInt16("max_hp")
                            };
                            dgram = ZeroFormatterSerializer.Serialize<ServerPacket.Packet>(S_hpSyn);
                            Broadcast(dgram);
                        }
                    }
                    else;
                    break;

                case ClientPacket.PacketType.PlayerSetting:
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
                        sql = "SELECT item FROM wearing_equipment WHERE account = " + client.Account;
                        table = new MySqlCommand(sql, connection).ExecuteReader();
                        List<int> wearingEquip = new List<int>();
                        while (table.Read())
                        {
                            wearingEquip.Add(table.GetInt32("item"));
                        }
                        ServerPacket.PlayerSetting setInventoriesPacket = new ServerPacket.PlayerSetting
                        {
                            MaxEquipmentSlot = maxEquipmentSlot,
                            MaxUseableSlot = maxUseableSlot,
                            MaxEtcSlot = maxEtcSlot,
                            MaxEnhancementSlot = maxEnhancementSlot,
                            Slot = slot,
                            Type = type,
                            Item = item,
                            Count = count,
                            Equip = wearingEquip
                        };
                        dgram = ZeroFormatterSerializer.Serialize<ServerPacket.Packet>(setInventoriesPacket);
                        Unicast(dgram);
                    }
                    break;
                case ClientPacket.PacketType.ConnectionAck:
                    if (client != null)
                        client.ConnectionState = 5;
                    break;
                case ClientPacket.PacketType.Disconnected:
                    ClientInfo disconnected = ClientInfo.Find(remoteEP);
                    if (disconnected != null)
                    {
                        dgram = ZeroFormatterSerializer.Serialize<ServerPacket.Packet>(new ServerPacket.DisconnectedPlayer { Id = disconnected.Player });
                        ClientInfo.ClientList.Remove(disconnected);
                        Console.WriteLine("삭제2");
                        Broadcast(dgram);
                    }
                    break;
                case ClientPacket.PacketType.ChangeItemSlot:
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
                    else if(slots.Count == 2)
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
                    if(success)
                    {
                        table = new MySqlCommand(sql, connection).ExecuteReader();
                        table.Close();
                        Console.WriteLine("scc");
                        ServerPacket.ChangeItemSlot changeItemSlot = new ServerPacket.ChangeItemSlot
                        {
                            Type = slotChange.Type,
                            Slot1 = slotChange.Slot1,
                            Slot2 = slotChange.Slot2
                        };
                        dgram = ZeroFormatterSerializer.Serialize<ServerPacket.Packet>(changeItemSlot);
                        Unicast(dgram);
                    }
                    break;
                case ClientPacket.PacketType.UseItem:
                    ClientPacket.UseItem useItem = (ClientPacket.UseItem)deserialize;
                    transaction = connection.BeginTransaction();

                    bool result = false;
                    if(useItem.Type == 0)
                    {
                        sql = "SELECT item, equipment_number FROM inventories WHERE account = " + client.Account + " AND type = 0 AND slot = " + useItem.Slot;
                        table = new MySqlCommand(sql, connection).ExecuteReader();
                        int iv_item = -1;

                        if(table.Read())
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
                            dgram = ZeroFormatterSerializer.Serialize<ServerPacket.Packet>(new ServerPacket.PutOnPlayer { Player = client.Player, Item =  iv_item});
                            excepts = new List<string>();
                            excepts.Add(client.Player);
                            Multicast(dgram, excepts);
                            dgram = ZeroFormatterSerializer.Serialize<ServerPacket.Packet>(new ServerPacket.UseItem { Type = useItem.Type, Slot = useItem.Slot, Item = iv_item});
                            Unicast(dgram);
                        }
                    }
                    transaction.Dispose();
                    break;
                case ClientPacket.PacketType.EnableSpace:
                    ClientPacket.EnableSpace enableSpace = (ClientPacket.EnableSpace)deserialize;
                    dgram = ZeroFormatterSerializer.Serialize<ServerPacket.Packet>(new ServerPacket.EnableSpace { Player = client.Player, Enable = enableSpace.Enable });

                    excepts = new List<string>();
                    excepts.Add(client.Player);
                    Multicast(dgram, excepts);

                    break;

                case ClientPacket.PacketType.TakeOffEquipment:
                    ClientPacket.TakeOffEquipment takeOffEquipment = (ClientPacket.TakeOffEquipment)deserialize;

                    sql = "SELECT equipment_number, item FROM wearing_equipment WHERE account = " + client.Account + " AND slot = " + takeOffEquipment.SubType;
                    table = new MySqlCommand(sql, connection).ExecuteReader();
                    if(table.Read())
                    {
                        int equipment_number = table.GetInt32("equipment_number");
                        int item = table.GetInt32("item");
                        table.Close();

                        transaction = connection.BeginTransaction();
                        sql = "DELETE FROM wearing_equipment WHERE account = " + client.Account + " AND slot = " + takeOffEquipment.SubType;
                        command.CommandText = sql;
                        if (0 < command.ExecuteNonQuery())
                        {
                            if(takeOffEquipment.InventorySlot == -1)
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
                                i = 0;
                                while(table.Read())
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
                            if(sql != null)
                            {
                                command.CommandText = sql;
                                if (0 < command.ExecuteNonQuery())
                                {
                                    transaction.Commit();
                                    dgram = ZeroFormatterSerializer.Serialize<ServerPacket.Packet>(new ServerPacket.TakeOffPlayer
                                    { Player = client.Player, SubType = takeOffEquipment.SubType });
                                    excepts = new List<string>();
                                    excepts.Add(client.Player);
                                    Multicast(dgram, excepts);

                                    dgram = ZeroFormatterSerializer.Serialize<ServerPacket.Packet>(new ServerPacket.TakeOffEquipment
                                    { SubType = takeOffEquipment.SubType, InventorySlot = takeOffEquipment.InventorySlot });
                                    Unicast(dgram);
                                }
                                else transaction.Rollback();
                            }
                            else transaction.Rollback();
                        }
                        else transaction.Rollback();
                        transaction.Dispose();

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
                    byte[] disc = ZeroFormatterSerializer.Serialize<ServerPacket.Packet>(new ServerPacket.DisconnectedPlayer { Id = clientList[i].Player });
                    Broadcast(disc);
                    Console.WriteLine("삭제1");
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