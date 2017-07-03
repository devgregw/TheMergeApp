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
using System.Xml;
using MergeApi.Framework.Abstractions;
using Newtonsoft.Json;

namespace Merge_Data_Utility.UI.Windows {
    /// <summary>
    /// Interaction logic for ActionCodeViewerWindow.xaml
    /// </summary>
    public partial class ActionCodeViewerWindow : Window {
        public ActionCodeViewerWindow() {
            InitializeComponent();
            actionField.ShowViewCodeButton = false;
            actionField.ActionSelected += (s, e) => {
                var old = codeBox.Text;
                try {
                    codeBox.Text = actionField.SelectedAction == null ? "" : JsonConvert.SerializeObject(actionField.SelectedAction, Newtonsoft.Json.Formatting.Indented);
                } catch {
                    codeBox.Text = old;
                }
            };
        }

        public ActionCodeViewerWindow(ActionBase existing) : this() {
            actionField.DefaultAction = existing;
            actionField.Reset();
        }

        private void CreateAction(object sender, RoutedEventArgs e) {
            try {
                var a = ActionBase.FromJson(codeBox.Text);
                actionField.SelectedAction = a;
            } catch (Exception ex) {
                MessageBox.Show($"That code could not be translated into an action.\n{ex.GetType().FullName}: {ex.Message}\n{ex.StackTrace}", "Action Code Viewer",
                    MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
            }
        }
    }
}
