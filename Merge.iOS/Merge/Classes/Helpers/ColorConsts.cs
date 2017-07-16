#region LICENSE

// Project Merge.iOS:  ColorConsts.cs (in Solution Merge.iOS)
// Created by Greg Whatley on 10/27/2016 at 8:20 AM.
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

using UIKit;

#endregion

namespace Merge.Classes.Helpers {
    public static class ColorConsts {
        /*public const string PrimaryColor = "#363636";

        public const string PrimaryDarkColor = "#242424";*/

        public const string AccentColor = "#ffd326"; //"#fdd835";

        public const string PrimaryLightColor = "#e6e6e6";

        public static readonly UIColor PrimaryUiColor = UIColorFromHex(0xE6E6E6);

        //public static readonly UIColor PrimaryDarkUiColor = UIColor.FromRGB((byte)36, (byte)36, (byte)36);

        public static readonly UIColor AccentUiColor = UIColorFromHex(0xffd326);

        public static UIColor UIColorFromHex(int hexValue) => UIColor.FromRGB(((hexValue & 0xFF0000) >> 16) / 255.0f,
            ((hexValue & 0xFF00) >> 8) / 255.0f, (hexValue & 0xFF) / 255.0f);

        //public static readonly UIColor PrimaryLightUiColor = UIColor.FromRGB((byte)72, (byte)72, (byte)72);
    }
}