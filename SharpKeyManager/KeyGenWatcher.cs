using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.InteropServices;

namespace SharpKeyManager
{
    public class OnGeneratorKeyChangedEventArgs : EventArgs
    {
        public string Newkey { get; set; }
    }
    class KeyGenWatcher
    {
        const int PROCESS_WM_READ = 0x0010;

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(int hProcess,
          int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        public delegate void KeyChanged(OnGeneratorKeyChangedEventArgs e);
        public event KeyChanged OnGeneratorKeyChanged;

        private string ProcessName;


        public KeyGenWatcher(string processname)
        {
            ProcessName = processname;
            Process generator = new Process();
            generator.StartInfo.FileName = ProcessName+".exe";
            generator.StartInfo.UseShellExecute = false;
            generator.Start();

            Thread watch = new Thread(WatchForChange);
            watch.Start();
        }
        private void WatchForChange()
        {
            ProcessName = ProcessName.Substring(4);
            Process process = Process.GetProcessesByName(ProcessName)[0];
            IntPtr processHandle = OpenProcess(PROCESS_WM_READ, false, process.Id);

            int bytesRead = 0;
            byte[] buffer = new byte[24];
            string oldkey = string.Empty;
            string newkey = string.Empty;
            while (!process.HasExited)
            {
                ReadProcessMemory((int)processHandle, 0x00403230, buffer, buffer.Length, ref bytesRead);
                newkey = Encoding.Default.GetString(buffer, 0, bytesRead);

                if (oldkey != newkey)
                    OnGeneratorKeyChanged(new OnGeneratorKeyChangedEventArgs { Newkey = newkey });

                oldkey = newkey;

                Thread.Sleep(10);
            }
        }
    }
}
