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
        Login, ObjectSynchronization, PlayerSetting, HPlayerHpSynchronization, ConnectionAck, Disconnected, ChangeItemSlot, 
        UseItem, TakeOffEquipment, HWarpMap
    }
    [Union(typeof(Login), typeof(ObjectSynchronization), typeof(PlayerSetting), typeof(HPlayerHpSynchronization), 
        typeof(ConnectionAck), typeof(Disconnected), typeof(ChangeItemSlot), typeof(UseItem), typeof(TakeOffEquipment), typeof(HWarpMap))]
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
    public class HPlayerHpSynchronization : Packet
    {
        public override PacketType packetType
        {
            get
            {
                return PacketType.HPlayerHpSynchronization;
            }
        }
        [Index(0)] public virtual string Player { get; set; }
        [Index(1)] public virtual short VariationHP { get; set; }
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
    public class HWarpMap : Packet
    {
        public override PacketType packetType
        {
            get
            {
                return PacketType.HWarpMap;
            }
        }
        [Index(0)] public virtual string RemoteEP { get; set; }
        [Index(1)] public virtual int Map { get; set; }
    }
}

