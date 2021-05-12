
using System.Collections.Generic;
using ZeroFormatter;

namespace HServerPacket
{
    public enum PacketType
    {
        InitPlayer, LoginSuccess, ObjectSynchronization, AttackPlayer, EnableSpace, InitPlayers, WarpMap
    }

    [Union(typeof(ObjectSynchronization), typeof(AttackPlayer), typeof(EnableSpace), typeof(LoginSuccess), typeof(InitPlayer), typeof(InitPlayers),
        typeof(WarpMap))]
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
        [Index(4)] public virtual float VelocityX { get; set; }
        [Index(5)] public virtual float VelocityY { get; set; }
        [Index(6)] public virtual bool FlipX { get; set; }
        [Index(7)] public virtual bool MapCheck { get; set; }
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
        [Index(0)] public virtual List<string> Players { get; set; }
        [Index(1)] public virtual List<List<int>> Equips { get; set; }
        [Index(2)] public virtual List<float> PositionX { get; set; }
        [Index(3)] public virtual List<float> PositionY { get; set; }
        [Index(4)] public virtual List<float> VelocityX { get; set; }
        [Index(5)] public virtual List<float> VelocityY { get; set; }
        [Index(6)] public virtual List<bool> FlipX { get; set; }
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
        [Index(1)] public virtual int LinkedMap { get; set; }
        [Index(2)] public virtual float PosX { get; set; }
        [Index(3)] public virtual float PosY { get; set; }
    }
}
