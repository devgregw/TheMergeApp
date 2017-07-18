#region LICENSE

// Project Merge Data Utility:  Pictaculous.cs (in Solution Merge Data Utility)
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using MergeApi.Client;
using MergeApi.Framework.Enumerations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#endregion

namespace Merge_Data_Utility.Tools {
    public static class Pictaculous {
        public static async Task<List<Color>> GetPaletteAsync(Uri uri) {
            using (var client = new WebClient()) {
                var name = ImageConverter.TemporaryName;
                await client.DownloadFileTaskAsync(uri, name);
                return await GetPaletteAsync(name);
            }
        }

        public static async Task<List<Color>> GetPaletteAsync(string path) {
            var bytes = File.ReadAllBytes(path);
            using (var client = new WebClient()) {
                MergeDatabase.LogReceiver.Log(LogLevel.Info, "Pictaculous",
                    "POST " + "https://merge.devgregw.com.com/util/pictaculous.php");
                var r = JObject.Parse(
                    Encoding.UTF8.GetString(
                        await client.UploadDataTaskAsync("https://merge.devgregw.com/util/pictaculous.php", bytes)));
                MergeDatabase.LogReceiver.Log(LogLevel.Debug, "Pictaculous", r.ToString(Formatting.None));
                var colors = r.Value<JObject>("info").Value<JArray>("colors");
                return colors.Select(t => t.ToString().ToColor()).ToList();
            }
        }
    }
}