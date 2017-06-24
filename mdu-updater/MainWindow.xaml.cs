using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
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

namespace mdu_updater {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            Loaded += (s, e) => {
                using (var client = new WebClient()) {
                    client.DownloadFileCompleted += (ss, ee) => {
                        status.Text = "Applying update...";
                        progress.IsIndeterminate = true;
                        using (var archive = ZipFile.OpenRead("update.zip")) {
                            foreach (var file in archive.Entries) {
                                if (File.Exists(file.FullName))
                                    File.Delete(file.FullName);
                            }
                        }
                        ZipFile.ExtractToDirectory("update.zip", Environment.CurrentDirectory);
                        status.Text = "Complete.";
                        progress.IsIndeterminate = false;
                        progress.Value = 0;
                        if (MessageBox.Show(this,
                                "The update has completed.  Do you want to launch Merge Data Utility now?",
                                "Update Complete", MessageBoxButton.YesNo, MessageBoxImage.Information,
                                MessageBoxResult.Yes) == MessageBoxResult.Yes)
                            Process.Start("Merge Data Utility.exe");
                        Close();
                    };
                    client.DownloadProgressChanged += (ss, ee) => {
                        status.Text = $"Downloading update...\n{ee.BytesReceived} bytes received.\n{ee.TotalBytesToReceive - ee.BytesReceived} bytes remaining.";
                        progress.IsIndeterminate = false;
                        progress.Value = ee.ProgressPercentage;
                    };
                    client.DownloadFileAsync(new Uri("http://api.mergeonpoint.com/console/latest.zip"), "update.zip");
                }
            };
        }
    }
}
