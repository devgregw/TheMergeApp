#region LICENSE

// Project Merge.Android:  WelcomeActivity.cs (in Solution Merge.Android)
// Created by Greg Whatley on 06/30/2017 at 9:18 AM.
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
using System.Linq;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.OS;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Firebase.Messaging;
using Merge.Android.Helpers;
using MergeApi.Client;
using MergeApi.Framework.Enumerations;
using MergeApi.Models.Actions;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using Orientation = Android.Widget.Orientation;

#endregion

namespace Merge.Android.UI.Activities {
    [Activity(Label = "Merge", ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    public class WelcomeActivity : AppCompatActivity {
        // ReSharper disable once RedundantOverriddenMember
        public override void OnConfigurationChanged(Configuration newConfig) {
            base.OnConfigurationChanged(newConfig);
        }

        private void ShowSimpleDialog(string title, string message, string positiveText,
            EventHandler<DialogClickEventArgs> positiveClick, string negativeText,
            EventHandler<DialogClickEventArgs> negativeClick) {
            var dialog = new AlertDialog.Builder(this).SetCancelable(false).SetTitle(title).SetMessage(message)
                .SetPositiveButton(positiveText, positiveClick).SetNegativeButton(negativeText, negativeClick).Create();
            dialog.SetOnShowListener(AlertDialogColorOverride.Instance);
            dialog.Show();
        }

        private void ShowWelcomeDialog() {
            ShowSimpleDialog("Welcome to Merge",
                "Welcome to the official Merge app!  The Merge app is designed to be your one-stop shop for all things Merge.  To use the Merge app, follow the steps on-screen to complete this brief setup.",
                "Start", (s, e) => ShowLegalDialog(), "Exit", (s, e) => Process.KillProcess(Process.MyPid()));
        }

        private void ShowLegalDialog() {
            var dialog = new AlertDialog.Builder(this).SetTitle("Here's the Legal Stuff").SetMessage(
                    "By using the Merge app, you agree to the MIT license as well as all third-party licenses specified.  Tap 'Licenses' to learn more, or visit the 'About Merge' page after you complete the setup.")
                .SetCancelable(false).SetPositiveButton("I Agree", (s, e) => { ShowLoggingDialog(); })
                .SetNeutralButton("Licenses", (s, e) => {
                    LaunchUriAction.FromUri("https://api.mergeonpoint.com/merge.android.html").Invoke();
                    ShowLegalDialog();
                })
                .SetNegativeButton("Back", (s, e) => ShowWelcomeDialog()).Create();
            dialog.SetOnShowListener(AlertDialogColorOverride.Instance);
            dialog.Show();
            /*ShowSimpleDialog("Here's The Legal Stuff",
                "By using the Merge app, you agree to the MIT License, as well as all third-party licenses specified on the 'About Merge' page.", "I agree", (s, e) => ShowLoggingDialog(),
                "Back", (s, e) => ShowWelcomeDialog());*/
        }

        private void ShowLoggingDialog() {
            ShowSimpleDialog("Help Make Merge Better",
                "Merge is capable of logging usage events (such as crashes) to help track down bugs, but this is disabled by default.  If you report a bug, you may be asked to turn this feature on and send the resulting log via the 'Send Logs' feature in the menu.  This behavior can be disabled or enabled from the 'Settings' page.",
                "I Understand", (s, e) => {
                    if (SdkChecker.Marshmallow)
                        ShowPermissionsDialog();
                    else
                        ShowLeaderDialog();
                },
                "Back", (s, e) => ShowLegalDialog());
        }

        private void ShowPermissionsDialog() {
            ShowSimpleDialog("We Need Your Permission",
                "Some features require special permissions.  After reading about the permissions, tap 'Continue' to grant or deny them.  Your choices can be modifed from your phone's settings." +
                _permissions,
                "Continue", (s, e) => {
                    var permissions = new[] {
                        Manifest.Permission.AccessCoarseLocation,
                        Manifest.Permission.AccessFineLocation,
                        Manifest.Permission.CallPhone,
                        Manifest.Permission.WriteExternalStorage,
                        Manifest.Permission.ReadCalendar,
                        Manifest.Permission.WriteCalendar
                    };
                    var request = permissions.Where(
                        p => ContextCompat.CheckSelfPermission(this, p) == Permission.Denied).ToArray();
                    if (!request.Any())
                        ShowLeaderDialog();
                    else
                        RequestPermissions(request, 0);
                },
                "Back", (s, e) => ShowLoggingDialog());
        }

        private void ShowPermissionsWarningDialog() {
            ShowSimpleDialog("Notice",
                "Because you denied one or more permissions, the corresponding functionality may be disabled.",
                "I Understand", (s, e) => ShowLeaderDialog(),
                "Back", (s, e) => ShowPermissionsDialog());
        }

        private void ShowLeaderDialog() {
            var dialog = new AlertDialog.Builder(this).SetTitle("Are You a Leader?")
                .SetMessage(
                    "Leaders have special access to protected features.  If you are a leader and have been provided with a username and password, tap 'Yes'.  Otherwise, tap 'No'.")
                .SetCancelable(false).SetNeutralButton("Back", (s, e) => {
                    if (SdkChecker.Marshmallow)
                        ShowPermissionsDialog();
                    else
                        ShowLoggingDialog();
                }).SetPositiveButton("Yes", (s, e) => {
                    PreferenceHelper.IsLeader = true;
                    ShowLeaderAuthenticationDialog(false);
                })
                .SetNegativeButton("No", (s, e) => {
                    PreferenceHelper.IsLeader = false;
                    PreferenceHelper.AuthenticationState = PreferenceHelper.LeaderAuthenticationState.NoAttempt;
                    PreferenceHelper.LeaderUsername = "";
                    PreferenceHelper.LeaderPassword = "";
                    ShowTargetingDialog();
                }).Create();
            dialog.SetOnShowListener(AlertDialogColorOverride.Instance);
            dialog.Show();
        }

        private void ShowTargetingDialog() {
            var scrollView = new ScrollView(this) {
                LayoutParameters =
                    new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent,
                        ViewGroup.LayoutParams.MatchParent)
            };
            var layout = new LinearLayout(this) {
                Orientation = Orientation.Vertical,
                LayoutParameters =
                    new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent,
                        ViewGroup.LayoutParams.WrapContent)
            };
            layout.AddView(new TextView(this) {
                Text = "Grade Levels",
                TextSize = 18f
            });
            layout.SetPadding((int) Resources.GetDimension(Resource.Dimension.firstRunDialogPadding), 0, 0, 0);
            foreach (var grade in EnumConsts.AllGradeLevels)
                layout.AddView(new CheckBox(this) {
                    Text = $"{(int) grade}th grade",
                    Tag = new ObjectWrapper<GradeLevel>(grade)
                });
            layout.AddView(new TextView(this) {
                Text = "Genders",
                TextSize = 18f
            });
            foreach (var gender in EnumConsts.AllGenders)
                layout.AddView(new CheckBox(this) {
                    Text = gender.ToString(),
                    Tag = new ObjectWrapper<Gender>(gender)
                });
            scrollView.AddView(layout);
            var dialog = new AlertDialog.Builder(this).SetTitle("Tell Us About Yourself").SetMessage(
                    "Merge can filter out content that is irrelevant to you.  If you are a student, select your grade level and gender.  If you are a leader or parent, select the grade level(s) and gender(s) that apply to your student(s).  To disable this feature, do not select any grade levels or genders.  Your preferences can be modified anytime with the 'Settings' page.")
                .SetCancelable(false).SetView(scrollView)
                .SetPositiveButton("Save", (s, e) => {
                    PreferenceHelper.GradeLevels = layout.GetChildren()
                        .Where(v => v is CheckBox)
                        .Cast<CheckBox>()
                        .Where(v => v.Text.Contains("grade") && v.Checked)
                        .Select(v => (v.Tag as ObjectWrapper<GradeLevel>).Value)
                        .ToList();
                    PreferenceHelper.Genders = layout.GetChildren()
                        .Where(v => v is CheckBox)
                        .Cast<CheckBox>()
                        .Where(v => v.Text.ToLower().Contains("male") && v.Checked)
                        .Select(v => (v.Tag as ObjectWrapper<Gender>).Value)
                        .ToList();
                    ShowCompleteDialog();
                })
                .SetNegativeButton("Back", (s, e) => ShowLeaderDialog()).Create();
            dialog.SetOnShowListener(AlertDialogColorOverride.Instance);
            dialog.Show();
        }

        private void ShowCompleteDialog() {
            PreferenceHelper.FirstRun = false;
            ShowSimpleDialog("Setup Complete", "The Merge app is ready!  Tap 'Finish' to get started now.",
                "Finish", (s, e) => Finish(),
                "Back", (s, e) => ShowTargetingDialog());
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions,
            Permission[] grantResults) {
            if (grantResults.Contains(Permission.Denied))
                ShowPermissionsWarningDialog();
            else
                ShowLeaderDialog();
        }

        private void ShowLeaderRoutineLeaderDialog() {
            var dialog = new AlertDialog.Builder(this).SetTitle("Are You a Leader?")
                .SetMessage(
                    "Leaders have special access to protected features.  If you are a leader and have been provided with a username and password, tap 'Yes'.  Otherwise, tap 'No'.")
                .SetCancelable(false).SetPositiveButton("Yes", (s, e) => {
                    PreferenceHelper.IsLeader = true;
                    ShowLeaderAuthenticationDialog(true);
                })
                .SetNegativeButton("No", (s, e) => {
                    PreferenceHelper.IsLeader = false;
                    PreferenceHelper.AuthenticationState = PreferenceHelper.LeaderAuthenticationState.NoAttempt;
                    PreferenceHelper.LeaderUsername = "";
                    PreferenceHelper.LeaderPassword = "";
                    PreferenceHelper.Token = "";
                    PreferenceHelper.TokenExpiration = DateTime.MinValue;
                    FirebaseMessaging.Instance.UnsubscribeFromTopic("verified_leaders");
                    Finish();
                }).Create();
            dialog.SetOnShowListener(AlertDialogColorOverride.Instance);
            dialog.Show();
        }

        private void ShowLeaderAuthenticationDialog(bool leaderRoutine) {
            var root = LayoutInflater.Inflate(Resource.Layout.AuthenticationDialog, new LinearLayout(this) {
                LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent,
                    ViewGroup.LayoutParams.WrapContent)
            }, false);
            EditText username = root.FindViewById<EditText>(Resource.Id.authUsername),
                password = root.FindViewById<EditText>(Resource.Id.authPassword);
            var dialog = new AlertDialog.Builder(this).SetTitle("Sign In")
                .SetMessage("Enter your credentials then tap Sign In.").SetCancelable(false).SetView(root)
                .SetPositiveButton("Sign In",
                    async (s, e) => {
                        var progressDialog = new ProgressDialog(this) {
                            Indeterminate = true
                        };
                        progressDialog.SetMessage("Authenticating...");
                        progressDialog.SetTitle("Leader Verification");
                        progressDialog.SetCancelable(false);
                        progressDialog.Show();
                        try {
                            string u = username.Text, p = password.Text;
                            MergeApplication.AuthLink = await MergeDatabase.AuthenticateAsync(u, p);
                            PreferenceHelper.Token = MergeApplication.AuthLink.FirebaseToken;
                            PreferenceHelper.TokenExpiration =
                                MergeApplication.AuthLink.Created.AddSeconds(MergeApplication.AuthLink.ExpiresIn);
                            PreferenceHelper.AuthenticationState =
                                PreferenceHelper.LeaderAuthenticationState.Successful;
                            PreferenceHelper.LeaderUsername = u;
                            PreferenceHelper.LeaderPassword = p;
                            FirebaseMessaging.Instance.SubscribeToTopic("verified_leaders");
                            if (leaderRoutine)
                                Finish();
                            else
                                ShowTargetingDialog();
                        } catch {
                            Toast.MakeText(this, "Your credentials are incorrect.", ToastLength.Long).Show();
                            FirebaseMessaging.Instance.UnsubscribeFromTopic("verified_leaders");
                            PreferenceHelper.AuthenticationState = PreferenceHelper.LeaderAuthenticationState.Failed;
                            ShowLeaderAuthenticationDialog(leaderRoutine);
                        } finally {
                            progressDialog.Dismiss();
                        }
                    })
                .SetNegativeButton("Back", (s, e) => {
                    FirebaseMessaging.Instance.UnsubscribeFromTopic("verified_leaders");
                    if (leaderRoutine)
                        ShowLeaderRoutineLeaderDialog();
                    else
                        ShowLeaderDialog();
                }).Create();
            dialog.SetOnShowListener(AlertDialogColorOverride.Instance);
            dialog.Show();
        }

        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Empty);
            if (SdkChecker.KitKat)
                Window.AddFlags(WindowManagerFlags.TranslucentStatus);
            if (!Intent.GetBooleanExtra("leaderRoutine", false))
                ShowWelcomeDialog();
            else
                ShowLeaderRoutineLeaderDialog();
        }

        #region Extras

        private const string NewLine = "\n";

        private readonly string _permissions = $@"
{NewLine}Coarse and Fine Location:
Location Services are used to sort Merge Groups by distance and to display your position on the Merge Group map.{
                NewLine
            }
Call Phones:
Merge can automatically initiate phone calls strictly at your request.{NewLine}
Read and Write Calendar:
At your request, Merge can add events to your calendar.{NewLine}
Write External Storage:
Merge can write logs to help track down bugs.  This permission allows Merge to save the logs.
";

        #endregion
    }
}