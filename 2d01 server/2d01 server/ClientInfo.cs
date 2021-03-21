using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace _2d01_server
{


    class ClientInfo
    {
        private static List<ClientInfo> clientList = new List<ClientInfo>();
        private IPEndPoint remoteEP;
        private string player;

        public ClientInfo(IPEndPoint remoteEP, string player)
        {
            this.remoteEP = remoteEP;
            this.player = player;
        }

        public IPEndPoint GetAddress() { return remoteEP; }

        public string GetPlayer() { return player; }

        public static List<ClientInfo> GetClientList() { return clientList; }


        public static IPEndPoint FindClientAddress(string player)
        {
            for (int i = 0; i < clientList.Count; i++)
            {
                if (clientList[i].GetPlayer() == player)
                    return clientList[i].GetAddress();
            }
            return null;
        }
    }
}
