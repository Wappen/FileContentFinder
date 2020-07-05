using FileContentFinder.Models;
using System;
using System.Collections.Generic;
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

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            pathSelect.Path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            Query = "";
        }

        private List<ListViewItem> ToListViewItem(List<string> list)
        {
            var listViewItemList = new List<ListViewItem>();
            foreach (var item in list)
            {
                listViewItemList.Add(new ListViewItem() { Content = item });
            }
            return listViewItemList;
        }

        private void ListViewItem_ItemClicked(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ListViewItem;
            if (item != null && item.IsSelected)
            {
                ProcessStartInfo psi = new ProcessStartInfo($@"{pathSelect.Path}\{item.Content as string}");
                psi.UseShellExecute = true;
                psi.ErrorDialog = true;
                try
                {
                    Process.Start(psi);
                }
                catch (Exception ex) { }
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            searchButton.IsEnabled = false;

            var t = FileFinder.FindAsync(pathSelect.Path, Query, recursiveBox.IsChecked.GetValueOrDefault(), regexBox.IsChecked.GetValueOrDefault());
            t.ContinueWith((t2) => {
                fileListView.ItemsSource = ToListViewItem(t2.Result);
                searchButton.IsEnabled = true;
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }
    }
}