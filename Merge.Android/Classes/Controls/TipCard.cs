using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using Merge.Android.Classes.Helpers;
using MergeApi.Models.Core.Tab;

namespace Merge.Android.Classes.Controls {
    public sealed class TipCard : CardView, View.IOnClickListener {
        private readonly TabTip _tip;

        public TipCard(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) { }

        public TipCard(Context context, TabTip tip) : base(context) {
            _tip = tip;
            Tag = new ObjectWrapper<string>(_tip.Id);
            var v = Inflate(context, Resource.Layout.TipCard, this);
            SetBackgroundColor(Color.Transparent);
            var hasAction = tip.Action != null;
            var root = v.FindViewById<CardView>(Resource.Id.card);
            v.FindViewById<TextView>(Resource.Id.tipMessage).Text = tip.Message;
            v.FindViewById<LinearLayout>(Resource.Id.tipViewClickableLayout).Visibility =
                hasAction ? ViewStates.Visible : ViewStates.Gone;
            if (hasAction) {
                v.FindViewById<ImageView>(Resource.Id.tipCanClick).Alpha = 0.5f;
                root.Clickable = true;
                root.SetOnClickListener(this);
                if (SdkChecker.Lollipop)
                    root.Foreground =
                        new RippleDrawable(ColorStateList.ValueOf(Color.Argb(64, 0, 0, 0)), null,
                            new ColorDrawable(Color.Black));
            }
            var dismiss = v.FindViewById<ImageButton>(Resource.Id.tipDismissButton);
            dismiss.Visibility =
                tip.Persistent ? ViewStates.Invisible : ViewStates.Visible;
            if (!tip.Persistent)
                dismiss.SetOnClickListener(this);
        }

        public TipCard(Context context, IAttributeSet attrs) : base(context, attrs) { }
        public TipCard(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr) { }

        private void Dismiss() {
            PreferenceHelper.AddDismissedTip(_tip.Id);
            ((ViewGroup)Parent).RemoveView(this);
        }

        public void OnClick(View v) {
            switch (v.Id) {
                case Resource.Id.card:
                    _tip.Action.Invoke();
                    break;
                case Resource.Id.tipDismissButton:
                    Dismiss();
                    break;
            }
        }
    }
}