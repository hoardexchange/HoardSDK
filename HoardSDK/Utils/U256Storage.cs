using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Text;

namespace Hoard.Utils
{
    /// <summary>
    /// Available storage types
    /// </summary>
    public enum U256StorageDataType
    {
        UInt16 = 0,
        UInt32,
        UInt64,
        Bool,
        Byte,

        MaxStorageSize = 256,
    }

    /// <summary>
    /// Item state storage description
    /// </summary>
    public struct U256StorageDescription
    {
        public U256StorageDataType type;
        public int size;
        public object value;
    }

    /// <summary>
    /// Item state storage packer
    /// </summary>
    public class U256StoragePacker
    {
        private int ActualShift;
        private List<U256StorageDescription> Variables = null;

        /// <summary>
        /// Item state
        /// </summary>
        public BigInteger State { get; private set; }

        /// <summary>
        /// Item state storage constructor
        /// </summary>
        public U256StoragePacker()
        {
            State = BigInteger.Zero;
            ActualShift = 0;
            Variables = new List<U256StorageDescription>();
        }

        /// <summary>
        /// Return available bits in storage
        /// </summary>
        /// <returns></returns>
        public int GetBitsToPack()
        {
            return (int)U256StorageDataType.MaxStorageSize - ActualShift;
        }

        /// <summary>
        /// Pack value to storage
        /// </summary>
        /// <param name="value"> value </param>
        public void PackUInt8(byte value)
        {
            State |= new BigInteger(value) << ActualShift;
            ActualShift += 8;
            Debug.Assert(ActualShift <= (int)U256StorageDataType.MaxStorageSize);

            U256StorageDescription desc = new U256StorageDescription();
            desc.type = U256StorageDataType.Byte;
            desc.size = 8;
            desc.value = value;
            Variables.Add(desc);
        }

        /// <summary>
        /// Pack value to storage
        /// </summary>
        /// <param name="value"> value </param>
        public void PackUInt16(UInt16 value)
        {
            State |= new BigInteger(value) << ActualShift;
            ActualShift += 16;
            Debug.Assert(ActualShift <= (int)U256StorageDataType.MaxStorageSize);

            U256StorageDescription desc = new U256StorageDescription();
            desc.type = U256StorageDataType.UInt16;
            desc.size = 16;
            desc.value = value;
            Variables.Add(desc);
        }

        /// <summary>
        /// Pack value to storage
        /// </summary>
        /// <param name="value"> value </param>
        public void PackUInt32(UInt32 value)
        {
            State |= new BigInteger(value) << ActualShift;
            ActualShift += 32;
            Debug.Assert(ActualShift <= (int)U256StorageDataType.MaxStorageSize);

            U256StorageDescription desc = new U256StorageDescription();
            desc.type = U256StorageDataType.UInt32;
            desc.size = 32;
            desc.value = value;
            Variables.Add(desc);
        }

        /// <summary>
        /// Pack value to storage
        /// </summary>
        /// <param name="value"> value </param>
        public void PackUInt64(UInt64 value)
        {
            State |= new BigInteger(value) << ActualShift;
            ActualShift += 64;
            Debug.Assert(ActualShift <= (int)U256StorageDataType.MaxStorageSize);

            U256StorageDescription desc = new U256StorageDescription();
            desc.type = U256StorageDataType.UInt64;
            desc.size = 64;
            desc.value = value;
            Variables.Add(desc);
        }

        /// <summary>
        /// Pack value to storage
        /// </summary>
        /// <param name="value"> value </param>
        public void PackBool(bool value)
        {
            State |= new BigInteger((byte)(value == true ? 0xff : 0x00)) << ActualShift;
            ActualShift += 8;
            Debug.Assert(ActualShift <= (int)U256StorageDataType.MaxStorageSize);

            U256StorageDescription desc = new U256StorageDescription();
            desc.type = U256StorageDataType.Bool;
            desc.size = 8;
            desc.value = value;
            Variables.Add(desc);
        }

