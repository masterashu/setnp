using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace setnp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    
    class NetworkItem
    {
        public string network_name { get; set; }
        public int priority { get; set; }


        //public override string ToString()
        //{
        //    return this.network_name + ", " + this.priority + " years old";
        //}
    }

    public partial class MainWindow : Window
    {
        Process GetProcess()
        {
            Process p = new Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            return p;
        }
        public MainWindow()
        {
            InitializeComponent();
            RefreshData();
        }

        public void RefreshData()
        {
            int lineCount = 0;
            StringBuilder output = new StringBuilder();
            Process p = GetProcess();
            List<string> Networks = new List<string>();
            
            p.OutputDataReceived += new DataReceivedEventHandler((sender, e) =>
            {
                if (!String.IsNullOrEmpty(e.Data))
                {
                    lineCount++;
                    output.Append(e.Data);
                    if (e.Data.Contains("All User Profile")){
                        Networks.Add(e.Data);
                    }
                    //Console.WriteLine(e.Data);
                }
            });

            try { p.Start(); }
            catch (Exception ex){
                Console.WriteLine(ex);
            }

            p.BeginOutputReadLine();

            p.StandardInput.WriteLine("netsh " + "wlan " + "show " + "profiles");
            p.StandardInput.Flush();

            p.StandardInput.WriteLine("exit");
            p.WaitForExit();

            List<NetworkItem> items = new List<NetworkItem>();
            int priority = 1;
            //Console.WriteLine(Networks);
            foreach (string item in Networks)
            {
                var data = item.Split(':');
                //Console.WriteLine(data[1].Trim());
                items.Add(new NetworkItem() { network_name = data[1].Trim(), priority = priority++ });   
            }

            WifiList.ItemsSource = items;

        }

        private void WifiList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Console.WriteLine(e.Source);
            ListView mylistview = WifiList;
            int selected_index = mylistview.SelectedIndex;
            if (selected_index > -1)
            {
                MoveUp.IsEnabled = true;
                MoveDown.IsEnabled = true;
            }
            else
            {
                MoveUp.IsEnabled = false;
                MoveDown.IsEnabled = false;
            }

        }

        private void MoveUp_Click(object sender, RoutedEventArgs e)
        {
            Process p = GetProcess();
            p.Start();

            ListView mylistview = WifiList;
            int selected_index = mylistview.SelectedIndex;
            NetworkItem item = WifiList.Items[selected_index] as NetworkItem;

            if (selected_index > 1)
            {
                string command = string.Format("netsh wlan set profileorder name=\"{0}\" interface=\"Wi-Fi\" priority={1}", item.network_name, item.priority - 1);
                Console.WriteLine("Changing Priority of " + item.network_name + " with " + item.priority.ToString());
                p.StandardInput.WriteLine(command);
                p.StandardInput.Flush();
                p.StandardInput.WriteLine("exit");
                p.StandardInput.Flush();
                p.WaitForExit();
                RefreshData();
            }
        }

        private void MoveDown_Click(object sender, RoutedEventArgs e)
        {
            Process p = GetProcess();
            p.Start();

            ListView mylistview = WifiList;
            int selected_index = mylistview.SelectedIndex;
            NetworkItem item = WifiList.Items[selected_index] as NetworkItem;

            if (selected_index > 0 && selected_index < mylistview.Items.Count - 1)
            { 
                string command = string.Format("netsh wlan set profileorder name=\"{0}\" interface=\"Wi-Fi\" priority={1}", item.network_name, item.priority + 1);
                Console.WriteLine(command);
                p.StandardInput.WriteLine(command);
                p.StandardInput.Flush();
                p.StandardInput.WriteLine("exit");
                p.StandardInput.Flush();
                p.WaitForExit();
                RefreshData();
            }
        }

    }


}
