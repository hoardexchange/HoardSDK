using Hoard;
using System;
using System.Diagnostics;
using System.IO;
using System.Management;

namespace HoardTests.Fixtures
{
    public class HoardExchangeFixture : IDisposable
    {
        public static string EnvironmentCurrentDirectory = Environment.CurrentDirectory;
        public static string ExchangeDirectory = EnvironmentCurrentDirectory + "\\..\\..\\..\\..\\HoardExchangeServer";

        public HoardService HoardService { get; private set; }

        private Process cmd = new Process();

        public HoardExchangeFixture()
        {
        }

        public void Dispose()
        {
            if (HoardService != null)
            {
                HoardService.Shutdown();
            }

            KillProcess(cmd.Id);
        }

        public void Initialize(HoardService hoardService)
        {
            HoardService = hoardService;

            RunExchangeServer();
        }

        private void RunExchangeServer()
        {
            Directory.SetCurrentDirectory(ExchangeDirectory);

            cmd.StartInfo = new ProcessStartInfo
            {
                FileName = "cmd",
                Arguments = "/c workon hoardexchange && python manage.py runserver --hoardaddress " + HoardService.Options.GameCenterContract,
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
            if (!String.IsNullOrEmpty(outLine.Data))
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
    }
}
