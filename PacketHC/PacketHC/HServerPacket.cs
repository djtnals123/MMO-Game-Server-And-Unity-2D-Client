
using System.Collections.Generic;
using ZeroFormatter;

namespace HServerPacket
{
    public enum PacketType
    {
        InitPlayer, LoginSuccess, ObjectSynchronization, AttackPlayer, EnableSpace, InitObjects, WarpMap, MoveMob, PlayerHpSynchronization,
        MobHpSynchronization, ReSpawnMobs
    }

    [Union(typeof(ObjectSynchronization), typeof(AttackPlayer), typeof(EnableSpace), typeof(LoginSuccess), typeof(InitPlayer), typeof(InitObjects),
        typeof(WarpMap), typeof(MoveMob), typeof(PlayerHpSynchronization), typeof(MobHpSynchronization), typeof(ReSpawnMobs))]
    public abstract class Packet
    {
        [UnionKey] public abstract PacketType packetType { get; }
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
    public class EnableSpace : Packet
    {
        public override PacketType packetType
        {
            get
            {
                return PacketType.EnableSpace;
            }
        }
        [Index(0)] public virtual string Player { get; set; }
        [Index(1)] public virtual bool IsEnabled { get; set; }
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
        [Index(0)] public virtual int Map { get; set; }
        [Index(1)] public virtual float PositionX { get; set; }
        [Index(2)] public virtual float PositionY { get; set; }
        [Index(3)] public virtual List<int> Equips { get; set; }
        [Index(4)] public virtual short MaxHP { get; set; }
        [Index(5)] public virtual short HP { get; set; }
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
        [Index(0)] public virtual string Player { get; set; }
        [Index(1)] public virtual List<int> Equips { get; set; }
        [Index(2)] public virtual float PositionX { get; set; }
        [Index(3)] public virtual float PositionY { get; set; }
        [Index(4)] public virtual bool FlipX { get; set; }
        [Index(5)] public virtual bool MapCheck { get; set; }
        [Index(6)] public virtual short MaxHP { get; set; }
        [Index(7)] public virtual short HP { get; set; }
    }

    [ZeroFormattable]
    public class InitObjects : Packet
    {
        public override PacketType packetType
        {
            get
            {
                return PacketType.InitObjects;
            }
        }
        [Index(0)] public virtual List<string> Players { get; set; }
        [Index(1)] public virtual List<List<int>> Equips { get; set; }
        [Index(2)] public virtual List<float> PositionX { get; set; }
        [Index(3)] public virtual List<float> PositionY { get; set; }
        [Index(4)] public virtual List<float> VelocityX { get; set; }
        [Index(5)] public virtual List<float> VelocityY { get; set; }
        [Index(6)] public virtual List<bool> FlipX { get; set; }
        [Index(7)] public virtual List<int> MaxHP { get; set; }
        [Index(8)] public virtual List<int> HP { get; set; }
        [Index(9)] public virtual LinkedList<int> DeadMobs { get; set; }
        [Index(10)] public virtual LinkedList<sbyte> NextMoves { get; set; }
        [Index(11)] public virtual LinkedList<float> MobPosX { get; set; }
        [Index(12)] public virtual LinkedList<float> MobPosY { get; set; }
        [Index(13)] public virtual LinkedList<short> MobHP { get; set; }
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
        [Index(0)] public virtual int LinkedMap { get; set; }
        [Index(1)] public virtual float PosX { get; set; }
        [Index(2)] public virtual float PosY { get; set; }
    }
    [ZeroFormattable]
    public class MoveMob : Packet
    {
        public override PacketType packetType
        {
            get
            {
                return PacketType.MoveMob;
            }
        }
        [Index(0)] public virtual int MobID { get; set; }
        [Index(1)] public virtual sbyte NextMove { get; set; }
        [Index(2)] public virtual float PosX { get; set; }
        [Index(3)] public virtual float PosY { get; set; }
        [Index(4)] public virtual float VelY { get; set; }
    }
    [ZeroFormattable]
    public class PlayerHpSynchronization : Packet
    {
        public override PacketType packetType
        {
            get
            {
                return PacketType.PlayerHpSynchronization;
            }
        }
        [Index(0)] public virtual string Player { get; set; }
        [Index(1)] public virtual short VariationHP { get; set; }
    }
    [ZeroFormattable]
    public class MobHpSynchronization : Packet
    {
        public override PacketType packetType
        {
            get
            {
                return PacketType.MobHpSynchronization;
            }
        }
        [Index(0)] public virtual sbyte MobID { get; set; }
        [Index(1)] public virtual short VariationHP { get; set; }
        [Index(2)] public virtual bool MapCheck { get; set; }
    }
    [ZeroFormattable]
    public class ReSpawnMobs : Packet
    {
        public override PacketType packetType
        {
            get
            {
                return PacketType.ReSpawnMobs;
            }
        }
    }


}
