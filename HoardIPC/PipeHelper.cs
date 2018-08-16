using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace HoardIPC
{
    public class PipeHelper
    {
        static public int MessageChunkSize = 256;
        static public int HeaderChunkSize = 32;

        public static byte[] ToBytes(Object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
    }
}
