using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace _2d01_server
{
    static class Handler
    {
        public static string PacketHandler(string[] dgramSplit, IPEndPoint remoteEP)
        {
            string message = ServerPacketHandler(dgramSplit, remoteEP);
            if (message == null)
            {
                message = DbPacketHandler(dgramSplit);
                if (message == null) 
                    message = "FAILURE";
            }
            return message;
        }

        private static string DbPacketHandler(string[] dgramSplit)
        {
            string message;
            string sql;
            MySqlDataReader table = null;

            switch (dgramSplit[0])
            {
                case "LOGIN":
                    sql = "SELECT * FROM account WHERE id = '" + dgramSplit[1] + "' AND password = '" + dgramSplit[2] + "'";
                    table = new MySqlCommand(sql, Transport.GetConnection()).ExecuteReader();
                    if (table.HasRows)
                    {
                        message = "LOGIN_SUCCESS";
                    }
                    else message = "LOGIN_FAILURE";
                    break;
                case "HPCHANGE":
                    sql = "UPDATE account SET hp = hp " + dgramSplit[2] + " WHERE " + "id = '" + dgramSplit[1] + "'";
                    table = new MySqlCommand(sql, Transport.GetConnection()).ExecuteReader();

                    if (table.RecordsAffected > 0)
                    {
                        table.Close();
                        sql = "SELECT maxhp, hp FROM account WHERE id = '" + dgramSplit[1] + "'";
                        table = new MySqlCommand(sql, Transport.GetConnection()).ExecuteReader();
                        table.Read();
                        message = "HPCHANGE_SUCCESS|" + table["maxhp"] + "|" + table["hp"];
                        Console.WriteLine("[Send] {0} ", table["hp"]);
                    }
                    else message = "HPCHANGE_FAILURE";
                    break;
                case "GET_INVENTORY":
                    sql = "SELECT max_inventory, account FROM account WHERE id = '" + dgramSplit[1] + "'";
                    table = new MySqlCommand(sql, Transport.GetConnection()).ExecuteReader();
                    table.Read();
                    if (table.HasRows)
                    {
                        string max_inventory = table["max_inventory"].ToString();
                        string accout = table["account"].ToString();
                        table.Close();
                        sql = "SELECT slot, item, count FROM inventories WHERE account = '" + accout + "'";
                        table = new MySqlCommand(sql, Transport.GetConnection()).ExecuteReader();

                        string messageString = "GET_INVENTORY_SUCCESS|" + max_inventory;
                        while (table.Read())
                        {
                            messageString += "|" + table["slot"] + "|" + table["item"] + "|" + table["count"];
                        }
                        message = messageString;
                    }
                    else message = "GET_INVENTORY_FAILURE";
                    break;
                default:
                    message = null;
                    break;
            }
            if(table != null)
                table.Close();
            return message;
        }

        private static string ServerPacketHandler(string[] dgramSplit, IPEndPoint remoteEP)
        {
            string message;
            switch (dgramSplit[0])
            {
                case "INIT_CLIENT":
                    ClientInfo.GetClientList().Add(new ClientInfo(remoteEP, dgramSplit[1]));
                    message = "SUCCESS_INIT_CLIENT";
                    break;
                case "PLAYER_MOVEMENT":

                    message = null;
                    break;

                default:
                    message = null;
                    break;
            }
            return message;
        }
    }

}
