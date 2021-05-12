
using System.Collections.Generic;
using System.Net;
using ZeroFormatter;

namespace ServerPacket
{
    public enum PacketType
    {
        Login_Failure, ObjectSynchronization, PlayerSetting, Failure, HpSynchronization, ConnectionCheck,
        DisconnectedPlayer, ChangeItemSlot, UseItem, PutOnPlayer, TakeOffEquipment, TakeOffPlayer, HInitPlayer,
        
    }
    [Union(typeof(LoginFailure), typeof(PlayerSetting),
        typeof(HpSynchronization), typeof(ConnectionCheck), typeof(DisconnectedPlayer), typeof(ChangeItemSlot), typeof(UseItem),
        typeof(PutOnPlayer), typeof(TakeOffEquipment), typeof(TakeOffPlayer), typeof(HInitPlayer) 
        )]
    public abstract class Packet
    {
        [UnionKey] public abstract PacketType packetType { get; }
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
    public class HInitPlayer : Packet
    {
        public override PacketType packetType
        {
            get
            {
                return PacketType.HInitPlayer;
            }
        }
        [Index(0)] public virtual string Player { get; set; }
        [Index(1)] public virtual List<int> Equips { get; set; }
        [Index(7)] public virtual int Map { get; set; }
        [Index(8)] public virtual string RemoteEP { get; set; }
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
        [Index(0)] public virtual short MaxEquipmentSlot { get; set; }
        [Index(1)] public virtual short MaxUseableSlot { get; set; }
        [Index(2)] public virtual short MaxEtcSlot { get; set; }
        [Index(3)] public virtual short MaxEnhancementSlot { get; set; }
        [Index(4)] public virtual List<short> Slot { get; set; }
        [Index(5)] public virtual List<sbyte> Type { get; set; }
        [Index(6)] public virtual List<int> Item { get; set; }
        [Index(7)] public virtual List<short> Count { get; set; }

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
        [Index(0)] public virtual string Id { get; set; }
        [Index(1)] public virtual int MaxHp { get; set; }
        [Index(2)] public virtual int Hp { get; set; }
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
        [Index(0)] public virtual string Id { get; set; }
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
        [Index(2)] public virtual int Item { get; set; }
    }
    [ZeroFormattable]
    public class PutOnPlayer : Packet
    {
        public override PacketType packetType
        {
            get
            {
                return PacketType.PutOnPlayer;
            }
        }
        [Index(0)] public virtual string Player { get; set; }
        [Index(1)] public virtual int Item { get; set; }
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
    public class TakeOffPlayer : Packet
    {
        public override PacketType packetType
        {
            get
            {
                return PacketType.TakeOffPlayer;
            }
        }
        [Index(0)] public virtual string Player { get; set; }
        [Index(1)] public virtual sbyte SubType { get; set; }
    }
}