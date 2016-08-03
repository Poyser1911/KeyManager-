using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace SharpKeyManager
{
    static class CdKey
    {
        static string cdkeypath = "SOFTWARE\\ACTIVISION\\Call of Duty 4";

        public static string GetCod4Key()
        {
            return Registry.CurrentUser.OpenSubKey(cdkeypath, false).GetValue("codkey").ToString();
        }

        public static void SetCod4Key(string cdkey)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(cdkeypath, true);
            if (key != null)
                key.SetValue("codkey", cdkey, RegistryValueKind.String);

            key.Close();
        }

        public static string CdkeyToGuid(string key)
        {
            string filename = "bin\\keytoguid.exe";
            if (!File.Exists(filename))
                File.WriteAllBytes(filename,Properties.Resources.keytoguid);
            
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
