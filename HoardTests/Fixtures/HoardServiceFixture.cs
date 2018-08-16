using Hoard;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Xunit;

namespace HoardTests.Fixtures
{
    public class HoardServiceFixture : IDisposable
    {
        static string EnvironmentCurrentDirectory = Environment.CurrentDirectory;
        static string ContractsDirectory = EnvironmentCurrentDirectory + "\\..\\..\\..\\..\\Contracts\\";
        private Process cmd = new Process();

        public HoardService HoardService { get; private set; }

        public Dictionary<string, string> DeployedAddresses = new Dictionary<string, string>();
        public Dictionary<string, string> DeployedABIs = new Dictionary<string, string>();
        public List<string> Accounts = new List<string> {
            "0x90f8bf6a479f320ead074411a4b0e7944ea8c9c1",
            "0xffcf8fdee72ac11b5c542428b35eef5769c409f0",
            "0x22d491bde2303f2f43325b2108d26f1eaba1e32b",
            "0xe11ba2b4d45eaed5996cd0823791e0c93114882d",
            "0xd03ea8624c8c5987235048901fb614fdca89b117",
            "0x95ced938f7991cd0dfcb48f0a06a40fa1af46ebc",
            "0x3e5e9111ae8eb78fe1cc3bb8915d5d461f3ef9a9",
            "0x28a8746e75304c0780e011bed21c72cd78cd535e",
            "0xaca94ef8bd5ffee41947b4585a84bda5a3d3da6e",
            "0x1df62f291b2e969fb0849d99d9ce41e2f137006e"
        };

        public HoardServiceFixture()
        {
            StartLocalNetwork();
            RunMigrations();

            HoardServiceOptions options = new HoardServiceOptions();
            options.GameCenterContract = DeployedAddresses["HoardGames"];

            options.RpcClient = new Nethereum.JsonRpc.Client.RpcClient(new Uri(@"http://127.0.0.1:8545"));

            HoardService = HoardService.Instance;

            Assert.True(HoardService.Initialize(options), "ERROR: Could not initialize HOARD!");

            Hoard.PlayerID myId = HoardService.DefaultPlayer;
        }

        public void Dispose()
        {
            cmd.Close();
        }

        private void StartLocalNetwork()
        {
            cmd.StartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c ganache-cli -d",
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            cmd.Start();

            // FIXME: handle errors?
            while (!cmd.StandardOutput.EndOfStream)
            {
                string line = cmd.StandardOutput.ReadLine();
                if (line.Equals("Listening on 127.0.0.1:8545"))
                {
                    break;
                }
            }
        }

        private void RunMigrations()
        {
            Directory.SetCurrentDirectory(ContractsDirectory);

            cmd.StartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c truffle.cmd migrate --reset",
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            cmd.Start();
            cmd.WaitForExit();

            // FIXME: handle errors

            string[] fileNames = Directory.GetFiles("migrations\\", "*DeployedAddress.txt");
            foreach(string fileName in fileNames)
            {
                string name = fileName.Replace("DeployedAddress.txt", "");
                name = name.Replace("migrations\\", "");

                DeployedAddresses.Add(name, System.IO.File.ReadAllText(fileName));

                var contractBuild = JObject.Parse(File.ReadAllText("build\\contracts\\" + name + ".json"));
                DeployedABIs.Add(name, contractBuild.GetValue("abi").ToString());
            }

            Directory.SetCurrentDirectory(EnvironmentCurrentDirectory);
        }
    }
}
