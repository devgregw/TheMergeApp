using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Merge_Data_Utility.UI.EditorStructure.Fields {
    public sealed class ImageUploadField : EditorField<string> {
        public ImageUploadField(EditorSchema parent, string title, string desc, string propName, string[] fileExts)
            : base(parent) {
            Title = title;
            Description = desc;
            PropertyName = propName;
            _extensions = fileExts;
        }

        private string[] _extensions;

        public override string Title { get; }

        public override string Description { get; }

        public override string PropertyName { get; }

        private TextBox GetTextBox() {
            return (TextBox) ((StackPanel) ((StackPanel) Element).Children[0]).Children[0];
        }

        private Image GetImage() {
            return (Image)((Grid)((StackPanel)Element).Children[1]).Children[1];
        }

        public override bool CheckInput() {
            return !string.IsNullOrWhiteSpace(GetTextBox().Text);
        }

        public override UIElement Build() {
            var tb = new TextBox {
                IsReadOnly = true,
                Width = 300
            };
            var browse = new Button {
                Margin = new Thickness(5, 0, 0, 0),
                Content = "Browse..."
            };
            var chuploaded = new Button {
                Margin = new Thickness(5, 0, 0, 0),
                Content = "Choose uploaded file..."
            };
            //TODO: EVENT HANDLERS
            Element =  new StackPanel {
                HorizontalAlignment = HorizontalAlignment.Left,
                Children = {
                    new StackPanel {
                        Orientation = Orientation.Horizontal,
                        Children = {
                            new TextBox {
                                IsReadOnly = true,
                                Width = 300
                            },
                            browse,
                            chuploaded
                        }
                    },
                    new Grid {
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Width = 320,
                        Height = 180,
                        Children = {
                            new Border {
                                BorderThickness = new Thickness(1),
                                BorderBrush = new SolidColorBrush(Colors.Black)
                            },
                            new Image {
                                HorizontalAlignment = HorizontalAlignment.Stretch,
                                VerticalAlignment = VerticalAlignment.Stretch,
                                Stretch = Stretch.UniformToFill
                            }
                        }
                    }
                }
            };
            return Element;
        }

        public override string RawValue {
            get { return GetTextBox().Text; }
            set {
                GetTextBox().Text = value;
                GetImage().Source = new BitmapImage(new Uri(value));
            }
        }

        public override string RealValue { get; set; }
    }
}
