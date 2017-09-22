#region LICENSE

// Project Merge.Android:  FileUploader.cs (in Solution Merge.Android)
// Created by Greg Whatley on 09/06/2017 at 3:58 PM.
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
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MergeApi.Client;
using MergeApi.Tools;
using UIKit;

#endregion

namespace Merge.Classes.Helpers {
    public static class FileUploader {
        public static async Task<StorageReference> PutImageAsync(UIImage image, string name, string folder) {
            byte[] bytes;
            using (var stream = new MemoryStream()) {
                image.AsJPEG().AsStream().CopyTo(stream);
                bytes = stream.ToArray();
            }
            var base64 = Convert.ToBase64String(bytes);
            using (var client = new WebClient()) {
                var r = await client.UploadValuesTaskAsync(
                    $"https://merge.devgregw.com/content/manager.php?name={name}&folder={folder}",
                    new NameValueCollection {
                        {"data", base64}
                    });
                Console.WriteLine(Encoding.UTF8.GetString(r));
            }
            return await MergeDatabase.GetStorageReferenceAsync(name, folder);
        }
    }
}