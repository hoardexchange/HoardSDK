using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hoard.BC.Contracts
{
    /// <summary>
    /// Contract determining what other interfaces are supported by this contract.
    /// All Hoard Game Item contracts must support it.
    /// </summary>
    internal class ContractInterfaceID
    {
        /// <summary>
        /// InterfaceID stored in 4 bytes
        /// </summary>
        public byte[] InterfaceID { get; }

        /// <summary>
        /// Contract type connected with interface ID
        /// </summary>
        public Type ContractType { get; }

        /// <summary>
        /// Creates a new instance of contract
        /// </summary>
        /// <param name="interfaceID">interfaceID 4 bytes represented in hex</param>
        /// <param name="contractType">target type of the the contract that implements given interfaceID</param>
        public ContractInterfaceID(string interfaceID, Type contractType)
        {
            InterfaceID = BitConverter.GetBytes(Convert.ToUInt32(interfaceID, 16));

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(InterfaceID);
            }

            ContractType = contractType;
        }
    }

}
