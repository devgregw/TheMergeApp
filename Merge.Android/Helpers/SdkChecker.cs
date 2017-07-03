#region LICENSE

// Project Merge.Android:  SdkChecker.cs (in Solution Merge.Android)
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

using Android.OS;

#endregion

namespace Merge.Android.Helpers {
    /// <summary>
    ///     A helper that checks the current API level
    /// </summary>
    public static class SdkChecker {
        /// <summary>
        ///     Gets a value indicating whether the device is running Android API 21 or later
        /// </summary>
        /// <value><c>true</c> if lollipop or later; otherwise, <c>false</c></value>
        public static bool Lollipop => CheckSdk(BuildVersionCodes.Lollipop);

        public static bool Marshmallow => CheckSdk(BuildVersionCodes.M);

        public static bool Nougat => CheckSdk(BuildVersionCodes.N);

        public static bool KitKat => CheckSdk(BuildVersionCodes.Kitkat);

        public static bool CheckSdk(BuildVersionCodes minimum) {
            LogHelper.WriteMessage("INFO", $"Checking SDK level {Build.VERSION.SdkInt} against {minimum}");
            return (int) Build.VERSION.SdkInt >= (int) minimum;
        }
    }
}