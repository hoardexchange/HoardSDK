using Hid.Net;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Hoard.HW
{
    public static class Helpers
    {
        public static async Task<IHidDevice> GetHIDDeviceAsync(DeviceInfo[] deviceInfo, UsageSpecification[] usageSpecification)
        {
            var devices = new List<DeviceInformation>();

            var collection = WindowsHidDevice.GetConnectedDeviceInformations();

            foreach (var ids in deviceInfo)
            {
                devices.AddRange(collection.Where(c => c.VendorId == ids.VendorId && c.ProductId == ids.ProductId));
            }

            var deviceFound = devices.FirstOrDefault(d =>
                    usageSpecification == null ||
                    usageSpecification.Length == 0 ||
                    usageSpecification.Any(u => d.UsagePage == u.UsagePage && d.Usage == u.Usage));

            if (deviceFound != null)
            {
                var hidDevice = new WindowsHidDevice(deviceFound);
                await hidDevice.InitializeAsync();
                return hidDevice;
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
