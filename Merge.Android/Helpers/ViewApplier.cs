#region LICENSE

// Project Merge.Android:  ViewApplier.cs (in Solution Merge.Android)
// Created by Greg Whatley on 06/30/2017 at 9:26 AM.
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

using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using Merge.Android.UI.Views;

#endregion

namespace Merge.Android.Helpers {
    public sealed class ViewApplier {
        private Context _context;
        private bool _isLoading;
        private LinearLayout _layout;

        public ViewApplier(Context context, LinearLayout layout) {
            _layout = layout;
            _context = context;
        }

        private void InternalApply(IEnumerable<View> views) {
            var list = views.ToList();
            _isLoading = list.Count == 1 && list[0] is LoadingCard;
            _layout.RemoveAllViews();
            list.Where(i => i != null).ToList().ForEach(_layout.AddView);
        }

        public void Apply(IEnumerable<View> views, bool animate) {
            if (!animate) {
                InternalApply(views);
                return;
            }
            AlphaAnimation fadeOut = new AlphaAnimation(1f, 0f) {
                    Duration = 100
                },
                fadeIn = new AlphaAnimation(0f, 1f) {
                    Duration = 100
                };
            fadeOut.AnimationEnd += (s, e) => {
                InternalApply(views);
                _layout.StartAnimation(fadeIn);
            };
            _layout.StartAnimation(fadeOut);
        }

        public void Apply(View view, bool animate) => Apply(new[] {view}, animate);

        public void Apply(View first, IEnumerable<View> others, bool animate) => Apply(
            new List<View> {first}.Concat(others), animate);

        public void Apply(View first, View second, bool animate) => Apply(new[] {first, second}, animate);

        public void ApplyLoadingCard(bool animate) {
            if (!_isLoading)
                Apply(new LoadingCard(_context), animate);
        }
    }
}