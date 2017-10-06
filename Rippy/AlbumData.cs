using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rippy
{
    class AlbumData : INotifyPropertyChanged
    {
        private string _artist;
        private string _album;
        private string _year;
        private string _publisher;
        private string _number;
        private string _flacFolder;
        private string _medium;
        private string _tracker;

        public event PropertyChangedEventHandler PropertyChanged;
        
        protected void OnPropertyChanged(string name)
        {
            if (name != "MP3320")
                OnPropertyChanged("MP3320");
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public string FlacFolder
        {
            get { return _flacFolder; }
            set { _flacFolder = value; OnPropertyChanged("FlacFolder");}
        }

        public string Artist
        {
            get { return _artist; }
            set { _artist = value; OnPropertyChanged("Artist"); }
        }
        public string Album
        {
            get { return _album; }
            set { _album = value; OnPropertyChanged("Album"); }
        }
        public string Year
        {
            get { return _year; }
            set { _year = value; OnPropertyChanged("Year"); }
        }
        public string Publisher
        {
            get { return _publisher; }
            set { _publisher = value; OnPropertyChanged("Publisher"); }
        }
        public string Number
        {
            get { return _number; }
            set { _number = value; OnPropertyChanged("Number"); }
        }

        public string Medium
        {
            get { return _medium; }
            set { _medium = value; OnPropertyChanged("Medium"); }
        }

        public List<string> Trackers
        {
            get { return Properties.Settings.Default.Trackers.Split(',').ToList(); }
        }

        public string TrackersString
        {
            get { return Rippy.Properties.Settings.Default.Trackers; }
            set { Rippy.Properties.Settings.Default.Trackers = value; OnPropertyChanged("Trackers"); OnPropertyChanged("TrackersString"); }
        }

        public string Tracker
        {
            get { return _tracker; }
            set { _tracker = value; OnPropertyChanged("Tracker"); }
        }

        public string MP3320
        {
            get { return FormatFolderName("MP3 320"); }
        }

        public string MP3V0
        {
            get { return FormatFolderName("MP3 V0"); }
        }

        public string MP3V2
        {
            get
            { return FormatFolderName("MP3 V2"); }
        }

        public string FLAC
        {
            get
            { return FormatFolderName("FLAC"); }
        }

        public string FLAC16
        {
            get
            { return FormatFolderName("16-bit FLAC"); }
        }

        private string FormatFolderName(string format)
        {

            var space = !string.IsNullOrWhiteSpace(Publisher) && !string.IsNullOrWhiteSpace(Number) ? " " : "";
            var pubNum = !string.IsNullOrWhiteSpace(Publisher) || !string.IsNullOrWhiteSpace(Number) ? $"{Publisher}{space}{Number}," : "";
            var medium = !string.IsNullOrWhiteSpace(Medium) ? $"{Medium}" : "";
            return $"{Artist} - {Year} - {Album} {{{pubNum}{medium}}}[{format}]";
        }
    }
}
