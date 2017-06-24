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
using System.Windows.Navigation;
using System.Windows.Shapes;
using MergeApi.Client;
using MergeApi.Framework.Abstractions;
using MergeApi.Models.Actions;
using MergeApi.Models.Core;
using Merge_Data_Utility.Tools;
using Merge_Data_Utility.UI.Pages.Base;
using Merge_Data_Utility.UI.Windows;

namespace Merge_Data_Utility.UI.Pages.ActionConfiguration {
    /// <summary>
    /// Interaction logic for OpenGroupDetailsActionPage.xaml
    /// </summary>
    public partial class OpenGroupDetailsActionPage : ActionConfigurationPage {
        public OpenGroupDetailsActionPage() : this(null) {
        }

        public OpenGroupDetailsActionPage(OpenGroupDetailsAction action) {
            InitializeComponent();
            Initialize(action);
        }

        public override void Update() {
            if (!HasCurrentValue) return;
            var cv = GetCurrentAction<OpenGroupDetailsAction>();
            groupIdBox.SetId("groups", cv.GroupId1);
        }

        public override ActionBase GetAction() {
            if (!string.IsNullOrWhiteSpace(groupIdBox.Text))
                return OpenGroupDetailsAction.FromGroupId(groupIdBox.GetId());
            DisplayErrorMessage(new[] { "No group selected" });
            return null;
        }

        private void ChooseGroup(object sender, RoutedEventArgs args) {
            var window =
                new ObjectChooserWindow(
                    async () => (await MergeDatabase.ListAsync<MergeGroup>()).Select(e => new ListViewItem {
                        Content = $"{e.Name} (groups/{e.Id})",
                        Tag = e
                    }));
            window.ShowDialog();
            if (window.ObjectSelected)
                groupIdBox.SetId("groups", window.GetSelectedObject<MergeGroup>().Id);
        }
    }
}
