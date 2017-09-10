using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.IO;
using MergeApi.Client;
using MergeApi.Tools;
using Console = System.Console;

namespace Merge.Android.Helpers {
    public static class FileUploader {
        public static async Task<StorageReference> PutImageAsync(string path, string name, string folder) {
            var bytes = System.IO.File.ReadAllBytes(path);
            var base64 = Convert.ToBase64String(bytes);
            using (var client = new WebClient()) {
                var r = await client.UploadValuesTaskAsync(
                    $"https://merge.devgregw.com/content/manager.php?name={name}&folder={folder}", new NameValueCollection() {
                        { "data", base64 }
                    });
                Console.WriteLine(Encoding.UTF8.GetString(r));
            }
            return await MergeDatabase.GetStorageReferenceAsync(name, folder);
        }
    }
}