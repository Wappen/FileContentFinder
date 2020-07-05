using Ookii.Dialogs.Wpf;
using System;
using System.Windows;
using System.Windows.Controls;

namespace FileContentFinder.Views
{
    /// <summary>
    /// Interaktionslogik für PathSelector.xaml
    /// </summary>
    public partial class PathSelector : UserControl
    {
        public string Path
        {
            get => path.Text;
            set
            {
                if (path.Text != value)
                {
                    path.Text = value;
                    PathChanged?.Invoke(this, new EventArgs());
                }
            }
        }

        public event EventHandler PathChanged;

        public PathSelector()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaFolderBrowserDialog()
            {
                RootFolder = Environment.SpecialFolder.MyDocuments,
            };

            if (dialog.ShowDialog().GetValueOrDefault())
                Path = dialog.SelectedPath;
        }
    }
}
