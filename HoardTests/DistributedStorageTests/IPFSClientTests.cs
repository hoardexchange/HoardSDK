using Hoard.DistributedStorage;
using HoardTests.Fixtures;
using System;
using System.Text;
using Xunit;

namespace HoardXTests.DistributedStorageTests
{
    public class IPFSClientTests : IClassFixture<IPFSFixture>
    {
        IPFSFixture fixture;
        IPFSClient client;

        private readonly Random random = new Random();

        public IPFSClientTests(IPFSFixture fixture)
        {
            this.fixture = fixture;
            this.client = fixture.Client;
        }

        [Fact]
        public void UploadDownloadTest()
        {
            string data = "Hello world!";
            string addressHex = "2755b55ef72f04f967504db835f1507481d0e02354e4e0b185137770332d7d7f"; // Base58 QmQzCQn4puG4qu8PVysxZmscmQ5vT1ZXpqo7f58Uh9QfyY

            byte[] newAddress = client.UploadAsync(Encoding.ASCII.GetBytes(data)).Result;
            string newAddresHex = BitConverter.ToString(newAddress).Replace("-", string.Empty);
            Assert.Equal(newAddresHex.ToLower(), addressHex.ToLower());

            byte[] dataBytes = client.DownloadBytesAsync(newAddress).Result;
            string dataString = Encoding.ASCII.GetString(dataBytes);

            Assert.Equal(data, dataString);
        }
    }
}
