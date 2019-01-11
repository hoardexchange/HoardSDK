using Hoard;
using Hoard.BC.Contracts;
using Hoard.ExchangeServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Threading.Tasks;
using Xunit;

namespace HoardTests.Fixtures
{
    public class HoardExchangeFixture : HoardServiceFixture
    {
        static string HoardGameTestName = "HoardExchange";

        public static string EnvironmentCurrentDirectory = Environment.CurrentDirectory;
        public static string ExchangeDirectory = EnvironmentCurrentDirectory + "\\..\\..\\..\\..\\HoardExchangeServer";

        public HoardServiceFixture HoardServiceFixture { get; private set; }
        public GameID[] GameIDs { get; private set; }
        public User[] Users { get; private set; }
        public List<GameItem> Items { get; private set; }

        public BCExchangeService BCExchangeService { get; private set; }
        public HoardExchangeService HoardExchangeService { get; private set; }

        private Process cmd = new Process();

        public HoardExchangeFixture()
        {
            base.Initialize(HoardGameTestName);

            Users = GetUsers();

            RunExchangeServer();

            BCExchangeService = new BCExchangeService(HoardService);
            BCExchangeService.Init();

            HoardExchangeService = new HoardExchangeService(HoardService);
            HoardExchangeService.Init();

            GameIDs = HoardService.QueryHoardGames().Result;
            foreach (var game in GameIDs)
            {
                HoardService.RegisterHoardGame(game);
            }

            Items = GetGameItems(Users[0]).Result;
            Assert.Equal(3, Items.Count);
            Assert.True(Items[0].Metadata is ERC223GameItemContract.Metadata);
            Assert.True(Items[1].Metadata is ERC223GameItemContract.Metadata);
            Assert.True(Items[2].Metadata is ERC721GameItemContract.Metadata);
        }

        public override void Dispose()
        {
            KillProcess(cmd.Id);
            base.Dispose();
        }

        private void RunExchangeServer()
        {
            Directory.SetCurrentDirectory(ExchangeDirectory);

            cmd.StartInfo = new ProcessStartInfo
            {
                FileName = "cmd",
                Arguments = "/c \""+ GetPythonInstallPath() + "\" manage.py runserver --hoardaddress " + HoardService.Options.GameCenterContract,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            cmd.Start();
            cmd.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
            cmd.ErrorDataReceived += new DataReceivedEventHandler(OutputHandler);

            cmd.BeginOutputReadLine();
            cmd.BeginErrorReadLine();

            Directory.SetCurrentDirectory(EnvironmentCurrentDirectory);
        }

        private static void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            // Collect the sort command output.
            if (!string.IsNullOrEmpty(outLine.Data))
            {
                Trace.WriteLine(outLine.Data);
            }
        }

        private static void KillProcess(int pid)
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher
                ("Select * From Win32_Process Where ParentProcessID=" + pid);
            ManagementObjectCollection collection = searcher.Get();

            Process proc = Process.GetProcessById(pid);
            if (!proc.HasExited) proc.Kill();

            if (collection != null)
            {
                foreach (ManagementObject mo in collection)
                {
                    KillProcess(Convert.ToInt32(mo["ProcessID"]));
                }
            }
        }

        public async Task<List<GameItem>> GetGameItems(User user)
        {
            var items = new List<GameItem>();
            foreach (var game in GameIDs)
            {
                items.AddRange(await HoardService.GetPlayerItems(user, game));
            }
            return items;
        }

        private User[] GetUsers()
        {
            var users = new List<User>();

            users.Add(new User("user0"));
            users[0].Accounts.Add(KeyStoreAccountService.CreateAccount("keystore", "0x779cd70609f0637ecf7449611b261411b281ee912456153d3fbdf762a8b21670"));
            Assert.True(users[0].ChangeDefaultAccount(users[0].Accounts[0]).Result);

            users.Add(new User("user1"));
            users[1].Accounts.Add(KeyStoreAccountService.CreateAccount("keystore", "0x63eacbf4503767f13c7ecdbf9b65d702913ce3d711e8386d71b8f2a2053c2b85"));
            Assert.True(users[1].ChangeDefaultAccount(users[1].Accounts[0]).Result);

            return users.ToArray();
        }
    }
}
