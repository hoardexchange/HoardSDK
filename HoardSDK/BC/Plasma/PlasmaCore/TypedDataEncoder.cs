using Hoard.Utils;
using Nethereum.ABI;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Util;
using PlasmaCore.EIP712;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Plasma.PlasmaCore
{
    public class TypedDataEncoder
    {
        private static readonly byte[] HEADER = new byte[] { 0x19, 0x01 };

        public static byte[] Encode<T>(T data, EIP712Domain domain)
        {
            byte[] domainSeparator = HashStruct(domain);
            byte[] hashStruct = HashStruct(data);
            return ByteUtil.Merge(HEADER, domainSeparator, hashStruct);
        }

        public static byte[] HashStruct<T>(T data)
        {
            byte[] typeHash = CalculateTypeHash(data);
            byte[] encodedData = EncodeData(data);
            byte[] mergedData = ByteUtil.Merge(typeHash, encodedData);
            return new Sha3Keccack().CalculateHash(mergedData);
        }

        public static byte[] EncodeData<T>(T data)
        {
            byte[] encodedData = new byte[0];
            byte[] encodedDataPart = null;

            Type dataType = data.GetType();
            foreach (PropertyInfo propInfo in dataType.GetProperties())
            {
                TypedDataAttribute typedAttr = propInfo.GetCustomAttribute<TypedDataAttribute>();
                object value = propInfo.GetValue(data);

                if (!IsReferenceType(typedAttr.Type))
                {
                    if(typedAttr.Type == "bytes")
                    {
                        encodedDataPart = new Sha3Keccack().CalculateHash((byte[])value);
                    }
                    else if (typedAttr.Type == "string")
                    {
                        encodedDataPart = new Sha3Keccack().CalculateHash((string)value).HexToByteArray();
                    }
                    else
                        encodedDataPart = ABIType.CreateABIType(typedAttr.Type).Encode(value);
                }
                else
                {
                    encodedDataPart = HashStruct(value);
                }
                encodedData = ByteUtil.Merge(encodedData, encodedDataPart);
            }
            return encodedData;
        }

        public static byte[] CalculateTypeHash<T>(T data)
        {
            string encodedType = EncodeType(data);
            Dictionary<string, string> dependencies = new Dictionary<string, string>();
            FindDependencies(data, ref dependencies);
            foreach(var dep in dependencies.OrderBy(x => x.Key))
            {
                encodedType += dep.Value;
            }

            return new Sha3Keccack().CalculateHash(encodedType).HexToByteArray();
        }

        public static string EncodeType<T>(T data)
        {
            Type dataType = data.GetType();
            List<string> types = new List<string>();
            foreach (PropertyInfo propInfo in dataType.GetProperties())
            {
                TypedDataAttribute typedAttr = propInfo.GetCustomAttribute<TypedDataAttribute>();
                types.Add(string.Format("{0} {1}", typedAttr.Type, typedAttr.Name));
            }

            TypedStructAttribute typeAttr = dataType.GetTypeInfo().GetCustomAttribute<TypedStructAttribute>();
            return string.Format("{0}({1})", typeAttr.Name, string.Join(",", types));
        }

        public static void FindDependencies<T>(T data, ref Dictionary<string, string> dependencies)
        {
            Type dataType = data.GetType();
            foreach (PropertyInfo propInfo in dataType.GetProperties())
            {
                TypedDataAttribute typedAttr = propInfo.GetCustomAttribute<TypedDataAttribute>();
                if(IsReferenceType(typedAttr.Type) && !dependencies.ContainsKey(typedAttr.Type))
                {
                    object depObj = Activator.CreateInstance(Type.GetType("PlasmaCore.EIP712." + typedAttr.Type));
                    dependencies[typedAttr.Type] = EncodeType(depObj);
                    FindDependencies(depObj, ref dependencies);
                }
            }
        }


        // FIXME not all types are implemented / listed
        private static readonly string[] supportedTypes = new string[]
        {
            "address",
            "bytes",
            "string",
            "bool"
        };

        public static bool IsReferenceType(string type)
        {
            if (supportedTypes.Contains(type))
                return false;

            if (type.StartsWith("bytes"))
            {
                int size = 0;
                int.TryParse(type.Substring(5), out size);
                if (size > 0 && size <= 32)
                    return false;
                throw new ArgumentOutOfRangeException("Invalid type - only bytes1 to bytes32 supported");
            }

            if (type.StartsWith("uint"))
            {
                int size = 0;
                int.TryParse(type.Substring(4), out size);
                if (size >= 8 && size <= 256 && size % 8 == 0)
                    return false;
                throw new ArgumentOutOfRangeException("Invalid type - only uint8 to uint256 supported");
            }


            if (type.StartsWith("int"))
            {
                int size = 0;
                int.TryParse(type.Substring(3), out size);
                if (size >= 8 && size <= 256 && size % 8 == 0)
                    return false;
                throw new ArgumentOutOfRangeException("Invalid type - only int8 to int256 supported");
            }

            return true;
        }
    }
}
