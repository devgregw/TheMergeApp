#region LICENSE

// Project Merge Data Utility:  TinyPng.cs (in Solution Merge Data Utility)
// Created by Greg Whatley on 06/23/2017 at 10:45 AM.
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
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

#endregion

namespace Merge_Data_Utility.Tools {
    public static class TinyPng {
        public static async Task<string> CompressImage(string stamp, string path) {
            using (var tinyPng = new WebClient()) {
                var authorization =
                    Convert.ToBase64String(Encoding.UTF8.GetBytes("api:dmQ6xY1VgogAuCOxBwqkJ99v22DLyZCj"));
                tinyPng.Headers.Add(HttpRequestHeader.Authorization, $"Basic {authorization}");
                var ext = path.Substring(path.LastIndexOf(".")).Replace(".", "");
                var response =
                    await
                        tinyPng.UploadDataTaskAsync("https://api.tinify.com/shrink", File.ReadAllBytes(path));
                //Console.WriteLine($"TINIFY RESPONSE:\n{Encoding.UTF8.GetString(response)}\nEND RESPONSE");
                var newName = $"Cache\\{stamp}.temporary.{ext}";
                await tinyPng.DownloadFileTaskAsync(tinyPng.ResponseHeaders["Location"], newName);
                return newName;
            }
        }
    }
}