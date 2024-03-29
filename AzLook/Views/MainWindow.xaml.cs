﻿using AzLook.Properties;
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
using System.Windows.Shapes;

namespace AzLook.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnLoaded(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Settings.Default.AzureSasUrl))
                MenuItem_Click(sender, null);
            else
                RefreshMenu.Command.Execute(RefreshMenu.CommandParameter);
        }

        private void DataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (sender is DataGrid dataGrid && dataGrid.SelectedItem != null)
                dataGrid.ScrollIntoView(dataGrid.SelectedItem);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var window = new SasInputWindow(Settings.Default.AzureSasUrl);
            if (window.ShowDialog() == true)
            {
                Settings.Default.AzureSasUrl = window.Answer;
                Settings.Default.Save();
                RefreshMenu.Command.Execute(RefreshMenu.CommandParameter);
            }
        }
    }
}
