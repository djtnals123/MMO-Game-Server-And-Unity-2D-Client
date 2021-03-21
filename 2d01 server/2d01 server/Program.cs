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
    class Program
    {
        static void Main(string[] args)
        {
            using (MySqlConnection connection = new MySqlConnection("Server=localhost;Port=3306;Database=2d01;Uid=root;Pwd=root"))
            {
                try
                {
                    connection.Open();

                    UdpClient srv = new UdpClient(7777); // 포트 7777
                    IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0); // 클라이언트 IP

                    while (true)
                    {
                        byte[] dgram = srv.Receive(ref remoteEP); // 데이터 수신
                        Console.WriteLine("[Receive] {0} 로부터 {1} 바이트 수신", remoteEP.ToString(), dgram.Length);

                        string dgramString = Encoding.Default.GetString(dgram);
                        string[] dgramSplit = dgramString.Split('|');

                        string sql;
                        byte[] message;
                        MySqlDataReader table=null;

                        switch (dgramSplit[0])
                        {
                            case "LOGIN":
                                sql = "SELECT * FROM account WHERE id = '" + dgramSplit[1] + "' AND password = '" + dgramSplit[2]+"'";
                                table = new MySqlCommand(sql, connection).ExecuteReader();
                                if(table.HasRows)
                                {
                                    message = Encoding.UTF8.GetBytes("LOGIN_SUCCESS");
                                }
                                else message = Encoding.UTF8.GetBytes("LOGIN_FAILURE");
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
                                    message = Encoding.UTF8.GetBytes("HPCHANGE_SUCCESS|" + table["maxhp"] + "|" + table["hp"]);
                                    Console.WriteLine("[Send] {0} ", table["hp"]);
                                }
                                else message = Encoding.UTF8.GetBytes("HPCHANGE_FAILURE");
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

                                    string messageString = "GET_INVENTORY_SUCCESS|" + max_inventory;
                                    while (table.Read())
                                    {
                                        messageString += "|" + table["slot"] + "|" + table["item"] + "|" + table["count"];
                                    }
                                    message = Encoding.UTF8.GetBytes(messageString);
                                }
                                else  message = Encoding.UTF8.GetBytes("GET_INVENTORY_FAILURE");
                                break;
                            default:
                                message = Encoding.UTF8.GetBytes("FAILURE");
                                break;
                        }
                        table.Close();
                        srv.Send(message, message.Length, remoteEP); // 데이터 송신
                        Console.WriteLine("[Send] {0} 로 {1} 바이트 송신 {2}", remoteEP.ToString(), message.Length, Encoding.Default.GetString(message));




                    }

           //         while (table.Read())
               //     {
             //           Console.WriteLine("{0} {1}", table["id"], table["password"]);
                 //   }

                }
                catch (Exception ex)
                {
                    Console.WriteLine("실패");
                    Console.WriteLine(ex.ToString());
                }
            }
        }
    }
}
