using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Media;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using CheeseBind;
using Merge.Android.Helpers;
using MergeApi.Models.Core;
using MergeApi.Models.Core.Attendance;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using Newtonsoft.Json;
using Com.Nostra13.Universalimageloader.Core;
using Com.Nostra13.Universalimageloader.Core.Display;
using Java.IO;
using Environment = Android.OS.Environment;
using Uri = Android.Net.Uri;
using MergeApi.Client;

namespace Merge.Android.UI.Activities.LeadersOnly {
    [Activity(Label = "Merge Group Record Editor", ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize, WindowSoftInputMode = SoftInput.AdjustPan)]
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    public class MergeGroupRecordEditorActivity : AppCompatActivity {
        private const int StateNone = 0, StateOriginal = 1, StateModified = 2;

        [BindView(Resource.Id.groupIdView)]
        private TextView _groupId;
        [BindView(Resource.Id.recordDate)]
        private TextView _date;
        [BindView(Resource.Id.countEditText)]
        private EditText _studentCount;
        [BindView(Resource.Id.clearButton)]
        private Button _clearButton;
        [BindView(Resource.Id.cameraButton)]
        private Button _cameraButton;
        [BindView(Resource.Id.browseButton)]
        private Button _browseButton;
        [BindView(Resource.Id.image)]
        private ImageView _image;

        private bool _enable = true;
        private MergeGroup _group;
        private MergeGroupAttendanceRecord _record;
        private Bitmap _bitmap;
        private string _next, _fileName, _filePath;
        private int _imageState = StateNone;
        private Java.IO.File _file;

        [OnClick(Resource.Id.clearButton)]
        private void ClearButton_OnClick(object sender, EventArgs e) {
            if (_imageState == StateOriginal) {
                _image.SetImageResource(global::Android.Resource.Color.Transparent);
                _imageState = StateNone;
                _clearButton.Enabled = false;
            } else if (_imageState == StateModified) {
                _imageState = StateOriginal;
                if (string.IsNullOrWhiteSpace(_record?.Image)) {
                    ClearButton_OnClick(sender, e);
                    return;
                }
                Utilities.LoadImageForDisplay(_record.Image, _image);
            }
        }

        [OnClick(Resource.Id.cameraButton)]
        private void CameraButton_OnClick(object sender, EventArgs e) {
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.ReadExternalStorage) ==
                Permission.Granted) {
                var intent = new Intent(MediaStore.ActionImageCapture);
                intent.PutExtra(MediaStore.ExtraOutput, FileProvider.GetUriForFile(this, GenericFileProvider.GetAuthority(this), _file));
                StartActivityForResult(intent, 100);
                _clearButton.Enabled = true;
            } else
                RequestPermissions("camera");
        }

        [OnClick(Resource.Id.browseButton)]
        private void BrowseButton_OnClick(object sender, EventArgs e) {
            var intent = new Intent(Intent.ActionGetContent);
            intent.SetType("image/*");
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.ReadExternalStorage) == Permission.Granted)
                StartActivityForResult(intent, 101);
            else
                RequestPermissions("browse");
        }

        private void RequestPermissions(string next) {
            _next = next;
            RequestPermissions(new[] { Manifest.Permission.ReadExternalStorage, Manifest.Permission.WriteExternalStorage }, 100);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults) {
            if (requestCode == 100 && grantResults[0] == Permission.Granted) {
                if (_next == "camera")
                    CameraButton_OnClick(null, null);
                else if (_next == "browse")
                    BrowseButton_OnClick(null, null);
                _next = null;
            } else
                Toast.MakeText(this, "Permission to read storage denied.", ToastLength.Short).Show();
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data) {
            base.OnActivityResult(requestCode, resultCode, data);
            if (resultCode != Result.Ok) return;
            if (requestCode == 100) {
                try {
                    var bounds = new BitmapFactory.Options {InJustDecodeBounds = true};
                    BitmapFactory.DecodeFile(_filePath, bounds);
                    var bitmap = BitmapFactory.DecodeFile(_filePath);
                    var exif = new ExifInterface(_filePath);
                    var exifOrient = exif.GetAttribute(ExifInterface.TagOrientation);
                    var orientation = !string.IsNullOrWhiteSpace(exifOrient)
                        ? int.Parse(exifOrient)
                        : 1;
                    int angle;
                    switch (orientation) {
                        case 6:
                            angle = 90;
                            break;
                        case 3:
                            angle = 180;
                            break;
                        case 8:
                            angle = 270;
                            break;
                        default:
                            angle = 0;
                            break;
                    }
                    var matrix = new Matrix();
                    matrix.SetRotate(angle, (float) bitmap.Width / 2, (float) bitmap.Height / 2);
                    var final = Bitmap.CreateBitmap(bitmap, 0, 0, bounds.OutWidth, bounds.OutHeight, matrix, true);
                    _image.SetImageBitmap(final);
                    _bitmap = final;
                    
                } catch (Exception ex) {
                    Toast.MakeText(this, $"Could not rotate image: {ex.Message}", ToastLength.Long).Show();
                    _bitmap = BitmapFactory.DecodeFile(_filePath);
                } finally {
                    _imageState = StateModified;
                    _clearButton.Enabled = true;
                    using (var output = new MemoryStream()) {
                        _bitmap.Compress(Bitmap.CompressFormat.Jpeg, 100, output);
                        System.IO.File.WriteAllBytes(_filePath, output.ToArray());
                    }
                }
            } else if (requestCode == 101) {
                Bitmap b;
                if (data == null || (b =
                        MediaStore.Images.Media.GetBitmap(ApplicationContext.ContentResolver, data.Data)) ==
                    null) return;
                using (var output = new MemoryStream()) {
                    b.Compress(Bitmap.CompressFormat.Jpeg, 100, output);
                    System.IO.File.WriteAllBytes(_filePath, output.ToArray());
                }
                _image.SetImageBitmap(b);
                _clearButton.Enabled = true;
                _imageState = StateModified;
            }
        }

        public override bool OnPrepareOptionsMenu(IMenu menu) {
            menu.Add(0, 1, 0, "Save Changes").SetEnabled(_enable)
                .SetShowAsAction(ShowAsAction.Always);
            return base.OnPrepareOptionsMenu(menu);
        }

        private void SaveAndExit() {
            if (!int.TryParse(_studentCount.Text, out var count)) {
                new AlertDialog.Builder(this).SetTitle("Error")
                    .SetMessage("Please specify a number greater than or equal to 0 for the student count.")
                    .SetPositiveButton("Dismiss",
                        (s, e) => { }).Show();
                return;
            }
            var dialog = new ProgressDialog(this) {
                Indeterminate = true
            };
            dialog.SetTitle("Merge Group Record Editor");
            dialog.SetMessage("Saving changes...");
            dialog.SetCancelable(false);
            dialog.Show();
            Task.Run(async () => {
                if (_imageState == StateNone && !string.IsNullOrWhiteSpace(_record?.Image))
                    await MergeDatabase.DeleteStorageReferenceAsync(_record.Image.Replace(
                        "https://merge.devgregw.com/content/",
                        ""), "");
                await MergeDatabase.UpdateAsync(new MergeGroupAttendanceRecord {
                    MergeGroupId = _group.Id,
                    StudentCount = count,
                    Date = _record?.Date ?? DateTime.Today,
                    Image = _imageState == StateNone ? "" : _imageState == StateModified ? (await FileUploader.PutImageAsync(_filePath, _fileName, "")).Url : _record.Image
                });
                dialog.Dismiss();
                Finish();
            });
        }

        public override void OnBackPressed() {
            if (!_enable) {
                Finish();
                return;
            }
            var dialog = new AlertDialog.Builder(this).SetTitle("Save Changes").SetMessage("Do you want to save or discard your changes?").SetPositiveButton("Save",
                (s, e) => SaveAndExit()).SetNegativeButton("Discard", (s, e) => Finish()).SetNeutralButton("Cancel", (s, e) => { }).Create();
            dialog.SetOnShowListener(AlertDialogColorOverride.Instance);
            dialog.Show();
        }

        public override bool OnOptionsItemSelected(IMenuItem item) {
            switch (item.ItemId) {
                case global::Android.Resource.Id.Home:
                    OnBackPressed();
                    return true;
                case 1:
                    SaveAndExit();
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }

        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.MergeGroupRecordEditorActivity);
            Cheeseknife.Bind(this);
            if (SdkChecker.KitKat)
                Window.AddFlags(WindowManagerFlags.TranslucentStatus);
            SetSupportActionBar(FindViewById<Toolbar>(Resource.Id.toolbar));
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            _group = JsonConvert.DeserializeObject<MergeGroup>(Intent.GetStringExtra("groupJson"));
            _fileName = $"MergeGroup_{_group.Id}_{DateTime.Now.ToString("MMddyyyy", CultureInfo.CurrentUICulture)}.jpg";
            _file = new Java.IO.File(Environment.ExternalStorageDirectory,
                _fileName);
            _filePath = _file.Path;
            _groupId.Text = $"{_group.Id} ({_group.Name})";
            _date.Text = _record?.Date.ToLongDateString() ?? DateTime.Now.ToLongDateString();
            string recordJson;
            if (!string.IsNullOrWhiteSpace(recordJson = Intent.GetStringExtra("recordJson"))) {
                _record = JsonConvert.DeserializeObject<MergeGroupAttendanceRecord>(recordJson);
                _enable = DateTime.Now.DayOfYear == _record.Date.DayOfYear;
                _browseButton.Enabled = _enable;
                _cameraButton.Enabled = _enable;
                Title = $"{(_enable ? "Edit" : "View")} Record";
                _studentCount.Text = _record.StudentCount.ToString();
                if (!string.IsNullOrWhiteSpace(_record.Image)) {
                    _clearButton.Enabled = _enable;
                    Utilities.LoadImageForDisplay(_record.Image, _image);
                    _imageState = StateOriginal;
                } else
                    _clearButton.Enabled = false;
                LogHelper.FirebaseLog(this, "manageMergeGroupAttendanceRecord", new Dictionary<string, string> {
                    {"groupId", _group.Id},
                    {"recordDate", _record.Date.ToString("MMddyyyy")},
                    { "editable", _enable.ToString()}
                });
            } else {
                _clearButton.Enabled = false;
                Title = "Add Record";
                LogHelper.FirebaseLog(this, "manageMergeGroupAttendanceRecord", new Dictionary<string, string> {
                    {"groupId", _group.Id},
                    {"recordDate", "null"},
                    { "editable", _enable.ToString()}
                });
            }
        }
    }
}