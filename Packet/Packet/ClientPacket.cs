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
        Login, RequestPlayerList, ObjectSynchronization, PlayerSetting, AttackPlayer, HpSynchronization, ConnectionAck, Disconnected, ChangeItemSlot, 
        UseItem, EnableSpace, TakeOffEquipment, WarpMap
    }
    [Union(typeof(Login), typeof(RequestPlayerList), typeof(ObjectSynchronization), typeof(PlayerSetting), typeof(AttackPlayer), typeof(HpSynchronization), 
        typeof(ConnectionAck), typeof(Disconnected), typeof(ChangeItemSlot), typeof(UseItem), typeof(EnableSpace), typeof(TakeOffEquipment), typeof(WarpMap) )]
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

        [Index(0)] public virtual string Id { get; set; }
        [Index(1)] public virtual string Pass { get; set; }
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
        [Index(0)] public virtual string Id { get; set; }
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
        [Index(0)] public virtual string Name { get; set; }
        [Index(1)] public virtual float PositionX { get; set; }
        [Index(2)] public virtual float PositionY { get; set; }
        [Index(3)] public virtual float VelocityX { get; set; }
        [Index(4)] public virtual float VelocityY { get; set; }
        [Index(5)] public virtual float Rotation { get; set; }
        [Index(6)] public virtual float AngularVelocity { get; set; }
        [Index(7)] public virtual bool FlipX { get; set; }
        [Index(8)] public virtual bool MapCheck { get; set; }
    }
    [ZeroFormattable]
    public class PlayerSetting : Packet
    {
        public override PacketType packetType
        {
            get
            {
                return PacketType.PlayerSetting;
            }
        }
        [Index(0)] public virtual string Id { get; set; }
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
        [Index(0)] public virtual string Id { get; set; }
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
        [Index(0)] public virtual short Hp { get; set; }
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
    [ZeroFormattable]
    public class ChangeItemSlot : Packet
    {
        public override PacketType packetType
        {
            get
            {
                return PacketType.ChangeItemSlot;
            }
        }
        [Index(0)] public virtual sbyte Type { get; set; }
        [Index(1)] public virtual short Slot1 { get; set; }
        [Index(2)] public virtual short Slot2 { get; set; }
    }
    [ZeroFormattable]
    public class UseItem : Packet
    {
        public override PacketType packetType
        {
            get
            {
                return PacketType.UseItem;
            }
        }
        [Index(0)] public virtual sbyte Type { get; set; }
        [Index(1)] public virtual short Slot { get; set; }
    }
    [ZeroFormattable]
    public class EnableSpace : Packet
    {
        public override PacketType packetType
        {
            get
            {
                return PacketType.EnableSpace;
            }
        }
        [Index(0)] public virtual bool Enable { get; set; }
    }
    [ZeroFormattable]
    public class TakeOffEquipment : Packet
    {
        public override PacketType packetType
        {
            get
            {
                return PacketType.TakeOffEquipment;
            }
        }
        [Index(0)] public virtual sbyte SubType { get; set; }
        [Index(1)] public virtual short InventorySlot { get; set; }
    }
    [ZeroFormattable]
    public class WarpMap : Packet
    {
        public override PacketType packetType
        {
            get
            {
                return PacketType.WarpMap;
            }
        }
        [Index(0)] public virtual int Portal { get; set; }
        [Index(1)] public virtual bool FlipX { get; set; }
    }

}

