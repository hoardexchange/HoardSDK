using Hoard.DistributedStorage;
using System;
using System.Diagnostics;
using System.IO;

namespace HoardXTests.Fixtures
{
    public class IPFSFixture : IDisposable
    {
        static string EnvironmentCurrentDirectory = Environment.CurrentDirectory;
        static string IPFSDirectory = EnvironmentCurrentDirectory + "\\..\\..\\..\\..\\..\\HoardTestnet\\ipfs\\";
        private Process cmd = new Process();

        public IPFSClient Client { get; private set; }

        public IPFSFixture()
        {
            Clean();

            Directory.SetCurrentDirectory(IPFSDirectory);
            cmd.StartInfo = new ProcessStartInfo
            {
                FileName = IPFSDirectory + "init.bat",
                Arguments = "-n",
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            cmd.Start();
            cmd.WaitForExit();
            cmd.StartInfo.FileName = IPFSDirectory + "run.bat";
            cmd.Start();

            //FIXME
            System.Threading.Thread.Sleep(10000);

            Directory.SetCurrentDirectory(EnvironmentCurrentDirectory);

            Client = new IPFSClient("http://localhost:5001", "http://localhost:8080");
        }

        public void Dispose()
        {
            Clean();

            cmd.Close();
        }

        private void Clean()
        {
            Directory.SetCurrentDirectory(IPFSDirectory);
            cmd.StartInfo = new ProcessStartInfo
            {
                FileName = IPFSDirectory + "stop.bat",
                Arguments = "-n",
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            cmd.Start();
            cmd.WaitForExit();
            cmd.StartInfo.FileName = IPFSDirectory + "delete.bat";
            cmd.Start();
            cmd.WaitForExit();
            Directory.SetCurrentDirectory(EnvironmentCurrentDirectory);
        }
    }
}
