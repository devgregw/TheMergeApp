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
using MergeApi.Framework.Enumerations;
using MergeApi.Tools;

namespace Merge_Data_Utility.UI.Controls.EditorFields {
    /// <summary>
    /// Interaction logic for RecurrenceField.xaml
    /// </summary>
    public partial class RecurrenceField : UserControl {
        public RecurrenceField() {
            InitializeComponent();
        }

        public List<string> GetValidationErrors() {
            if (!enableBox.IsChecked.GetValueOrDefault(false))
                return new List<string>();
            return new List<string> {
                    frequencyBox.SelectedIndex == -1 ? "A recurrence frequency must be selected." : "",
                    endBehaviorBox.SelectedIndex == 1 && !endDateBox.Value.HasValue
                        ? "A recurrence end date and time must be specified."
                        : "",
                    endBehaviorBox.SelectedIndex == -1 ? "A recurrence end behavior must be selected." : ""
                };
        }

        public RecurrenceRule GetRule() =>
            enableBox.IsChecked.GetValueOrDefault(false) ? new RecurrenceRule {
                Frequency = ((ComboBoxItem)frequencyBox.Items[frequencyBox.SelectedIndex]).Content.ToString().ToEnum<RecurrenceFrequency>(),
                Interval = intervalBox.Value.GetValueOrDefault(1),
                Count = endBehaviorBox.SelectedIndex == 2 ? occurrencesBox.Value.GetValueOrDefault(1) : (int?)null,
                // ReSharper disable once PossibleInvalidOperationException
                End = endBehaviorBox.SelectedIndex == 1 ? endDateBox.Value.Value : (DateTime?)null
            } : null;

        public void SetSource(RecurrenceRule source) {
            if (source != null) {
                enableBox.IsChecked = true;
                frequencyBox.SelectedIndex = frequencyBox.Items.OfType<ComboBoxItem>().Select(i => i.Content.ToString())
                    .ToList()
                    .IndexOf(source.Frequency.ToString());
                intervalBox.Value = source.Interval;
                if (source.End.HasValue) {
                    endBehaviorBox.SelectedIndex = 1;
                    endDateBox.Value = source.End;
                } else if (source.Count.HasValue) {
                    endBehaviorBox.SelectedIndex = 2;
                    occurrencesBox.Value = source.Count;
                } else
                    endBehaviorBox.SelectedIndex = 0;
            } else
                enableBox.IsChecked = false;
        }

        private void Enable_Changed(object sender, RoutedEventArgs e) {
            main.IsEnabled = enableBox.IsChecked.GetValueOrDefault(false);
        }

        private void EndBehaviorChanged(object sender, SelectionChangedEventArgs e) {
            void SetStates(bool until, bool number)
            {
                endDatePanel.Visibility = until ? Visibility.Visible : Visibility.Collapsed;
                occurrencesPanel.Visibility = number ? Visibility.Visible : Visibility.Collapsed;
            }

            switch (endBehaviorBox.SelectedIndex) {
                case -1:
                case 0:
                    SetStates(false, false);
                    break;
                case 1:
                    SetStates(true, false);
                    break;
                case 2:
                    SetStates(false, true);
                    break;
            }
        }
    }
}
