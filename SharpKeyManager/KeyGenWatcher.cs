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
            if (!MemoryReader.isRunnung(ProcessName))
                return;
            Process process = Process.GetProcessesByName(ProcessName)[0];
            string oldkey = string.Empty;
            string newkey = string.Empty;
            while (!process.HasExited)
            {
                newkey = MemoryReader.ReadAddress(ProcessName, Addresses.KeyGenCdKey, 24);
                if (oldkey != newkey)
                    OnGeneratorKeyChanged(new OnGeneratorKeyChangedEventArgs { Newkey = newkey });
                oldkey = newkey;
                Thread.Sleep(10);
            }
        }
    }
}
