using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace Merge.Android.Classes.Controls {
    public sealed class BasicCard : CardView {
        public BasicCard(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) { }

        public BasicCard(Context context, View content) : base(context) {
            var v = Inflate(context, Resource.Layout.BasicCard, this);
            v.FindViewById<RelativeLayout>(Resource.Id.root).AddView(content);
            SetBackgroundColor(Color.Transparent);
        }

        public static string MakeExceptionString(Exception e, string current = "") => e == null ? current : MakeExceptionString(e.InnerException, $"{current}{e.GetType().FullName}: {e.Message}\n");

        private static RelativeLayout.LayoutParams MakeLayoutParams() {
            var p = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            p.AddRule(LayoutRules.CenterHorizontal);
            return p;
        }

        public BasicCard(Context context, Exception error) : this(context, new IconView(context, Resource.Drawable.Error, $"An error occurred while loading content.\n{MakeExceptionString(error)}", true, true) {
            LayoutParameters = MakeLayoutParams()
        }) {
            
        }
        
        public BasicCard(Context context, IAttributeSet attrs) : base(context, attrs) { }

        public BasicCard(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr) { }
    }
}