        /// <summary>
        /// Exports state to specified file
        /// </summary>
        /// <param name="fileName"> destination file name </param>
        public bool ExportToFile(string fileName)
        {
            try
            {
                var json = JsonConvert.SerializeObject(Variables);
                System.IO.File.WriteAllText(fileName, json);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Item state storage unpacker
    /// </summary>
    public class U256StorageUnpacker
    {
        private int ActualShift;

        /// <summary>
        /// Item state
        /// </summary>
        public BigInteger State { get; private set; }

        /// <summary>
        /// Item state storage constructor
        /// </summary>
        /// <param state="state"> the state we want to modify </param>
        public U256StorageUnpacker(BigInteger state)
        {
            State = state;
            ActualShift = 0;
        }

        /// <summary>
        /// Unpack value from storage
        /// </summary>
        /// <returns></returns>
        public byte UnpackUInt8()
        {
            byte value = (byte)((State >> ActualShift) & 0xff);
            ActualShift += 8;
            Debug.Assert(ActualShift <= (int)U256StorageDataType.MaxStorageSize);
            return value;
        }

        /// <summary>
        /// Unpack value from storage
        /// </summary>
        /// <returns></returns>
        public UInt16 UnpackUInt16()
        {
            UInt16 value = (UInt16)((State >> ActualShift) & 0xffff);
            ActualShift += 16;
            Debug.Assert(ActualShift <= (int)U256StorageDataType.MaxStorageSize);
            return value;
        }

        /// <summary>
        /// Unpack value from storage
        /// </summary>
        /// <returns></returns>
        public UInt32 UnpackUInt32()
        {
            UInt32 value = (UInt32)((State >> ActualShift) & 0xffffffff);
            ActualShift += 32;
            Debug.Assert(ActualShift <= (int)U256StorageDataType.MaxStorageSize);
            return value;
        }

        /// <summary>
        /// Unpack value from storage
        /// </summary>
        /// <returns></returns>
        public UInt64 UnpackUInt64()
        {
            UInt64 value = (UInt64)((State >> ActualShift) & 0xffffffffffffffff);
            ActualShift += 64;
            Debug.Assert(ActualShift <= (int)U256StorageDataType.MaxStorageSize);
            return value;
        }

        /// <summary>
        /// Unpack value from storage
        /// </summary>
        /// <returns></returns>
        public bool UnpackBool()
        {
            return UnpackUInt8() == 0xff ? true : false;
        }

        /// <summary>
        /// Import state from json file
        /// </summary>
        /// <param name="fileName"> file name to import item state </param>
        /// <param name="state"> imported item state </param>
        static public bool ImportFromFile(string fileName, ref BigInteger state)
        {
            try
            {
                string data = System.IO.File.ReadAllText(fileName);
                List<U256StorageDescription> variables = JsonConvert.DeserializeObject<List<U256StorageDescription>>(data);
                int shift = 0;
                foreach (U256StorageDescription desc in variables)
                {
                    switch (desc.size)
                    {
                        case 8:
                            if (desc.type == U256StorageDataType.Bool)
                                state |= new BigInteger(Convert.ToByte((byte)((bool)desc.value == true ? 0xff : 0x00))) << shift;
                            else
                                state |= new BigInteger(Convert.ToByte(desc.value)) << shift;
                            break;
                        case 16:
                            state |= new BigInteger(Convert.ToUInt16(desc.value)) << shift;
                            break;
                        case 32:
                            state |= new BigInteger(Convert.ToUInt32(desc.value)) << shift;
                            break;
                        case 64:
                            state |= (BigInteger)desc.value << shift;
                            break;
                        default:
                            Debug.Assert(false);
                            return false;
                    }
                    shift += (int)desc.size;
                    Debug.Assert(shift <= (int)U256StorageDataType.MaxStorageSize);
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
