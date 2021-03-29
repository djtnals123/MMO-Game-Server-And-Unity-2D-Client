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
            try
            {
                Transport.StartServer();
            }
            catch (Exception ex)
            {
                Console.WriteLine("실패");
                Console.WriteLine(ex.ToString());
            }
            
        }

        ~Program()
        {
            Transport.Close();
        }
    }
}
