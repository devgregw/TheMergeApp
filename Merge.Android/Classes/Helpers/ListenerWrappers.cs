#region LICENSE

// Project Merge.Android:  ListenerWrappers.cs (in Solution Merge.Android)
// Created by Greg Whatley on 05/22/2017 at 9:50 PM.
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
using System.Threading.Tasks;
using Android.App;
using Android.Support.Design.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using Object = Java.Lang.Object;

#endregion

namespace Merge.Android.Classes.Helpers {
    /// <summary>
    ///     A helper class that wraps <c>Func</c>s and <c>Task</c>s to Android listeners
    /// </summary>
    public static class ListenerWrappers {
        public sealed class AppBarStateChangeListener : AppBarLayout.IOnOffsetChangedListener {
            public enum State {
                Expanded,
                Collapsed
            }

            private CollapsingToolbarLayout _collapsingToolbarLayout;

            private Action<AppBarLayout, State> _onStateChanged;

            private Toolbar _toolbar;

            public AppBarStateChangeListener(CollapsingToolbarLayout layout, Toolbar toolbar,
                Action<AppBarLayout, State> onStateChanged) {
                _collapsingToolbarLayout = layout;
                _onStateChanged = onStateChanged;
                _toolbar = toolbar;
            }

            public IntPtr Handle { get; }

            void AppBarLayout.IOnOffsetChangedListener.OnOffsetChanged(AppBarLayout layout, int verticalOffset) {
                var s = verticalOffset == -_collapsingToolbarLayout.Height + _toolbar.Height
                    ? State.Collapsed
                    : State.Expanded;
                _onStateChanged(layout, s);
                Console.WriteLine("ONOFFSETCHANGED: " + s);
            }

            public void Dispose() { }
        }

        /// <summary>
        ///     Wrapper class for <c>IMenuItemOnMenuItemClickListener</c>
        /// </summary>
        public class MenuItemOnMenuItemClickListener : Object, IMenuItemOnMenuItemClickListener {
            /// <summary>
            ///     The function to run
            /// </summary>
            private Func<IMenuItem, bool> click;

            public MenuItemOnMenuItemClickListener(Func<IMenuItem, bool> onMenuItemClick) {
                click = onMenuItemClick;
            }

            public bool OnMenuItemClick(IMenuItem item) {
                return click.Invoke(item);
            }

            /// <summary>
            ///     Subclass for wrapping an async task
            /// </summary>
            public class Async : Object, IMenuItemOnMenuItemClickListener {
                /// <summary>
                ///     The task to run
                /// </summary>
                private Func<IMenuItem, Task<bool>> click;

                public Async(Func<IMenuItem, Task<bool>> onMenuItemClick) {
                    click = onMenuItemClick;
                }

                public bool OnMenuItemClick(IMenuItem item) {
                    var t = click.Invoke(item);
                    t.Wait();
                    return t.Result;
                }
            }
        }

#pragma warning disable 612, 618

        /// <summary>
        ///     Wrapper for <c>ActionBar.IOnNavigationListener</c>
        /// </summary>
        public class OnNavigationListener : Object, ActionBar.IOnNavigationListener {
            /// <summary>
            ///     The task to run
            /// </summary>
            private Func<int, long, bool> itemSelected;

            public OnNavigationListener(Func<int, long, bool> onNavigationItemSelected) {
                itemSelected = onNavigationItemSelected;
            }

            public bool OnNavigationItemSelected(int position, long id) {
                return itemSelected.Invoke(position, id);
            }
        }

#pragma warning restore 612, 618
    }
}