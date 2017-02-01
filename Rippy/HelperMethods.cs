using MonoTorrent.BEncoding;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rippy
{
    public static class HelperMethods
    {
        public static Task RunProcessAsync(string fileName, string arguments)
        {
            // there is no non-generic TaskCompletionSource
            var tcs = new TaskCompletionSource<bool>();

            var process = new Process
            {
                StartInfo = { FileName = fileName, Arguments = arguments },
                EnableRaisingEvents = true
            };

            process.Exited += (sender, args) =>
            {
                tcs.SetResult(true);
                process.Dispose();
            };

            process.Start();

            return tcs.Task;
        }
        /// <summary>
        /// Get custom value from the info dict of the torrent
        /// </summary>
        /// <param name="tc">TorrentCreator the extension method is bound to</param>
        /// <param name="key">The dictionary key to get</param>
        /// <returns>The value of the given key</returns>
        public static BEncodedValue GetCustomInfo(this MonoTorrent.Common.TorrentCreator tc, BEncodedString key)
        {
            var dict = (BEncodedDictionary)typeof(MonoTorrent.Common.TorrentCreator).GetField("info", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(tc);
            
            BEncodedValue val = ((BEncodedDictionary)dict).Get(key);
            return val;
        }

        /// <summary>
        /// Set custom value in the info dict of the torrent
        /// </summary>
        /// <param name="tc">TorrentCreator the extension method is bound to</param>
        /// <param name="key">The dictionary key to set</param>
        /// <param name="value">The desired value of the key</param>
        public static void AddCustomInfo(this MonoTorrent.Common.TorrentCreator tc, BEncodedString key, BEncodedValue value)
        {
            var dict = (BEncodedDictionary)typeof(MonoTorrent.Common.TorrentCreator).GetField("info", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(tc);

            ((BEncodedDictionary)dict).Set(key, value);
        }

        /// <summary>
        /// Get key from BEncodedDictionary
        /// </summary>
        /// <param name="dictionary">BEncodedDictionary the extension method is bound to</param>
        /// <param name="key">The dictionary key to get</param>
        /// <returns>the value of the given key</returns>
        public static BEncodedValue Get(this BEncodedDictionary dictionary, BEncodedString key)
        {
            return dictionary.ContainsKey(key) ? dictionary[key] : null;
        }

        /// <summary>
        /// Set key in BEncodedDictionary
        /// </summary>
        /// <param name="dictionary">BEncodedDictionary the extension method is bound to</param>
        /// <param name="key">The dictionary key to set</param>
        /// <param name="value">The desired value of the key</param>
        public static void Set(this BEncodedDictionary dictionary, BEncodedString key, BEncodedValue value)
        {
            if (dictionary.ContainsKey(key))
                dictionary[key] = value;
            else
                dictionary.Add(key, value);
        }
    }
}
