using AzLook.Models;
using AzLook.Properties;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace AzLook.ViewModels
{
    internal class MainViewModel : ObservableObject
    {
        private Downloader downloader;
        public MainViewModel()
        {
            RefreshCommand = new AsyncRelayCommand(Refresh);
            ExitCommand = new RelayCommand(Exit);

            Logs = new ObservableCollection<LogItem>();
            LogsView = CollectionViewSource.GetDefaultView(Logs);
            LogsView.Filter = OnFilterTriggered;

            string sasUrl = Settings.Default.AzureSasUrl;
            if (string.IsNullOrEmpty(sasUrl))
            {
                MessageBox.Show("Please specify Azure SAS URL for the log container in app settings.");
                downloader = null;
            }
            else
                downloader = new Downloader(sasUrl);
        }

        public ICollectionView LogsView { get; private set; }

        private ObservableCollection<LogItem> logs;
        public ObservableCollection<LogItem> Logs
        {
            get => logs;
            set => SetProperty(ref logs, value);
        }

        private string sourceFilterText;
        public string SourceFilterText
        {
            get => sourceFilterText;
            set
            {
                SetProperty(ref sourceFilterText, value);
                LogsView.Refresh();
            }
        }

        private string statusText;

        public string StatusText { get => statusText; set => SetProperty(ref statusText, value); }

        private bool isUpdating;

        public bool IsUpdating { get => isUpdating; set => SetProperty(ref isUpdating, value); }

        public ICommand RefreshCommand { get; }
        private async Task Refresh()
        {
            if (downloader == null)
            {
                UpdateStatusText("Please specify Azure SAS URL.");
                return;
            }   

            IsUpdating = true;
            Logs.Clear();
            UpdateStatusText();
            await downloader.DownloadLog(DateTime.Now);
            LogReader reader = new LogReader(@"myLog.txt");
            foreach (var item in reader.ReadItems())
            {
                Logs.Add(item);
            }
            IsUpdating = false;
            UpdateStatusText();
        }

        public ICommand ExitCommand { get; }
        private void Exit()
        {
            Application.Current.MainWindow.Close();
        }

        private bool OnFilterTriggered(object item)
        {
            if (item is LogItem LogItem)
            {
                if (!string.IsNullOrEmpty(SourceFilterText))
                    return LogItem.Source.ToLower().Contains(SourceFilterText.ToLower());
            }
            return true;
        }

        private void UpdateStatusText(string additionalNote = "")
        {
            StatusText = IsUpdating ?
                $"Loading events... {additionalNote}" :
                $"{Logs.Count} events loaded. {additionalNote}";
        }

    }
}
