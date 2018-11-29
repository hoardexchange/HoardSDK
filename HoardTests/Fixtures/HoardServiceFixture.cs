using Hoard;
using IniParser;
using IniParser.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace HoardTests.Fixtures
{
    public class HoardServiceFixture : IDisposable
    {
        public class UserInputProviderFixture : IUserInputProvider
        {
            public async Task<string> RequestInput(User user, eUserInputType type, string description)
            {
                if (type == eUserInputType.kLogin)
                    return "TestUser";
                else if (type == eUserInputType.kPassword)
                    return "dev";

                return null;
            }
        }
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
            options.UserInputProvider = new UserInputProviderFixture();

            HoardService = HoardService.Instance;

            Assert.True(HoardService.Initialize(options), "ERROR: Could not initialize HOARD!");

            //authenticate user
            UserIDs.Add(HoardService.Instance.LoginPlayer().Result);

            HoardService.DefaultUser = UserIDs[0];
        }

        public void Initialize(string testName = null)
        {
            Deploy(testName);

            HoardServiceOptions options = new HoardServiceOptions();
            options.UserInputProvider = new UserInputProviderFixture();
    
            FileIniDataParser parser = new FileIniDataParser();
            IniData data = parser.ReadFile("settings.ini");

            options.GameCenterContract = data["hoard_game_center"]["contract_addr"];
            Assert.NotEmpty(options.GameCenterContract);

            options.RpcClient = new Nethereum.JsonRpc.Client.RpcClient(new Uri(string.Format("http://{0}:{1}", data["network"]["host"], data["network"]["port"])));
            
            HoardService = HoardService.Instance;
            
            Assert.True(HoardService.Initialize(options), "ERROR: Could not initialize HOARD!");

            //authenticate user
            UserIDs.Add(HoardService.Instance.LoginPlayer().Result);

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
