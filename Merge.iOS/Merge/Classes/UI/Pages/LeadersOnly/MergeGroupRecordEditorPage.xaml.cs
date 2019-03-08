#region LICENSE

// Project Merge.iOS:  MergeGroupRecordEditorPage.xaml.cs (in Solution Merge.iOS)
// Created by Greg Whatley on 09/13/2017 at 2:03 PM.
// 
// The MIT License (MIT)
// 
// Copyright (c) 2017 Greg Whatley
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#endregion

#region USINGS

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using AVFoundation;
using Foundation;
using Merge.Classes.Helpers;
using Merge.Classes.Receivers;
using MergeApi;
using MergeApi.Models.Core;
using MergeApi.Models.Core.Attendance;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

#endregion

namespace Merge.Classes.UI.Pages.LeadersOnly {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MergeGroupRecordEditorPage : ContentPage {
        private bool _enable;
        private MergeGroup _group;

        private ImageState _imageState;
        private MergeGroupAttendanceRecord _record;
        private UIImage _uiImage;

        public MergeGroupRecordEditorPage(MergeGroup group, MergeGroupAttendanceRecord record) {
            InitializeComponent();
            _group = group;
            _record = record;
            _enable = record == null ? true : record.Date == DateTime.Today;
            groupId.Text = $"{_group.Name} ({_group.Id})";
            date.Text = _record?.Date.ToLongDateString() ?? DateTime.Now.ToLongDateString();
            studentCount.Text = _record?.StudentCount.ToString() ?? "0";
            _imageState = record == null
                ? ImageState.None
                : string.IsNullOrWhiteSpace(_record.Image)
                    ? ImageState.None
                    : ImageState.Original;
            if (_imageState == ImageState.Original) {
                clearButton.IsEnabled = _enable;
                image.Source = ImageSource.FromUri(new Uri(record.Image));
            }
            cameraButton.IsEnabled = _enable;
            galleryButton.IsEnabled = _enable;
            if (_enable)
                ToolbarItems.Add(new ToolbarItem("Save", null, async () => await SaveAndExit()));
            MergeLogReceiver.Log("manageMergeGroupAttendanceRecord", new Dictionary<string, string> {
                {"grouoId", _group.Id},
                {"recordDate", _record?.Date.ToString("MMddyyyy") ?? "null"},
                {"editable", _enable.ToString()}
            });
        }

        private void SetImage(UIImage i) {
            /*var size = i.Size;
            var radians = (nfloat)Math.PI / 2;
            var viewBox = new UIView(new CGRect(0, 0, i.Size.Width, i.Size.Height));
            var transform = CGAffineTransform.MakeRotation(radians);
            viewBox.Transform = transform;
            var rotatedSize = viewBox.Frame.Size;
            UIGraphics.BeginImageContextWithOptions(rotatedSize, false, UIScreen.MainScreen.Scale);
            var context = UIGraphics.GetCurrentContext();
            context.TranslateCTM(rotatedSize.Width / 2, rotatedSize.Height / 2);
            context.RotateCTM(radians);
            context.ScaleCTM(1f, -1f);
            context.DrawImage(new CGRect(-i.Size.Width / 2, -i.Size.Height / 2, i.Size.Width, i.Size.Height), i.CGImage);
            var newImage = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();*/
            _uiImage = i; //newImage;
            image.Source = ImageSource.FromStream(() => _uiImage.AsPNG().AsStream());
            _imageState = ImageState.Modified;
            clearButton.IsEnabled = true;
        }

        private async Task SaveAndExit() {
            if (!int.TryParse(studentCount.Text, out var count)) {
                AlertHelper.ShowAlert("Invalid Student Count",
                    "The student count must an integer greater than or equal to 0.", null, "OK");
                return;
            }
            new NSObject().InvokeOnMainThread(() => ((App) Application.Current).ShowLoader("Saving changes..."));
            var record = new MergeGroupAttendanceRecord {
                MergeGroupId = _group.Id,
                Date = DateTime.Parse(date.Text),
                StudentCount = count
            };
            switch (_imageState) {
                case ImageState.None:
                    if (!string.IsNullOrWhiteSpace(_record?.Image))
                        await MergeDatabase.DeleteStorageReferenceAsync(
                            _record.Image.Replace("https://merge.devgregw.com/content/", ""), "");
                    record.Image = "";
                    break;
                case ImageState.Modified:
                    record.Image = (await FileUploader.PutImageAsync(_uiImage,
                        $"MergeGroup_{_group.Id}_{DateTime.Now.ToString("MMddyyyy", CultureInfo.CurrentUICulture)}.jpg",
                        "")).Url;
                    break;
                case ImageState.Original:
                    record.Image = _record.Image;
                    break;
            }
            await MergeDatabase.UpdateAsync(record);
            new NSObject().InvokeOnMainThread(((App) Application.Current).HideLoader);
            await Navigation.PopAsync();
        }

        private void ClearImage(object sender, EventArgs e) {
            if (_imageState == ImageState.Original) {
                image.Source = null;
                _imageState = ImageState.None;
                clearButton.IsEnabled = false;
            } else if (_imageState == ImageState.Modified) {
                _imageState = ImageState.Original;
                if (string.IsNullOrWhiteSpace(_record?.Image)) {
                    ClearImage(sender, e);
                    return;
                }
                image.Source = ImageSource.FromUri(new Uri(_record.Image));
            }
        }

        private async void SelectImageCamera(object sender, EventArgs e) {
            if (AVCaptureDevice.GetAuthorizationStatus(AVMediaType.Video) != AVAuthorizationStatus.Authorized) {
                var r = await AVCaptureDevice.RequestAccessForMediaTypeAsync(AVMediaType.Video);
                if (!r) {
                    AlertHelper.ShowAlert("Access Denied", "Your camera cannot be used because you denied permission.",
                        null, "OK");
                    return;
                }
            }
            var picker = new UIImagePickerController();
            picker.FinishedPickingMedia += (s, args) => {
                SetImage(args.OriginalImage);
                picker.DismissModalViewController(true);
            };
            picker.Canceled += (s, args) => { picker.DismissModalViewController(true); };
            //picker.AllowsEditing = true;
            //picker.AllowsImageEditing = true;
            picker.SourceType = UIImagePickerControllerSourceType.Camera;
            picker.CameraDevice = UIImagePickerControllerCameraDevice.Rear;
            picker.CameraCaptureMode = UIImagePickerControllerCameraCaptureMode.Photo;
            picker.ShowsCameraControls = true;
            UIApplication.SharedApplication.KeyWindow.GetTopmostViewController()
                .PresentViewController(picker, true, () => { });
        }

        private void SelectImageGallery(object sender, EventArgs e) {
            var picker = new UIImagePickerController();
            picker.FinishedPickingMedia += (s, args) => {
                SetImage(args.OriginalImage);
                picker.DismissModalViewController(true);
            };
            picker.Canceled += (s, args) => { picker.DismissModalViewController(true); };
            //picker.AllowsEditing = true;
            //picker.AllowsImageEditing = true;
            picker.SourceType = UIImagePickerControllerSourceType.PhotoLibrary;
            UIApplication.SharedApplication.KeyWindow.GetTopmostViewController()
                .PresentViewController(picker, true, () => { });
        }

        private enum ImageState {
            None,
            Modified,
            Original
        }
    }
}