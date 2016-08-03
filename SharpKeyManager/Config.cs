using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace SharpKeyManager
{
    class Config
    {
        public string gamefilepath { get; set; }
        public List<string> cdkeys { get; set; }

    }
    class ConfigManager
    {
        public Config config { get; set; }
        private string jsonfile;
        public string gamepath { get; set; }
        public string cod4xpath { get; set; }

        public ConfigManager(string jsonfilename)
        {
            try
            {
                if (!File.Exists(jsonfilename))
                    File.WriteAllBytes(jsonfilename, Properties.Resources.emptyconfig);
            }
            catch (Exception e) { System.Windows.Forms.MessageBox.Show(e.Message); }

            jsonfile = jsonfilename;
        }
        public void Save()
        {
            File.WriteAllText(jsonfile, JsonConvert.SerializeObject(config));
        }
        public List<string> GetKeys()
        {
            return config.cdkeys;
        }

        public void AddKey(string key)
        {
            if (config.cdkeys.Contains(key))
                return;
            config.cdkeys.Add(key);
            Save();
        }

        public string RemoveKey(string key)
        {
            config.cdkeys.Remove(key);
            Save();
            return key+" was Removed.";
        }

    }
}
