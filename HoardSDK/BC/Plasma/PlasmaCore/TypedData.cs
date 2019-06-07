using Newtonsoft.Json;
using System;
using System.Numerics;

namespace PlasmaCore.EIP712
{
    /// <summary>
    /// Typed data attribute
    /// </summary>
    public class TypedDataAttribute : Attribute
    {
        /// <summary>
        /// Name attribute
        /// </summary>
        [JsonProperty(propertyName: "name")]
        public string Name { get; }

        /// <summary>
        /// Type attribute
        /// </summary>
        [JsonProperty(propertyName: "type")]
        public string Type { get; }

        /// <summary>
        /// Constructs typed data attribute with given name and type
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        public TypedDataAttribute(string name, string type)
        {
            Name = name;
            Type = type;
        }
    }

    /// <summary>
    /// Typed struct attribute
    /// </summary>
    public class TypedStructAttribute : Attribute
    {
        /// <summary>
        /// Name attribute
        /// </summary>
        [JsonProperty(propertyName: "name")]
        public string Name { get; }

        /// <summary>
        /// Constructs typed struct attribute with given name
        /// </summary>
        /// <param name="name"></param>
        public TypedStructAttribute(string name)
        {
            Name = name;
        }
    }

    /// <summary>
    /// EIP-712 domain separator definition
    /// </summary>
    [TypedStruct("EIP712Domain")]
    public class EIP712Domain 
    {
        /// <summary>
        /// Signing domain name
        /// </summary>
        [TypedData("name", "string")]
        public string Name { get; set; }

        /// <summary>
        /// Signing domain version
        /// </summary>
        [TypedData("version", "string")]
        public string Version { get; set; }
        
        /// <summary>
        /// Signature verifying contract
        /// </summary>
        [TypedData("verifyingContract", "address")]
        public string VerifyingContract { get; set; }

        /// <summary>
        /// Salt for the protocol
        /// </summary>
        [TypedData("salt", "bytes32")]
        public byte[] Salt { get; set; }

        /// <summary>
        /// Constructs EIP-712 domain separator
        /// </summary>
        /// <param name="name"></param>
        /// <param name="version"></param>
        /// <param name="verifyingContract"></param>
        /// <param name="salt"></param>
        public EIP712Domain(string name, string version, string verifyingContract, byte[] salt)
        {
            Name = name;
            Version = version;
            VerifyingContract = verifyingContract;
            Salt = salt;
        }
    }
}
