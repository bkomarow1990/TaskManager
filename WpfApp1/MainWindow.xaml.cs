using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
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
using System.Windows.Threading;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //DispatcherTimer _timer = null;
        public MainWindow()
        {
            InitializeComponent();

            //_timer = new DispatcherTimer();
            //_timer.Interval = new TimeSpan(0, 0, 50);
            //_timer.Tick += _timer_Tick;
            //_timer.Start();
            Load();
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            Load();
        }

        public void Load()
        {
            grid.ItemsSource = Process.GetProcesses().Select(p => new ProcessInfo(p));
        }

        private void refreshBtn_Click(object sender, RoutedEventArgs e)
        {
            Load();
        }

        private void killBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (grid.SelectedItem == null)
                {
                    MessageBox.Show("Please, select an item");
                    return;
                }
                foreach (ProcessInfo item in grid.SelectedItems)
                {
                    Process.GetProcessById(item.Id).Kill();
                    grid.Items.Remove(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void startProcces_Click(object sender, RoutedEventArgs e)
        {
            try { 
            if (String.IsNullOrEmpty(proccesNameTxtBox.Text))
            {
                MessageBox.Show("Enter Process Name");
                return;
            }
            if (String.IsNullOrEmpty(argumentsTxtBox.Text))
            {
                Process.Start(proccesNameTxtBox.Text);
                return;
            }
            Process.Start(proccesNameTxtBox.Text,argumentsTxtBox.Text);
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
        }

        private void detailsBtn_Click(object sender, RoutedEventArgs e)
        {
            if (grid.SelectedItem == null)
            {
                MessageBox.Show("Please, select an item");
                return;
            }
            ProcessInfo pr = (grid.SelectedItem as ProcessInfo);
            MessageBox.Show($"============================================\n" +
                $" Id: {pr.Id}\n Name: {pr.ProcessName}\n Exited: {pr.Exited}\n Owner: {pr.UserName}\n Procces Time: {pr.TotalProcessorTime}, StartTime: {pr.StartTime}\n" +
                "============================================\n" );
        }
    }

    class ProcessInfo
    {
        public int Id { get; set; }
        public TimeSpan TotalProcessorTime { get; set; }
        public ProcessPriorityClass PriorityClass { get; set; }
        public string UserName { get; set; }
        public string ProcessName { get; set; }
        public bool Exited { get; set; }
        public DateTime StartTime { get; set; }

        public ProcessInfo(Process pr)
        {
            Id = pr.Id;
            try { ProcessName = pr.ProcessName; } catch { }
            UserName = GetProcessOwner(pr.Id);
            try { StartTime = pr.StartTime; } catch { }
            try { TotalProcessorTime = pr.TotalProcessorTime; } catch { }
            try { PriorityClass = pr.PriorityClass; } catch { }
            try { Exited = pr.HasExited; } catch { }
        }

        public string GetProcessOwner(int processId)
        {
            string query = "Select * From Win32_Process Where ProcessID = " + processId;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            ManagementObjectCollection processList = searcher.Get();

            foreach (ManagementObject obj in processList)
            {
                string[] argList = new string[] { string.Empty, string.Empty };
                int returnVal = Convert.ToInt32(obj.InvokeMethod("GetOwner", argList));
                if (returnVal == 0)
                {
                    // return DOMAIN\user
                    return argList[0];
                }
            }

            return "NO OWNER";
        }
    }
}
