using Hoard.DistributedStorage;
using System;
using System.Diagnostics;
using System.IO;

namespace HoardTests.Fixtures
{
    public class IPFSFixture : IDisposable
    {
        static string EnvironmentCurrentDirectory = Environment.CurrentDirectory;
        static string IPFSDirectory = EnvironmentCurrentDirectory + "\\..\\..\\..\\..\\HoardTestnet\\ipfs\\";
        private Process cmd = new Process();

        public IPFSClient Client { get; private set; }

        public IPFSFixture()
        {
            Clean();

            RunDevIPFS();

            Client = new IPFSClient("http://10.30.10.121:3081", "http://10.30.10.121:3080");
        }

        public void Dispose()
        {
            Clean();

            cmd.Close();
        }

        private void RunDevIPFS()
        {
            if (Directory.Exists(IPFSDirectory))
            {
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

                // FIXME: handle errors?
                while (!cmd.StandardOutput.EndOfStream)
                {
                    string line = cmd.StandardOutput.ReadLine();
                    if (line.Equals("Daemon is ready"))
                    {
                        break;
                    }
                }

                Directory.SetCurrentDirectory(EnvironmentCurrentDirectory);
            }
        }

        private void Clean()
        {
            if (Directory.Exists(IPFSDirectory))
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
}
