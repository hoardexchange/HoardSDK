using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Hoard
{
    public class DataStorageUtils
    {
        static public byte[] LoadFromDisk(string path)
        {
            byte[] data = null;
            if (File.Exists(path))
            {
                using (FileStream stream = File.Open(path, FileMode.Open))
                {
                    data = new byte[stream.Length];
                    stream.Read(data, 0, (int)stream.Length);
                }
            }
            return data;
        }

        static public void SaveToDisk(string path, byte[] data)
        {
            string dir = Path.GetDirectoryName(path);
            System.IO.Directory.CreateDirectory(dir);
            using (FileStream stream = File.Open(path, FileMode.Create))
            {
                stream.Write(data, 0, (int)data.Length);
            }
        }

        static public GameDataInfo Deserialize(byte[] data)
        {
            string serialized = Encoding.ASCII.GetString(data);
            GameDataInfo gameDataInfo = JsonConvert.DeserializeObject<GameDataInfo>(serialized);
            return gameDataInfo;
        }

        static public byte[] Serialize(GameDataInfo dataInfo)
        {
            string serialized = JsonConvert.SerializeObject(dataInfo);
            return Encoding.ASCII.GetBytes(serialized);
        }
    }
}
