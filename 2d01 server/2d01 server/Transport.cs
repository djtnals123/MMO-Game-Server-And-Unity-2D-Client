using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace _2d01_server
{
    class Transport
    {
        private static UdpClient srv = new UdpClient(7777); // 포트 7777
        private static IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0); // 클라이언트 IP


        private static MySqlConnection connection = new MySqlConnection("Server=localhost;Port=3306;Database=2d01;Uid=root;Pwd=root");
        public static void ConnectDB() => connection.Open();
        private enum Routing
        {
            Unicast, Multicast
        }


        public static void StartServer()
        {
            try
            {
                string[] splitMessage = PacketParsing(ReceivePacket());
                PacketHandler(splitMessage);
            }
            catch (SocketException e)
            {
                Console.WriteLine("Socket Error. {0} {1}", e.ErrorCode, e.Message);
            }
        }

        private static byte[] ReceivePacket()
        {
            byte[] dgram = srv.Receive(ref remoteEP); // 데이터 수신
            Console.WriteLine("[Receive] {0} 로부터 {1} 바이트 수신 {2}", remoteEP.ToString(), dgram.Length, dgram[0]);
            return dgram;
        }

        private static string[] PacketParsing(byte[] dgram) {
            return Encoding.Default.GetString(dgram).Split('|');
        }


        private static void PacketHandler(string[] dgramSplit)
        {
            string message;
            string sql;
            MySqlDataReader table = null;
            List<string> excepts = null;
            int routing = -1;

            switch (dgramSplit[0])
            {
                case "ObjectSynchronization":
                    excepts = new List<string>();
                    excepts.Add(dgramSplit[1]);
                    message = string.Join("|", dgramSplit);
                    routing = (int)Routing.Multicast;
                    break;
                case "PLAYER_MOVEMENT":

                    message = null;
                    break;
                case "INIT_PLAYERS":
                    message = "INIT_PLAYERS";
                    foreach (ClientInfo client in ClientInfo.GetClientList()) {
                        if (client.GetPlayer() != dgramSplit[1]) {
                            message += "|" + client.GetPlayer();
                        }
                    }
                    routing = (int)Routing.Unicast;
                    break;
                case "LOGIN":
                    sql = "SELECT * FROM account WHERE id = '" + dgramSplit[1] + "' AND password = '" + dgramSplit[2] + "'";
                    table = new MySqlCommand(sql, connection).ExecuteReader();
                    if (table.HasRows)
                    {
                        ClientInfo.GetClientList().Add(new ClientInfo(remoteEP, dgramSplit[1]));
                        message = "LOGIN_SUCCESS";
                        Unicast(message);

                        excepts = new List<string>();
                        excepts.Add(dgramSplit[1]);
                        message = "INIT_PLAYERS|" + dgramSplit[1];
                        routing = (int)Routing.Multicast;
                    }
                    else message = "LOGIN_FAILURE";
                    break;
                case "HPCHANGE":
                    sql = "UPDATE account SET hp = hp " + dgramSplit[2] + " WHERE " + "id = '" + dgramSplit[1] + "'";
                    table = new MySqlCommand(sql, connection).ExecuteReader();

                    if (table.RecordsAffected > 0)
                    {
                        table.Close();
                        sql = "SELECT maxhp, hp FROM account WHERE id = '" + dgramSplit[1] + "'";
                        table = new MySqlCommand(sql, connection).ExecuteReader();
                        table.Read();
                        message = "HPCHANGE_SUCCESS|" + table["maxhp"] + "|" + table["hp"];
                        Console.WriteLine("[Send] {0} ", table["hp"]);
                    }
                    else message = "HPCHANGE_FAILURE";
                    routing = (int)Routing.Unicast;
                    break;
                case "GET_INVENTORY":
                    sql = "SELECT max_inventory, account FROM account WHERE id = '" + dgramSplit[1] + "'";
                    table = new MySqlCommand(sql, connection).ExecuteReader();
                    table.Read();
                    if (table.HasRows)
                    {
                        string max_inventory = table["max_inventory"].ToString();
                        string accout = table["account"].ToString();
                        table.Close();
                        sql = "SELECT slot, item, count FROM inventories WHERE account = '" + accout + "'";
                        table = new MySqlCommand(sql, connection).ExecuteReader();

                        message = "GET_INVENTORY_SUCCESS|" + max_inventory;
                        while (table.Read())
                        {
                            message += "|" + table["slot"] + "|" + table["item"] + "|" + table["count"];
                        }
                    }
                    else message = "GET_INVENTORY_FAILURE";
                    routing = (int)Routing.Unicast;
                    break;
                default:
                    message = null;
                    break;
            }
            if (table != null)
                table.Close();
            if (message != null) // 데이터 송신
            {
                if (routing == (int)Routing.Unicast)
                    Unicast(message);
                else if (routing == (int)Routing.Multicast && excepts != null)
                    Multicast(message, excepts);
            }
        }

        private static void Unicast(string message)
        {
            srv.Send(Encoding.UTF8.GetBytes(message), message.Length, remoteEP); // 데이터 송신
            Console.WriteLine("[Send] {0} 로 {1} 바이트 송신 {2}", remoteEP.ToString(), message.Length, message);
        }

        private static void Multicast(string message, List<string> excepts)
        {
            foreach (string except in excepts) {
                foreach (ClientInfo client in ClientInfo.GetClientList()) {
                    if (client.GetPlayer() != except) {
                        srv.Send(Encoding.UTF8.GetBytes(message), message.Length, client.GetAddress());
                        Console.WriteLine("[Send] {0} 로 {1} 바이트 송신 {2}", client.GetAddress().ToString(), message.Length, message);
                    }
                }
            }
        }

    }

}
