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
        private int connectionState;
        private int account;

        public ClientInfo(IPEndPoint remoteEP, string player, int account)
        {
            this.remoteEP = remoteEP;
            this.player = player;
            this.account = account;
            connectionState = 5;
        }


        public int Account
        {
            get
            {
                return account;
            }
            set
            {
                account = value;
            }
        }
        public static List<ClientInfo> ClientList
        {
            get
            {
                return clientList;
            }
            set
            {
                clientList = value;
            }
        }
        public string Player
        {
            get
            {
                return player;
            }
            set
            {
                player = value;
            }
        }
        public int ConnectionState
        {
            get
            {
                return connectionState;
            }
            set
            {
                connectionState = value;
            }
        }
        public IPEndPoint RemoteEP
        {
            get
            {
                return remoteEP;
            }
            set
            {
                remoteEP = value;
            }
        }


        public static IPEndPoint FindClientAddress(string player)
        {
            foreach (ClientInfo client in clientList)
            {
                if (client.player == player)
                    return client.remoteEP;
            }
            return null;
        }

        public static string FindPlayerName(IPEndPoint remoteEP)
        {
            foreach (ClientInfo client in clientList )
            {
                if (client.remoteEP.ToString() == remoteEP.ToString())
                    return client.Player;
            }
            return null;
        }

        public static ClientInfo Find(string player)
        {
            foreach (ClientInfo client in clientList)
            {
                if (client.player == player)
                    return client;
            }
            return null;
        }

        public static ClientInfo Find(IPEndPoint remoteEP)
        {
            foreach (ClientInfo client in clientList)
            {
                if (client.remoteEP.ToString() == remoteEP.ToString())
                    return client;
            }
            return null;
        }
    }
}
