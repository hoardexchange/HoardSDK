using System;
using System.IO;

namespace HoardIPC
{
    public enum Message
    {
        MSG_UNKNOWN = 0,
        MSG_REGISTER_GAME,
        MSG_UNREGISTER_GAME,
        MSG_HEARTBEAT,
    }

    public class PipeHeader
    {
        public uint headerId = 0x88888887;
        public int msgSize = 0;
        public int version = 100;
        public Message msgId;

        public byte[] Serialize()
        {
            using (MemoryStream m = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(m))
                {
                    writer.Write(headerId);
                    writer.Write(msgSize);
                    writer.Write(version);
                    writer.Write((int)msgId);
                }
                return m.ToArray();
            }
        }

        public void Desserialize(byte[] data)
        {
            using (MemoryStream m = new MemoryStream(data))
            {
                using (BinaryReader reader = new BinaryReader(m))
                {
                    headerId = reader.ReadUInt32();
                    msgSize = reader.ReadInt32();
                    version = reader.ReadInt32();
                    msgId = (Message)reader.ReadInt32();
                }
            }
        }
    }

    public class PipeMessage
    {
        protected Message id = Message.MSG_UNKNOWN;
    
        public Message GetId() { return id; }

        virtual public byte[] Serialize()
        {
            return null;
        }

        virtual public void Desserialize(byte[] data)
        {
        }
    }

    public class MsgRegisterGame : PipeMessage
    {
        public uint gameId;

        public MsgRegisterGame() { id = Message.MSG_REGISTER_GAME; }

        override public byte[] Serialize()
        {
            using (MemoryStream m = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(m))
                {
                    writer.Write(gameId);
                }
                return m.ToArray();
            }
        }

        override public void Desserialize(byte[] data)
        {
            using (MemoryStream m = new MemoryStream(data))
            {
                using (BinaryReader reader = new BinaryReader(m))
                {
                    gameId = reader.ReadUInt32();
                }
            }
        }
    }

    public class MsgUnregisterGame : MsgRegisterGame
    {
        public MsgUnregisterGame() { id = Message.MSG_UNREGISTER_GAME; }
    }

    public class MsgHeartbeat : MsgRegisterGame
    {
        public MsgHeartbeat() { id = Message.MSG_HEARTBEAT; }
    }
}
