using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Content;
using Android.Support.V4.Graphics.Drawable;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace Merge.Android.Classes.Controls {
    public sealed class IconView : RelativeLayout {
        public IconView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) { }
        public IconView(Context context, int icon, string text, bool black = false, bool large = false) : base(context) =>
            Initialize(icon, text, black, large);

        public IconView(Context context, int icon, View view) : base(context) => Initialize(icon, view);
        public IconView(Context context, IAttributeSet attrs) : base(context, attrs) { }
        public IconView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr) { }
        public IconView(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes) { }

        private void Initialize(int icon, View view) {
            var v = Inflate(Context, Resource.Layout.IconView, this);
            v.FindViewById<ImageView>(Resource.Id.itIcon).SetImageResource(icon);
            v.FindViewById<FrameLayout>(Resource.Id.itContainer).AddView(view);
        }

        private void Initialize(int icon, string text, bool black, bool large) {
            var tv = new TextView(Context) {Text = text};
            if (black)
                tv.SetTextColor(Color.Black);
            if (large)
                tv.TextSize = 18f;
            Initialize(icon, tv);
        }
    }
}