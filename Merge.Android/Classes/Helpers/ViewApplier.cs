using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using Merge.Android.Classes.Controls;

namespace Merge.Android.Classes.Helpers {
    public sealed class ViewApplier {
        private LinearLayout _layout;
        private Context _context;
        private bool _isLoading = false;

        public ViewApplier(Context context, LinearLayout layout) {
            _layout = layout;
            _context = context;
        }

        private void InternalApply(IEnumerable<View> views) {
            var list = views.ToList();
            _isLoading = list.Count == 1 && list[0] is LoadingCard;
            _layout.RemoveAllViews();
            list.ForEach(_layout.AddView);
        }

        public void Apply(IEnumerable<View> views, bool animate) {
            if (!animate) {
                InternalApply(views);
                return;
            }
            AlphaAnimation fadeOut = new AlphaAnimation(1f, 0f) {
                Duration = 100
            }, fadeIn = new AlphaAnimation(0f, 1f) {
                Duration = 100
            };
            fadeOut.AnimationEnd += (s, e) => {
                InternalApply(views);
                _layout.StartAnimation(fadeIn);
            };
            _layout.StartAnimation(fadeOut);
        }

        public void Apply(View view, bool animate) => Apply(new[] { view }, animate);

        public void Apply(View first, IEnumerable<View> others, bool animate) => Apply(new List<View> { first }.Concat(others), animate);

        public void Apply(View first, View second, bool animate) => Apply(new[] { first, second }, animate);

        public void ApplyLoadingCard(bool animate) {
            if (!_isLoading)
                Apply(new LoadingCard(_context), animate);
        }
    }
}