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
using Formatting = Newtonsoft.Json.Formatting;

namespace Merge_Data_Utility.UI.Windows {
    /// <summary>
    /// Interaction logic for ActionCodeViewerWindow.xaml
    /// </summary>
    public partial class ActionCodeViewerWindow : Window {
        private bool _indent = true, _wrap = false;

        public ActionCodeViewerWindow() {
            InitializeComponent();
            codeBox.TextChanged += (s, e) => {
                count.Text = codeBox.Text.Length.ToString();
                tooMany.Visibility = codeBox.Text.Length > 200 ? Visibility.Visible : Visibility.Collapsed;
            };
            actionField.ShowViewCodeButton = false;
            actionField.ActionSelected += (s, e) => Update();
        }

        private void Update() {
            if (codeBox == null)
                return;
            var old = codeBox.Text;
            codeBox.TextWrapping = _wrap ? TextWrapping.Wrap : TextWrapping.NoWrap;
            try {
                codeBox.Text = actionField.SelectedAction == null ? "" : JsonConvert.SerializeObject(actionField.SelectedAction, _indent ? Formatting.Indented : Formatting.None);
            } catch {
                codeBox.Text = old;
            }
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

        private void Indent_Changed(object sender, RoutedEventArgs e) {
            _indent = ((CheckBox) sender).IsChecked.GetValueOrDefault(true);
            Update();
        }

        private void Wrap_Changed(object sender, RoutedEventArgs e) {
            _wrap = ((CheckBox)sender).IsChecked.GetValueOrDefault(false);
            Update();
        }
    }
}
