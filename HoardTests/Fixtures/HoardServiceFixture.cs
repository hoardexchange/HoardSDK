using Hoard;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Xunit;
using Nethereum.Web3.Accounts;

namespace HoardTests.Fixtures
{
    public class HoardServiceFixture : IDisposable
    {
        public static string EnvironmentCurrentDirectory = Environment.CurrentDirectory;
        public static string ContractsDirectory = EnvironmentCurrentDirectory + "\\..\\..\\..\\..\\hoardcontracts\\";
        private Process cmd = new Process();

        public HoardService HoardService { get; private set; }

        public static Dictionary<string, string> DeployedAddresses = new Dictionary<string, string>();
        public static List<Account> Accounts = new List<Account> {
            new Account("4f3edf983ac636a65a842ce7c78d9aa706d3b113bce9c46f30d7d21715b23b1d"),
            new Account("6cbed15c793ce57650b9877cf6fa156fbef513c4e6134f022a85b1ffdd59b2a1"),
            new Account("6370fd033278c143179d81c5526140625662b8daa446c22ee2d73db3707e620c"),
            new Account("646f1ce2fdad0e6deeeb5c7e8e5543bdde65e86029e2fd9fc169899c440a7913"),
            new Account("add53f9a7e588d003326d1cbf9e4a43c061aadd9bc938c843a79e7b4fd2ad743"),
            new Account("395df67f0c2d2d9fe1ad08d1bc8b6627011959b79c53d7dd6a3536a33ab8a4fd"),
            new Account("e485d098507f54e7733a205420dfddbe58db035fa577fc294ebd14db90767a52"),
            new Account("a453611d9419d0e56f499079478fd72c37b251a94bfde4d19872c44cf65386e3"),
            new Account("829e924fdf021ba3dbbc4225edfece9aca04b929d6e75613329ca6f1d31c0bb4"),
            new Account("b0057716d5917badaf911b193b12b910811c1497b5bada8d7711f758981c3773")
        };

        public PlayerID HoardUser = new PlayerID(Accounts[0].Address, Accounts[0].PrivateKey, "");
        public PlayerID GameOwnerUser = new PlayerID(Accounts[1].Address, Accounts[1].PrivateKey, "");
        public PlayerID PlayerUser = new PlayerID(Accounts[2].Address, Accounts[2].PrivateKey, "");

        public HoardServiceFixture()
        {
            StartLocalNetwork();
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

        public void Initialize(string[] testPaths)
        {
            Directory.SetCurrentDirectory(ContractsDirectory);

            foreach (string testPath in testPaths)
            {
                cmd.StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/c truffle.cmd test .\\test\\" + testPath,
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                };
                cmd.Start();
                
            }

            HoardServiceOptions options = new HoardServiceOptions();

            // FIXME: handle errors?

            string pattern = @"\w* deployed at: (0x[a-fA-F0-9]{40})";
            while (!cmd.StandardOutput.EndOfStream)
            {
                string line = cmd.StandardOutput.ReadLine();
                Match match = Regex.Match(line, pattern, RegexOptions.None);
                if (match.Success && options.GameCenterContract.Length == 0)
                {
                    options.GameCenterContract = match.Groups[1].Value;
                }
            }

            cmd.WaitForExit();

            Assert.NotEmpty(options.GameCenterContract);

            Directory.SetCurrentDirectory(EnvironmentCurrentDirectory);

            options.RpcClient = new Nethereum.JsonRpc.Client.RpcClient(new Uri(@"http://127.0.0.1:8545"));

            HoardService = HoardService.Instance;

            Assert.True(HoardService.Initialize(options), "ERROR: Could not initialize HOARD!");

            HoardService.DefaultPlayer = HoardUser;
        }
    }
}
