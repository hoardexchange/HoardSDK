using Hoard;
using IniParser;
using IniParser.Model;
using Nethereum.Web3.Accounts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Xunit;

namespace HoardTests.Fixtures
{
    public class HoardServiceFixture : IDisposable
    {
        public static string EnvironmentCurrentDirectory = Environment.CurrentDirectory;
        public static string ScriptsDirectory = EnvironmentCurrentDirectory + "\\..\\..\\..\\..\\HoardTools\\Scripts";

        public HoardService HoardService { get; private set; }

        public static List<User> UserIDs = new List<User>();
        private Process cmd = new Process();

        public HoardServiceFixture()
        {
        }

        public void Dispose()
        {
            if (HoardService != null)
            {
                HoardService.Shutdown();
            }

            cmd.Close();
        }

        public void InitializeFromConfig(string configPath = null)
        {
            HoardServiceConfig config = HoardServiceConfig.Load(configPath);
            HoardServiceOptions options = new HoardServiceOptions(config, new Nethereum.JsonRpc.Client.RpcClient(new Uri(config.ClientUrl)));

            HoardService = HoardService.Instance;

            Assert.True(HoardService.Initialize(options), "ERROR: Could not initialize HOARD!");

            UserIDs.AddRange(HoardService.Users);
            HoardService.DefaultUser = UserIDs[0];
        }

        public void Initialize(string testName = null)
        {
            Deploy(testName);

            HoardServiceOptions options = new HoardServiceOptions();
    
            FileIniDataParser parser = new FileIniDataParser();
            IniData data = parser.ReadFile("settings.ini");

            options.GameCenterContract = data["hoard_game_center"]["contract_addr"];
            Assert.NotEmpty(options.GameCenterContract);

            options.RpcClient = new Nethereum.JsonRpc.Client.RpcClient(new Uri(string.Format("http://{0}:{1}", data["network"]["host"], data["network"]["port"])));

            User newUser = new User("TestUser", "dev");
            foreach (KeyData account in data["accounts"])
            {
                Account acc = new Account(account.Value);
                UserIDs.Add(newUser);
                HoardService.LoadAccountInplace(newUser, acc.Address, acc.PrivateKey, "dev", User.ServiceType.KeyContainer);
            }

            HoardService = HoardService.Instance;
            
            Assert.True(HoardService.Initialize(options), "ERROR: Could not initialize HOARD!");

            HoardService.DefaultUser = UserIDs[0];
        }

        private void Deploy(string testName = null)
        {
            Directory.SetCurrentDirectory(ScriptsDirectory);

            cmd.StartInfo = new ProcessStartInfo
            {
                FileName = "cmd",
                Arguments = "/c python deploy_hoard_contracts.py",
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            if (testName != null)
            {
                cmd.StartInfo.Arguments = "/c python deploy_hoard_contracts.py --test " + testName;
            }

            cmd.Start();
            cmd.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);

            cmd.BeginOutputReadLine();
            cmd.WaitForExit();

            Directory.SetCurrentDirectory(EnvironmentCurrentDirectory);

            File.Copy(ScriptsDirectory + "\\settings.ini", EnvironmentCurrentDirectory + "\\settings.ini", true);
        }

        private static void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            // Collect the sort command output.
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                Trace.WriteLine(outLine.Data);
            }
        }
    }
}
