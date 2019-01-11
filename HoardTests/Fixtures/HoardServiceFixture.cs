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
                else if (type == eUserInputType.kEmail)
                    return "test.user@not.existing.domain";

                return null;
            }
        }
        public static string EnvironmentCurrentDirectory = Environment.CurrentDirectory;
        public static string ScriptsDirectory = EnvironmentCurrentDirectory + "\\..\\..\\..\\..\\HoardTools\\Scripts";

        public HoardService HoardService { get; private set; }

        public List<User> UserIDs = new List<User>();
        private Process cmd = new Process();

        public HoardServiceFixture()
        {
        }

        public virtual void Dispose()
        {
            if (HoardService != null)
            {
                HoardService.Shutdown();
            }

            cmd.Close();
        }

        public async Task<User> CreateUser()
        {
            //create user
            User user = new User("testPlayer");

            KeyStoreAccountService service = new KeyStoreAccountService(new UserInputProviderFixture());
            AccountInfo mainAcc = await service.CreateAccount("default", user);
            user.ChangeDefaultAccount(mainAcc);

            return user;
        }

        public void InitializeFromConfig(string configPath = null)
        {
            HoardServiceConfig config = HoardServiceConfig.Load(configPath);
            HoardServiceOptions options = new HoardServiceOptions(config, new Nethereum.JsonRpc.Client.RpcClient(new Uri(config.ClientUrl)));

            HoardService = HoardService.Instance;

            Assert.True(HoardService.Initialize(options), "ERROR: Could not initialize HOARD!");

            //authenticate user
            UserIDs.Add(CreateUser().Result);
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
            
            HoardService = HoardService.Instance;
            
            Assert.True(HoardService.Initialize(options), "ERROR: Could not initialize HOARD!");

            //authenticate user
            UserIDs.Add(CreateUser().Result);
        }

        private void Deploy(string testName = null)
        {
            Directory.SetCurrentDirectory(ScriptsDirectory);

            cmd.StartInfo = new ProcessStartInfo
            {
                FileName = "cmd",
                Arguments = "/c \"" + GetPythonInstallPath() + "\" deploy_hoard_contracts.py",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            if (testName != null)
            {
                cmd.StartInfo.Arguments += " --test " + testName;
            }

            cmd.Start();
            cmd.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
            cmd.ErrorDataReceived += new DataReceivedEventHandler(OutputHandler);

            cmd.BeginOutputReadLine();
            cmd.BeginErrorReadLine();
            cmd.WaitForExit();

            Directory.SetCurrentDirectory(EnvironmentCurrentDirectory);

            File.Copy(ScriptsDirectory + "\\settings.ini", EnvironmentCurrentDirectory + "\\settings.ini", true);
        }

        private static void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            // Collect the sort command output.
            if (!string.IsNullOrEmpty(outLine.Data))
            {
                Trace.WriteLine(outLine.Data);
            }
        }

        protected static string GetPythonInstallPath()
        {
            Microsoft.Win32.RegistryKey localmachineKey = Microsoft.Win32.RegistryKey.OpenBaseKey(
                Microsoft.Win32.RegistryHive.LocalMachine, Microsoft.Win32.RegistryView.Registry64);
            var pythonKey = localmachineKey.OpenSubKey(@"SOFTWARE\Python\PythonCore\3.6\InstallPath");
            if (pythonKey == null)
                return null;

            string filePath = (string)pythonKey.GetValue("ExecutablePath");
            return filePath;
        }
    }
}
