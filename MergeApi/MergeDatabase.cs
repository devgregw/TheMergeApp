#region LICENSE

// Project MergeApi:  MergeDatabase.cs (in Solution MergeApi)
// Created by Greg Whatley on 06/23/2017 at 10:42 AM.
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
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Database.Query;
using LoremNET;
using MergeApi.Framework.Abstractions;
using MergeApi.Framework.Enumerations;
using MergeApi.Framework.Interfaces;
using MergeApi.Framework.Interfaces.Receivers;
using MergeApi.Models.Core;
using MergeApi.Models.Core.Attendance;
using MergeApi.Models.Elements;
using MergeApi.Tools;
using Newtonsoft.Json.Linq;

#endregion

namespace MergeApi.Client {
    public static class MergeDatabase {
        private static bool _initialized;
        public static FirebaseClient Client { get; private set; }

        public static FirebaseAuthProvider AuthProvider { get; private set; }

        public static IActionInvocationReceiver ActionInvocationReceiver { get; private set; }

        public static IElementCreationReceiver ElementCreationReceiver { get; private set; }

        public static ILogReceiver LogReceiver { get; private set; }

        private static void ThrowIfNotInitialized() {
            if (!_initialized)
                throw new InvalidOperationException("The Merge database has not been initialized!");
        }

        public static void Initialize(Func<Task<string>> authFactory, IActionInvocationReceiver actionReceiver,
            IElementCreationReceiver elementReceiver, ILogReceiver logReceiver) {
            Utilities.ThrowIfNull("MergeDatabase", "Initialize", new Dictionary<string, object> {
                {nameof(authFactory), authFactory},
                {nameof(logReceiver), logReceiver}
            });
            Client = new FirebaseClient("https://the-merge-app.firebaseio.com", new FirebaseOptions {
                AuthTokenAsyncFactory = async () => {
                    LogReceiver.Log(LogLevel.Debug, "MergeDatabase.Client.AuthTokenAsyncFactory", "Requesting authorization...");
                    var token = await authFactory();
                    LogReceiver.Log(LogLevel.Verbose, "MergeDatabase.Client.AuthTokenAsyncFactory", $"Token: {token}");
                    return token;
                }
            });
            AuthProvider = new FirebaseAuthProvider(new FirebaseConfig("AIzaSyC5tIbjfLfTuLRVbFrP6mXdcx9sHYjRmOE"));
            ActionInvocationReceiver = actionReceiver;
            ElementCreationReceiver = elementReceiver;
            LogReceiver = logReceiver;
            LogReceiver.Initialize();
            LogReceiver.Log(LogLevel.Info, "MergeDatabase.Initialize", "The database has been initialized!");
            _initialized = true;
        }

        public static string NewId() {
            LogReceiver.Log(LogLevel.Verbose, "MergeDatabase.NewId", "Generating a new ID");
            var availableCharacters = new[] {
                'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U',
                'V', 'W', 'X', 'Y', 'Z', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'
            };
            const int length = 8;
            var identifier = "";
            for (var i = 0; i < length; i++)
                identifier += Lorem.Random(availableCharacters);
            LogReceiver.Log(LogLevel.Debug, "MergeDatabase.NewId", $"ID generated: {identifier}");
            return identifier;
        }

        public static async Task<StorageReference> GetStorageReferenceAsync(string fileName, string folder) {
            LogReceiver.Log(LogLevel.Debug, "MergeDatabase.GetStorageReferenceAsync",
                $"Requesting storage reference: {folder}/{fileName}");
            return (await ListStorageReferencesAsync(folder)).FirstOrDefault(r => r.Name == fileName);
        }

        public static async Task<List<StorageReference>> ListStorageReferencesAsync(string folder) {
            LogReceiver.Log(LogLevel.Debug, "MergeDatabase.ListSotrageReferencesAsync",
                $"Retrieving storage references from {folder}");
            using (var client = new HttpClient()) {
                var r = await client.GetStringAsync("https://merge.devgregw.com/content/manager.php?folder=" + folder);
                LogReceiver.Log(LogLevel.Debug, "MergeDatabase.ListStorageReferencesAsync", $"Response: {r}");
                return JObject.Parse(r).Value<JArray>("files").Select(t => new StorageReference(t.ToString(), folder))
                    .ToList();
            }
        }

        public static async Task DeleteStorageReferenceAsync(string fileName, string folder) {
            LogReceiver.Log(LogLevel.Debug, "MergeDatabase.DeleteStorageReferenceAsync",
                $"Deleting storage reference: {folder}/{fileName}");
            using (var client = new HttpClient()) {
                var r = await client.GetStringAsync(
                    $"https://merge.devgregw.com/content/manager.php?name={fileName}&action=delete&folder={folder}");
                LogReceiver.Log(LogLevel.Debug, "MergeDatabase.DeleteStorageReferenceAsync", $"Response: {r}");
            }
        }

        public static async Task DeleteAssetsAsync<T>(T data, string fkey) where T : IIdentifiable {
            LogReceiver.Log(LogLevel.Debug, "MergeDatabase.DeleteAssetsAsync",
                $"Deleting the assets associated with {typeof(T).Name} {data.Id} ({fkey})");
            var names = new List<string>();
            if (data is MergeGroupAttendanceRecord)
                names.Add((data as MergeGroupAttendanceRecord).Image.Replace("https://merge.devgregw.com/content/",
                    ""));
            if (data is ModelBase || data is MergeGroup)
                names.Add(((dynamic) data).CoverImage.Replace("https://merge.devgregw.com/content/", ""));
            if (data is MergePage)
                names.AddRange((data as MergePage).Content.OfType<ImageElement>()
                    .Select(e => e.Url.Replace("https://merge.devgregw.com/content/", "")));
            LogReceiver.Log(LogLevel.Verbose, "MergeDatabase.DeleteAssetsAsync",
                $"The following assets will be deleted from {typeof(T).Name} {data.Id} ({fkey}): {names.Format()}");
            foreach (var name in names)
                try {
                    await DeleteStorageReferenceAsync(name, "");
                } catch (Exception ex) {
                    LogReceiver.Log(LogLevel.Error, "MergeDatabase.DeleteAssetsAsync", ex);
                }
        }

