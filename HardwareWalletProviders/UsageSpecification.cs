namespace Hoard.HW
{
    internal class UsageSpecification
    {
        public ushort Usage { get; }
        public ushort UsagePage { get; }

        public UsageSpecification(ushort usagePage, ushort usage)
        {
            UsagePage = usagePage;
            Usage = usage;
        }
    }
}
