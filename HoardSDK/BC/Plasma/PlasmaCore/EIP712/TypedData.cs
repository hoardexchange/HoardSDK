using Newtonsoft.Json;
using System;

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
        // FIXME? chainId missing - plasma not using it

        /// <summary>
        /// The user readable name of signing domain
        /// </summary>
        [TypedData("name", "string")]
        public string Name { get; set; }

        /// <summary>
        /// The current major version of the signing domain
        /// </summary>
        [TypedData("version", "string")]
        public string Version { get; set; }

        /// <summary>
        /// The address of the contract that will verify the signature
        /// </summary>
        [TypedData("verifyingContract", "address")]
        public string VerifyingContract { get; set; }

        /// <summary>
        /// An disambiguating salt for the protocol
        /// </summary>
        [TypedData("salt", "bytes32")]
        public byte[] Salt { get; set; }

        /// <summary>
        /// Constructs EIP-712 domain separator
        /// </summary>
        /// <param name="name">user readable name of signing domain</param>
        /// <param name="version">current major version of the signing domain</param>
        /// <param name="verifyingContract">the address of the contract that will verify the signature</param>
        /// <param name="salt">an disambiguating salt for the protocol.</param>
        public EIP712Domain(string name, string version, string verifyingContract, byte[] salt)
        {
            Name = name;
            Version = version;
            VerifyingContract = verifyingContract;
            Salt = salt;
        }
    }
}
