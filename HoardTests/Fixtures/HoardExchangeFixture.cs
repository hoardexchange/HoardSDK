using Hoard;
using Hoard.BC.Contracts;
using HoardSDK.ExchangeServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Threading.Tasks;
using Xunit;
using static Hoard.KeyStoreAccountService;

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
            HoardService.DefaultUser = Users[0];

            RunExchangeServer();

            BCExchangeService = new BCExchangeService(HoardService);
            BCExchangeService.Init();
            BCExchangeService.User = Users[0];

            HoardExchangeService = new HoardExchangeService(HoardService);
            HoardExchangeService.Init();
            HoardExchangeService.User = Users[0];

            GameIDs = HoardService.QueryHoardGames().Result;
            foreach (var game in GameIDs)
            {
                HoardService.RegisterHoardGame(game);
            }

            Items = GetGameItems(HoardService.DefaultUser).Result;
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
                Arguments = "/c python manage.py runserver --hoardaddress " + HoardService.Options.GameCenterContract,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            cmd.Start();
            cmd.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);

            cmd.BeginOutputReadLine();

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
            users[0].Accounts.Add(new KeyStoreAccount("keystore", "0xdc457d26e6c34ed3c3db13e9af63709869bc3565", "0x779cd70609f0637ecf7449611b261411b281ee912456153d3fbdf762a8b21670"));
            users[0].SetActiveAccount(users[0].Accounts[0]);

            users.Add(new User("user1"));
            users[1].Accounts.Add(new KeyStoreAccount("keystore", "0x1488c9ccd72b710339c393be5fd4aca8c8b5563f", "0x63eacbf4503767f13c7ecdbf9b65d702913ce3d711e8386d71b8f2a2053c2b85"));
            users[1].SetActiveAccount(users[1].Accounts[0]);

            return users.ToArray();
        }
    }
}
