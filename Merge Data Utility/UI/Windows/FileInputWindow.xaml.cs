using System;
using System.Collections.Generic;
using System.Globalization;
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
using Merge_Data_Utility.Tools;
using Microsoft.Win32;

namespace Merge_Data_Utility.UI.Windows {
    /// <summary>
    /// Interaction logic for FileInputWindow.xaml
    /// </summary>
    public partial class FileInputWindow : Window {
        public FileInputWindow() {
            InitializeComponent();
        }

        private void Browse_Click(object sender, RoutedEventArgs e) {
            var dialog = new OpenFileDialog();
            dialog.ShowDialog(this);
            if (!string.IsNullOrWhiteSpace(dialog.FileName))
                pathBox.Text = dialog.FileName;
        }

        private void Cancel(object sender, RoutedEventArgs e) {
            Close();
        }

        private async void Ok(object sender, RoutedEventArgs e) {
            if (string.IsNullOrWhiteSpace(pathBox.Text)) {
                MessageBox.Show(this, "You must select a file or click Cancel.", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error, MessageBoxResult.OK);
                return;
            }
            var reference = new LoaderReference(content);
            reference.StartLoading("Processing...");
            try {
                await FileUploader.PutStorageReferenceAsync(pathBox.Text,
                    pathBox.Text.Remove(0, pathBox.Text.LastIndexOf("\\", StringComparison.CurrentCulture)));
            } catch (InvalidOperationException ex) {
                Console.WriteLine($"The following exception occurred, but it's probably OK.\n{ex.Message} (System.InvalidOperationException):\n{ex.StackTrace}");
            } finally {
                Close();
            }
        }
    }

}
