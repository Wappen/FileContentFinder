using FileContentFinder.Models;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FileContentFinder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _query;
        public string Query
        {
            get => _query;
            set
            {
                if (value != _query)
                    _query = value;
            }
        }

        public ObservableCollection<ListViewItem> FoundFilesItems { get; set; }

        private bool _search = true;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            FoundFilesItems = new ObservableCollection<ListViewItem>();
            pathSelect.Path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            Query = "";
        }

        private void ListViewItem_ItemClicked(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ListViewItem;
            if (item != null && item.IsSelected)
            {
                string filePath = $@"{pathSelect.Path}\{item.Content as string}";

                if (!System.IO.File.Exists(filePath))
                    return;

                string argument = "/select, \"" + filePath + "\"";

                Process.Start("explorer.exe", argument);

                //ProcessStartInfo psi = new ProcessStartInfo($@"{pathSelect.Path}\{item.Content as string}");
                //psi.UseShellExecute = true;
                //psi.ErrorDialog = true;
                //try
                //{
                //    Process.Start(psi);
                //}
                //catch (Exception ex) { }
            }
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            _search = true;

            FoundFilesItems.Clear();
            fileListView.IsEnabled = false;
            cancelButton.IsEnabled = true;
            cancelButton.Visibility = Visibility.Visible;
            searchButton.Visibility = Visibility.Hidden;
            progressBar.Visibility = Visibility.Visible;
            progressBar.IsIndeterminate = true;

            await foreach (var fileName in FileFinder.FindNext(pathSelect.Path, Query, recursiveBox.IsChecked.GetValueOrDefault(), regexBox.IsChecked.GetValueOrDefault()))
            {
                if (!_search) break;

                FoundFilesItems.Add(new ListViewItem() { Content = fileName });
                fileListView.ScrollIntoView(fileListView.Items[fileListView.Items.Count - 1]);
            }

            cancelButton.Visibility = Visibility.Hidden;
            searchButton.Visibility = Visibility.Visible;
            fileListView.IsEnabled = true;
            progressBar.Visibility = Visibility.Hidden;
            progressBar.IsIndeterminate = false;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            cancelButton.IsEnabled = false;
            _search = false;
        }
    }
}