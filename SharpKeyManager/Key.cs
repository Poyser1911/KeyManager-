using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace SharpKeyManager
{
    static class CdKey
    {
        static string cdkeypath = "SOFTWARE\\ACTIVISION\\Call of Duty 4";

        public static string GetCod4Key(bool istemp,string processname = "")
        {
            if (!istemp)
                return Registry.CurrentUser.OpenSubKey(cdkeypath, false).GetValue("codkey").ToString();
            else
                if (!MemoryReader.isRunnung(processname))
                    return "";

            return MemoryReader.ReadAddress(processname, Addresses.GameCdKeypart1, 16) + MemoryReader.ReadAddress(processname, Addresses.GameCdKeypart2, 4);
        }

        public static void SetCod4Key(string cdkey,bool istemp,string processname = "")
        {
            if (MessageBox.Show("Are you sure you want to Change Your Key to : " + cdkey, "Key Changer", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;
            if (!istemp)
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey(cdkeypath, true);
                if (key != null)
                    key.SetValue("codkey", cdkey, RegistryValueKind.String);

                key.Close();
            }
            else
            {
                MemoryReader.WriteAddress(processname, Addresses.GameCdKeypart1, 16, cdkey.Substring(0, 16));
                MemoryReader.WriteAddress(processname, Addresses.GameCdKeypart2, 4, cdkey.Substring(16));
            }
        }

        public static string CdkeyToGuid(string key)
        {
                string filename = "bin\\keytoguid.exe";
                if (!File.Exists(filename))
                    File.WriteAllBytes(filename, Properties.Resources.keytoguid);

                Process converter = new Process();
                converter.StartInfo.FileName = filename;
                converter.StartInfo.Arguments = key.Substring(0, 16);
                converter.StartInfo.UseShellExecute = false;
                converter.StartInfo.RedirectStandardOutput = true;
                converter.StartInfo.CreateNoWindow = true;
                converter.Start();

                string guid = converter.StandardOutput.ReadToEnd();

                converter.WaitForExit();

                return guid;
        }
    }
}
