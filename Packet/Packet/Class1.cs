using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZeroFormatter;



namespace ClientPacket
{
    public enum PacketType
    {
        Login, RequestPlayerList, ObjectSynchronization, GetInventories, AttackPlayer, HpSynchronization, ConnectionAck, Disconnected//AUTHENTICATION_CHECK
    }
    [Union(typeof(Login), typeof(RequestPlayerList), typeof(ObjectSynchronization), typeof(GetInventories), typeof(AttackPlayer), typeof(HpSynchronization), 
        typeof(ConnectionAck), typeof(Disconnected))]
    public abstract class Packet
    {
        [UnionKey] public abstract PacketType packetType { get; }
    }


    [ZeroFormattable]
    public class Login : Packet
    {
        public override PacketType packetType
        {
            get
            {
                return PacketType.Login;
            }
        }

        [Index(0)] public virtual string id { get; set; }
        [Index(1)] public virtual string pass { get; set; }
    }

    [ZeroFormattable]
    public class RequestPlayerList : Packet
    {
        public override PacketType packetType
        {
            get
            {
                return PacketType.RequestPlayerList;
            }
        }
        [Index(0)] public virtual string id { get; set; }
    }

    [ZeroFormattable]
    public class ObjectSynchronization : Packet
    {
        public override PacketType packetType
        {
            get
            {
                return PacketType.ObjectSynchronization;
            }
        }
        [Index(0)] public virtual string name { get; set; }
        [Index(1)] public virtual float positionX { get; set; }
        [Index(2)] public virtual float positionY { get; set; }
        [Index(3)] public virtual float velocityX { get; set; }
        [Index(4)] public virtual float velocityY { get; set; }
        [Index(5)] public virtual float rotation { get; set; }
        [Index(6)] public virtual float angularVelocity { get; set; }
        [Index(7)] public virtual bool flipX { get; set; }
    }
    [ZeroFormattable]
    public class GetInventories : Packet
    {
        public override PacketType packetType
        {
            get
            {
                return PacketType.GetInventories;
            }
        }
        [Index(0)] public virtual string id { get; set; }
    }

    [ZeroFormattable]
    public class AttackPlayer : Packet
    {
        public override PacketType packetType
        {
            get
            {
                return PacketType.AttackPlayer;
            }
        }
        [Index(0)] public virtual string id { get; set; }
    }

    [ZeroFormattable]
    public class HpSynchronization : Packet
    {
        public override PacketType packetType
        {
            get
            {
                return PacketType.HpSynchronization;
            }
        }
        [Index(0)] public virtual int hp { get; set; }
    }


    [ZeroFormattable]
    public class ConnectionAck : Packet
    {
        public override PacketType packetType
        {
            get
            {
                return PacketType.ConnectionAck;
            }
        }
    }

    [ZeroFormattable]
    public class Disconnected : Packet
    {
        public override PacketType packetType
        {
            get
            {
                return PacketType.Disconnected;
            }
        }
    }
}

namespace ServerPacket
{
    public enum PacketType
    {
        InitPlayers, LoginSuccess, Login_Failure, InitPlayer, ObjectSynchronization, SetInventories, Failure, AttackPlayer, HpSynchronization, ConnectionCheck, DisconnectedPlayer
    }
    [Union(typeof(LoginSuccess), typeof(InitPlayers), typeof(LoginFailure), typeof(InitPlayer), typeof(ObjectSynchronization), typeof(SetInventories), 
        typeof(AttackPlayer), typeof(HpSynchronization), typeof(ConnectionCheck), typeof(DisconnectedPlayer))]
    public abstract class Packet
    {
        [UnionKey] public abstract PacketType packetType { get; }
    }

    [ZeroFormattable]
    public class LoginSuccess : Packet
    {
        public override PacketType packetType
        {
            get
            {
                return PacketType.LoginSuccess;
            }
        }
    }

    [ZeroFormattable]
    public class LoginFailure : Packet
    {
        public override PacketType packetType
        {
            get
            {
                return PacketType.Login_Failure;
            }
        }
    }

    [ZeroFormattable]
    public class InitPlayers : Packet
    {
        public override PacketType packetType
        {
            get
            {
                return PacketType.InitPlayers;
            }
        }
        [Index(0)] public virtual List<string> players { get; set; }
    }

    [ZeroFormattable]
    public class InitPlayer : Packet
    {
        public override PacketType packetType
        {
            get
            {
                return PacketType.InitPlayer;
            }
        }
        [Index(0)] public virtual string player { get; set; }
    }


    [ZeroFormattable]
    public class ObjectSynchronization : Packet
    {
        public override PacketType packetType
        {
            get
            {
                return PacketType.ObjectSynchronization;
            }
        }
        [Index(0)] public virtual string name { get; set; }
        [Index(1)] public virtual float positionX { get; set; }
        [Index(2)] public virtual float positionY { get; set; }
        [Index(3)] public virtual float velocityX { get; set; }
        [Index(4)] public virtual float velocityY { get; set; }
        [Index(5)] public virtual float rotation { get; set; }
        [Index(6)] public virtual float angularVelocity { get; set; }
        [Index(7)] public virtual bool flipX { get; set; }
    }
    [ZeroFormattable]
    public class SetInventories : Packet
    {
        public override PacketType packetType
        {
            get
            {
                return PacketType.SetInventories;
            }
        }
        [Index(0)] public virtual int maxInventorySlot { get; set; }
        [Index(1)] public virtual List<int> slot { get; set; }
        [Index(2)] public virtual List<int> item { get; set; }
        [Index(3)] public virtual List<int> count { get; set; }
    }
    
    [ZeroFormattable]
    public class AttackPlayer : Packet
    {
        public override PacketType packetType
        {
            get
            {
                return PacketType.AttackPlayer;
            }
        }
        [Index(0)] public virtual string id { get; set; }
    }

    [ZeroFormattable]
    public class HpSynchronization : Packet
    {
        public override PacketType packetType
        {
            get
            {
                return PacketType.HpSynchronization;
            }
        }
        [Index(0)] public virtual string id { get; set; }
        [Index(1)] public virtual int maxHp { get; set; }
        [Index(2)] public virtual int hp { get; set; }
    }

    [ZeroFormattable]
    public class ConnectionCheck : Packet
    {
        public override PacketType packetType
        {
            get
            {
                return PacketType.ConnectionCheck;
            }
        }
    }

    [ZeroFormattable]
    public class DisconnectedPlayer : Packet
    {
        public override PacketType packetType
        {
            get
            {
                return PacketType.DisconnectedPlayer;
            }
        }
        [Index(0)] public virtual string id { get; set; }
    }
}