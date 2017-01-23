using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Rippy
{
    public class Settings : AppSettings<Settings>
    {
        public string MP3TagLocation { get; set; }
        public string DBPowerampLocation { get; set; }
        public string OutputDirectory { get; set; }
        public List<string> Trackers { get; set; }
        public string TorrentDirectory { get; set; }

        public static Settings Load()
        {
            if (File.Exists("settings.json"))
            {
                return AppSettings<Settings>.Load();
            }
            else
            {
                var defaultSettings = new Settings()
                {
                    MP3TagLocation = @"C:\Program Files (x86)\Mp3tag\Mp3tag.exe",
                    DBPowerampLocation = @"C:\Program Files\dBpoweramp\coreconverter.exe",
                    OutputDirectory = @".\Transcodes\",
                    TorrentDirectory = @".\Torrents\",
                    Trackers = new List<string>()
                    {
                        "https://example.com/stuff/announce",
                        "https://example2.com/stuff2/announce"
                    },
                };
                defaultSettings.Save();
                return defaultSettings;
            }
        }
    }
}
