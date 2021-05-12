using System;
using System.Collections.Generic;
using System.Text;
using ZeroFormatter;

namespace HClientPacket
{
    public enum PacketType
    {
        ObjectSynchronization, AttackPlayer, EnableSpace, WarpMap
    }
    [Union(typeof(ObjectSynchronization), typeof(AttackPlayer), typeof(EnableSpace), typeof(WarpMap))]
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
        [Index(0)] public virtual float PositionX { get; set; }
        [Index(1)] public virtual float PositionY { get; set; }
        [Index(2)] public virtual float VelocityX { get; set; }
        [Index(3)] public virtual float VelocityY { get; set; }
        [Index(4)] public virtual float Rotation { get; set; }
        [Index(5)] public virtual float AngularVelocity { get; set; }
        [Index(6)] public virtual bool FlipX { get; set; }
        [Index(7)] public virtual bool MapCheck { get; set; }
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
        [Index(0)] public virtual bool IsEnabled { get; set; }
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
    }
}
