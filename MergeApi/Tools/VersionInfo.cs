#region LICENSE

// Project MergeApi:  VersionInfo.cs (in Solution MergeApi)
// Created by Greg Whatley on 06/23/2017 at 10:43 AM.
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
using System.Collections.Generic;

#endregion

namespace MergeApi.Tools {
    public static class VersionInfo {
        public const string VERSION_STRING = "1.0.0.0";

        public static readonly Version Version = Version.Parse(VERSION_STRING);

        public static readonly int Update = 0;

        public static readonly Dictionary<string, string> Licenses = new Dictionary<string, string> {
            {"MergeApi", "http://api.mergeonpoint.com/license.txt"},
            {"Lorem.PCL.NET", "https://raw.githubusercontent.com/trichards57/Lorem.DNX.NET/master/license.md"},
            {"Newtonsoft.Json", "https://raw.githubusercontent.com/JamesNK/Newtonsoft.Json/master/LICENSE.md"},
            {"XLabs.Cryptography", "https://raw.githubusercontent.com/XLabs/Xamarin-Forms-Labs/master/LICENSE"}
        };
    }
}