using Device.Net;
using ProtoBuf;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Hoard.HW
{
    internal static class Helpers
    {
        public static async Task<IDevice> GetHIDDeviceAsync(FilterDeviceDefinition[] deviceInfo, UsageSpecification[] usageSpecification)
        {
            Usb.Net.Windows.WindowsUsbDeviceFactory.Register();

            var devices = await DeviceManager.Current.GetDevicesAsync(deviceInfo);

            var deviceFound = devices.FirstOrDefault();

            if (deviceFound != null)
            {
                await deviceFound.InitializeAsync();
                return deviceFound;
            }

            return null;
        }

        internal static byte[] ProtoBufSerialize(object msg)
        {
            using (var writer = new MemoryStream())
            {
                Serializer.NonGeneric.Serialize(writer, msg);
                return writer.ToArray();
            }
        }

        internal static object ProtoBufDeserialize(Type type, byte[] data)
        {
            using (var writer = new MemoryStream(data))
            {
                return Serializer.NonGeneric.Deserialize(type, writer);
            }
        }

        internal static byte[] ReadBytes(this Stream stream, int count)
        {
            var data = new byte[count];
            int offset = 0;
            while (offset < count)
            {
                int read = stream.Read(data, offset, count - offset);
                if (read == 0) throw new System.IO.EndOfStreamException();
                offset += read;
            }
            return data;
        }

        internal static byte[] ToBytes(this uint value)
        {
            return new byte[]
            {
                (byte)(value >> 24),
                (byte)(value >> 16),
                (byte)(value >> 8),
                (byte)value,
            };
        }
    }
}
