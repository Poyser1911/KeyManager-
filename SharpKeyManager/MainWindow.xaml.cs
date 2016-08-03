using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using System.Net;
using System.Data;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using System.IO;
using System.IO.Compression;

namespace SharpKeyManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        const string savefile = "config.json";
        ConfigManager SaveFile = new ConfigManager(savefile);

        public MainWindow()
        {
            InitializeComponent();
            Init();
            ipbox1.Text = Clipboard.GetText();
        }
        public void Refresh()
        {
            ReloadInfo();
            ReloadSaves();
        }
        public void Init()
        {
            Print.Richtextbox(version, "Version: ^31.0 ^7by ^7Poyser");

            SaveFile.config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("config.json"));

            if (!Directory.Exists("bin"))
                Directory.CreateDirectory("bin");

            if (SaveFile.config.gamefilepath != "")
                SetPaths(SaveFile.config.gamefilepath);

            Refresh();
        }
        public void ReloadInfo()
        {
            try
            {
                Location i = GeoIP.GetCurrentLocation();
                Print.Richtextbox(ipbox, "IP: ^3" + i.ip);
                Print.Richtextbox(countrybox, "^3" + i.region + "^7," + i.countryname);
                //Print.Richtextbox(cdkeybox, "Key: ^3" + CdKey.GetCod4Key());
                Print.Richtextbox(guidbox, "Guid: ^3" + CdKey.CdkeyToGuid(CdKey.GetCod4Key()));

                string filename = "img";
                if (!Directory.Exists(filename))
                {
                    File.WriteAllBytes(filename + ".zip", Properties.Resources.images);
                    Directory.CreateDirectory(filename);
                    ZipFile.ExtractToDirectory(filename + ".zip", filename);
                }
                string uri = AppDomain.CurrentDomain.BaseDirectory + filename + "\\" + i.countrycode.ToLower() + ".png";
                flagimg.Source = new BitmapImage(new Uri(uri));

                if (File.Exists(filename + ".zip"))
                    File.Delete(filename + ".zip");

                keybox.Text = CdKey.GetCod4Key();
                keybox.SelectAll();
                keybox.Focus();
            }
            catch (Exception e) { MessageBox.Show(e.Message, "Key Manager"); }
        }
        public void ReloadSaves()
        {
            if (SaveFile.config == null || SaveFile.config.cdkeys.Count <= 0)
                return;
            List<string> cdkeys = SaveFile.GetKeys();
            selectkey.Items.Clear();
            selectkey.Items.Add("[Select Key]");
            selectkey.SelectedIndex = 0;

            if (cdkeys.Count >= 1)
                foreach (string key in cdkeys)
                    selectkey.Items.Add(key);

        }
        private void save_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to Change Your Key to : " + keybox.Text, "Key Changer", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;
            CdKey.SetCod4Key(keybox.Text);
            SaveFile.AddKey(keybox.Text);
            ReloadSaves();
            ReloadInfo();
        }
        private void load_Click(object sender, RoutedEventArgs e)
        {
            if (selectkey.SelectedIndex <= 0)
                return;

            CdKey.SetCod4Key(selectkey.SelectionBoxItem.ToString());
            keybox.Text = selectkey.SelectionBoxItem.ToString();

            ReloadInfo();
        }

        private void refresh_Click(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        private void remove_Click(object sender, RoutedEventArgs e)
        {
            if (selectkey.SelectedIndex <= 0)
                return;

            if (MessageBox.Show("Are you sre you want to remove key: " + selectkey.SelectionBoxItem.ToString(), "Key Changer", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            MessageBox.Show(SaveFile.RemoveKey(selectkey.SelectionBoxItem.ToString()), "Key Changer");
            ReloadSaves();
        }

        private void keybox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (keybox.Text.Contains(" "))
                keybox.Text = keybox.Text.Replace(" ", "").Trim();
        }

        private void generate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string filename = "bin\\rzr-cod4";
                if (!File.Exists(filename + ".exe"))
                    File.WriteAllBytes(filename + ".exe", Properties.Resources.rzr_cod4);

                KeyGenWatcher watcher = new KeyGenWatcher(filename);
                watcher.OnGeneratorKeyChanged += OnGeneratorKeyChanged;
            }
            catch (Exception x) { MessageBox.Show(x.Message, "Key Manager"); }
        }

        void OnGeneratorKeyChanged(OnGeneratorKeyChangedEventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                keybox.Text = e.Newkey;
            }));
        }
        private void browse_path(object sender, RoutedEventArgs e)
        {
            SetupPath();
        }
        public bool SetupPath()
        {
            System.Windows.Forms.DialogResult result = System.Windows.Forms.MessageBox.Show("Do you want to set game file path?", "KeyManager", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Question);
            if (result != System.Windows.Forms.DialogResult.Yes)
                return false; ;
            System.Windows.Forms.OpenFileDialog file = new System.Windows.Forms.OpenFileDialog();
            if (file.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return false;

            SetPaths(file.FileName);
            return true;
        }
        public void SetPaths(string path)
        {
            SaveFile.config.gamefilepath = path;
            SaveFile.Save();

            SaveFile.gamepath = System.IO.Path.GetDirectoryName(path);
            SaveFile.cod4xpath = SaveFile.gamepath + "\\cod4x_client";

            gamepath.Text = path;
            gamepath.IsReadOnly = true;
            gamepath.IsEnabled = true;
        }
        public void JoinServer(bool justlaunch = false, string ip = "", string port = "")
        {
            if (SaveFile.config.gamefilepath == "")
                if (!SetupPath())
                    return;

            string inkpath = "cod4game.lnk";
            try
            {
                var wsh = new IWshRuntimeLibrary.IWshShell_Class();
                IWshRuntimeLibrary.IWshShortcut shortcut = wsh.CreateShortcut(inkpath) as IWshRuntimeLibrary.IWshShortcut;
                if (!justlaunch)
                    shortcut.Arguments = "connect " + ip + ":" + port;
                shortcut.TargetPath = SaveFile.config.gamefilepath;
                shortcut.WorkingDirectory = SaveFile.gamepath;
                shortcut.Save();

                Process.Start(inkpath);

                if (File.Exists(inkpath))
                    File.Delete(inkpath);
            }
            catch (Exception c)
            {
                MessageBox.Show(c.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void openlocation_Click(object sender, RoutedEventArgs e)
        {
            if (SaveFile.gamepath == null)
                if (!SetupPath())
                    return;
            Process.Start(SaveFile.gamepath);
        }
        public void CreateIfNotExist()
        {
            if (!Directory.Exists(SaveFile.cod4xpath))
            {
                string filename = "cod4x_client.zip";
                File.WriteAllBytes(filename, Properties.Resources.cod4x_client);
                ZipFile.ExtractToDirectory(filename, SaveFile.cod4xpath);

                if (File.Exists(filename))
                    File.Delete(filename);
            }
        }
        public void Start(string filename)
        {
            if (SaveFile.cod4xpath == null)
                if (!SetupPath())
                    return;

            CreateIfNotExist();
            try
            {
                string pathtofile = SaveFile.cod4xpath + filename;
                if (File.Exists(pathtofile))
                {
                    Process bat = new Process();
                    bat.StartInfo.FileName = pathtofile;
                    bat.StartInfo.WorkingDirectory = SaveFile.cod4xpath;
                    bat.StartInfo.UseShellExecute = false;
                    //bat.StartInfo.CreateNoWindow = true;
                    bat.Start();
                }
                else
                    MessageBox.Show(SaveFile.cod4xpath + filename + " Not Found");

            }
            catch (Exception e) { MessageBox.Show(e.Message); }
        }


        public void cod4xinstall_Click(object sender, RoutedEventArgs e)
        {
            Start("\\install.cmd");
        }

        private void cod4xuninstall_Click(object sender, RoutedEventArgs e)
        {
            Start("\\uninstall.cmd");
        }

        private void ipbox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ipbox1.Text.Contains(":"))
            {
                if (ipbox1.Text.Split(':')[1] != "")
                {
                    portbox1.Text = ipbox1.Text.Split(':')[1];
                    ipbox1.Text = ipbox1.Text.Split(':')[0];
                }
                else
                {
                    ipbox1.Text = ipbox1.Text.Replace(":", "");
                    portbox1.Focus();
                }
            }
        }

        private void connect_Click(object sender, RoutedEventArgs e)
        {
            JoinServer(false, ipbox1.Text, portbox1.Text);
        }

        private void launchgame_Click(object sender, RoutedEventArgs e)
        {
            JoinServer(true);
        }

    }
}
