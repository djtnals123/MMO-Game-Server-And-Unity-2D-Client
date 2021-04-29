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
        public ObjectInfo ObjectInfo { get; set; }
        public bool MapCheck { get; private set; }

        public ClientInfo(IPEndPoint remoteEP, string player, int account, Map map, ObjectInfo objectInfo)
        {
            RemoteEP = remoteEP;
            Player = player;
            Account = account;
            ConnectionState = 5;
            _map = map;
            ObjectInfo = objectInfo;
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


    public struct ObjectInfo
    {
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public float VelocityX { get; set; }
        public float VelocityY { get; set; }
        public float Rotation { get; set; }
        public float AngularVelocity { get; set; }
        public bool FlipX { get; set; }

        public ObjectInfo(float positionX, float positionY, float velocityX, float velocityY, float rotation, float angularVelocity, bool flipX)
        {
            PositionX = positionX;
            PositionY = positionY;
            VelocityX = velocityX;
            VelocityY = velocityY;
            Rotation = rotation;
            AngularVelocity = angularVelocity;
            FlipX = flipX;
        }
    }
}
