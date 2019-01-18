namespace Hoard.HW
{
    internal class DeviceInfo
    {
        public int VendorId { get; }
        public int ProductId { get; }
        public string Name { get; }

        public DeviceInfo(int vendorId, int productId, string name)
        {
            VendorId = vendorId;
            ProductId = productId;
            Name = name;
        }
    }
}
