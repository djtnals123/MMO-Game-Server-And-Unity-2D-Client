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
        private static MySqlConnection connection = new MySqlConnection("Server=localhost;Port=3306;Database=2d01;Uid=root;Pwd=root");
        private static UdpClient srv = new UdpClient(7777); // 포트 7777
        public static void ConnectDB() => connection.Open();

        public static MySqlConnection GetConnection() { return connection; }
        
        public static void StartServer()
        {
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0); // 클라이언트 IP

            byte[] dgram = srv.Receive(ref remoteEP); // 데이터 수신
            Console.WriteLine("[Receive] {0} 로부터 {1} 바이트 수신", remoteEP.ToString(), dgram.Length);

            string dgramString = Encoding.Default.GetString(dgram);
            string[] dgramSplit = dgramString.Split('|');

            string message = Handler.PacketHandler(dgramSplit, remoteEP);
            if(message != null)
            {
                srv.Send(Encoding.UTF8.GetBytes(message), message.Length, remoteEP); // 데이터 송신
                Console.WriteLine("[Send] {0} 로 {1} 바이트 송신 {2}", remoteEP.ToString(), message.Length, message);
            }
        }
    }

}
