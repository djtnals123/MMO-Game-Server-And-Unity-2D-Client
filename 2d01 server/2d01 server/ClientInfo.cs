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
        public IPEndPoint RemoteEP { get; set; }
        public string Player { get; set; }
        public int ConnectionState { get; set; }
        public int Account { get; set; }
        private Map _map;
        public bool MapCheck { get; private set; }

        public ClientInfo(IPEndPoint remoteEP, string player, int account, Map map)
        {
            RemoteEP = remoteEP;
            Player = player;
            Account = account;
            ConnectionState = 5;
            _map = map;
            MapCheck = false;
        }

        public Map Map
        {
            get
            {
                return _map;
            }
            set
            {
                MapCheck = !MapCheck;
                _map = value;
            }
        }
    }


}
