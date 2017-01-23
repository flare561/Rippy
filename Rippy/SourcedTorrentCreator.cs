using MonoTorrent.BEncoding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rippy
{
    public class SourcedTorrentCreator : MonoTorrent.Common.TorrentCreator
    {
        public string Source
        {
            get
            {
                var dict = (BEncodedDictionary)typeof(SourcedTorrentCreator).BaseType.GetField("info", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(this);
                
                BEncodedValue val = Get((BEncodedDictionary)dict, new BEncodedString("source"));
                return val == null ? string.Empty : val.ToString();
            }
            set {
                var dict = (BEncodedDictionary)typeof(SourcedTorrentCreator).BaseType.GetField("info", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(this);
                Set((BEncodedDictionary)dict, "source", new BEncodedString(value));
            }
        }

        private static BEncodedValue Get(BEncodedDictionary dictionary, BEncodedString key)
        {
            return dictionary.ContainsKey(key) ? dictionary[key] : null;
        }

        private static void Set(BEncodedDictionary dictionary, BEncodedString key, BEncodedValue value)
        {
            if (dictionary.ContainsKey(key))
                dictionary[key] = value;
            else
                dictionary.Add(key, value);
        }
    }
}