        public static async Task<FirebaseAuthLink> AuthenticateAsync() {
            ThrowIfNotInitialized();
            LogReceiver.Log(LogLevel.Info, "MergeDatabase.AuthenticateAsync(0)", "Authenticating anonymously");
            return await AuthProvider.SignInAnonymouslyAsync();
        }

        public static async Task<FirebaseAuthLink> AuthenticateAsync(string email, string password) {
            ThrowIfNotInitialized();
            LogReceiver.Log(LogLevel.Info, "MergeDatabase.AuthenticateAsync(2)", $"Authenticating as {email}");
            return await AuthProvider.SignInWithEmailAndPasswordAsync(email, password);
        }

        private static async Task<string> GetFirebaseKey<T>(T item) where T : IIdentifiable {
            ThrowIfNotInitialized();
            LogReceiver.Log(LogLevel.Verbose, "MergeDatabase.GetFirebaseKey",
                $"Retrieving Firebase key for the {typeof(T).Name} with the ID '{item.Id}'");
            var found = (await InternalListAsync<T>()).FirstOrDefault(i => i.Value.Id == item.Id);
            var key = found.Value == null ? null : found.Key;
            LogReceiver.Log(LogLevel.Verbose, "MergeDatabase.GetFirebaseKey",
                key != null
                    ? $"Identified {typeof(T).Name} {item.Id} as {key}"
                    : $"Could not identify {typeof(T).Name} {item.Id}");
            return key;
        }

        private static ChildQuery GetChildQuery(string typeName) {
            LogReceiver.Log(LogLevel.Verbose, "MergeDatabase.GetChildQuery(1)",
                $"Choosing appropriate ChildQuery for type '{typeName}'");
            if (typeName.ToLower() == "mergegroupattendancerecord")
                return Client.Child("attendance").Child("mergeGroupRecords");
            if (typeName.ToLower().Contains("attendance"))
                return Client.Child("attendance").Child($"{typeName.ToLower().Replace("attendance", "")}s");
            if (typeName.ToLower().Contains("merge"))
                return Client.Child($"{typeName.ToLower().Replace("merge", "")}s");
            if (typeName.ToLower().Contains("tab"))
                return Client.Child($"{typeName.ToLower().Replace("tab", "")}s");
            throw new InvalidOperationException("Cannot get ChildQuery for type " + typeName);
        }

        private static ChildQuery GetChildQuery(string typeName, string next) {
            var q = GetChildQuery(typeName).Child(next);
            LogReceiver.Log(LogLevel.Verbose, "MergeDatabase.GetChildQuery(2)", $"Appending child '{next}'");
            return q;
        }

        private static async Task<IDictionary<string, T>> InternalListAsync<T>() where T : IIdentifiable {
            ThrowIfNotInitialized();
            LogReceiver.Log(LogLevel.Debug, "MergeDatabase.InternalListAsync", $"Listing {typeof(T).Name}s");
            return (await GetChildQuery(typeof(T).Name).OrderByKey().OnceAsync<T>()).ToDictionary(fo => fo.Key,
                fo => fo.Object);
        }

        public static async Task<IEnumerable<T>> ListAsync<T>() where T : IIdentifiable {
            return (await InternalListAsync<T>()).Select(p => p.Value);
        }

        public static async Task<T> GetAsync<T>(string id) where T : IIdentifiable {
            ThrowIfNotInitialized();
            var all = await ListAsync<T>();
            LogReceiver.Log(LogLevel.Debug, "MergeDatabase.GetAsync",
                $"Selecting object of type '{typeof(T).Name}' with ID '{id}'");
            return all.FirstOrDefault(o => o.Id == id);
        }

        public static async Task UpdateAsync<T>(T data) where T : IIdentifiable {
            ThrowIfNotInitialized();
            var key = await GetFirebaseKey(data);
            LogReceiver.Log(LogLevel.Verbose, "MergeDatabase.UpdateAsync",
                $"Updating {typeof(T).Name}: {data.Id} ({(string.IsNullOrWhiteSpace(key) ? "<nonexistant>" : key)})");
            var q = GetChildQuery(typeof(T).Name);
            if (!string.IsNullOrWhiteSpace(key))
                await InternalDeleteAsync(data, key, false);
            await q.PostAsync(data);
        }

        private static async Task InternalDeleteAsync<T>(T item, string firebaseKey, bool deleteAssets)
            where T : IIdentifiable {
            ThrowIfNotInitialized();
            LogReceiver.Log(LogLevel.Verbose, "MergeDatabase.InternalDeleteAsync",
                $"Deleting {typeof(T).Name}: {item.Id} ({firebaseKey})");
            if (deleteAssets)
                await DeleteAssetsAsync(item, firebaseKey);
            await GetChildQuery(typeof(T).Name, firebaseKey).DeleteAsync();
        }

        public static async Task DeleteAsync<T>(T item) where T : IIdentifiable {
            await InternalDeleteAsync(item, await GetFirebaseKey(item), true);
        }
    }
}