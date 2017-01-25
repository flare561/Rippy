using MonoTorrent.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;


namespace Rippy
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Private class variables
        private AlbumData _albumData;
        private Ookii.Dialogs.Wpf.ProgressDialog _createProgressDialog = new Ookii.Dialogs.Wpf.ProgressDialog()
        {
            WindowTitle = "Creating torrents",
            Text = "Copying non-music files",
            Description = "Processing...",
            ShowTimeRemaining = true,
        };
        private Semaphore _pool = new Semaphore(8, 8);
        private int _fileNumber = 0;

        //Public Class Properties
        /// <summary>
        /// 
        /// </summary>
        public bool MP3320 { get; set; } = true;
        public bool MP3V0 { get; set; } = true;
        public bool MP3V2 { get; set; } = false;
        public bool FLAC { get; set; } = false;

        public MainWindow()
        {
            InitializeComponent();
            _albumData = new AlbumData();
            DataContext = _albumData;
            _albumData.Tracker = _albumData.Trackers[0];
            _createProgressDialog.DoWork += _createProgressDialog_DoWork;
            _createProgressDialog.RunWorkerCompleted += _createProgressDialog_RunWorkerCompleted;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        private void CreateFolder(string name)
        {
            if (!Directory.Exists(Rippy.Properties.Settings.Default.OutputDirectory))
                Directory.CreateDirectory(Rippy.Properties.Settings.Default.OutputDirectory);
            if (!Directory.Exists(Rippy.Properties.Settings.Default.TorrentDirectory))
                Directory.CreateDirectory(Rippy.Properties.Settings.Default.TorrentDirectory);

            Directory.CreateDirectory(Path.Combine(Rippy.Properties.Settings.Default.OutputDirectory, name));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="directory"></param>
        private void CopyFiles(string directory)
        {
            foreach (var file in Directory.EnumerateFiles(_albumData.FlacFolder).Where(x =>  ".log" == new FileInfo(x).Extension.ToLower()))
            {
                File.Copy(file, Path.Combine(directory, new FileInfo(file).Name));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="dbargs"></param>
        /// <param name="totalFiles"></param>
        /// <returns></returns>
        private bool TranscodeFiles(string directory, string dbargs, int totalFiles)
        {
            var processes = new List<Process>();
            foreach (var file in Directory.EnumerateFiles(_albumData.FlacFolder).Where(x => ".flac" == new FileInfo(x).Extension.ToLower()))
            {
                if (_createProgressDialog.CancellationPending)
                {
                    return false;
                }
                var infile = new FileInfo(file);
                var outfile = new FileInfo(Path.Combine(directory, infile.Name.Replace(infile.Extension, (dbargs == null) ? ".flac" : ".mp3")));
                _fileNumber++;
                int progress = (int)(100 * ((double)_fileNumber / (double)totalFiles));
                _createProgressDialog.ReportProgress(progress, "Transcoding Files", string.Format(System.Globalization.CultureInfo.CurrentCulture, $"Transcoding {_fileNumber} / {totalFiles}"));
                if (dbargs == null)
                {
                    File.Copy(infile.FullName, outfile.FullName, true);
                }
                else
                {
                    var convProc = new Process
                    {
                        StartInfo = new ProcessStartInfo(Rippy.Properties.Settings.Default.DBPowerampLocation, $"-infile=\"{infile.FullName}\" -outfile=\"{outfile.FullName}\" -convert_to\"mp3 (Lame)\" {dbargs}")
                        {
                            CreateNoWindow = true,
                            WindowStyle = ProcessWindowStyle.Hidden
                        },
                        EnableRaisingEvents = true
                    };
                    convProc.Exited += (sender, args) => { _pool.Release(); };
                    _pool.WaitOne();
                    processes.Add(convProc);
                    convProc.Start();
                }
            }
            foreach (var proc in processes)
            {
                proc.WaitForExit();
            }
            return true;
        }

        private void CreateTorrent(string directory, string name)
        {
            var tc = new SourcedTorrentCreator();
            tc.Private = true;
            tc.Announces.Add(new List<string>() { _albumData.Tracker });
            tc.Source = "PTH";
            tc.Create(new TorrentFileSource(directory), Path.Combine(Rippy.Properties.Settings.Default.TorrentDirectory, $"{name}.torrent"));
        }
        

        private bool ProcessDirectory(string name, string dbargs, int totalFiles)
        {
            CreateFolder(name);
            var directory = Path.Combine(Rippy.Properties.Settings.Default.OutputDirectory, name);
            CopyFiles(directory);
            if (!TranscodeFiles(directory, dbargs, totalFiles))
                return false;
            CreateTorrent(directory,name);
            return true;
        }

        private void _createProgressDialog_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            int fileCount = Directory.EnumerateFiles(_albumData.FlacFolder).Where(x => ".flac" == new FileInfo(x).Extension.ToLower()).Count();
            int totalFiles = 0;
            if (MP3320)
                totalFiles += fileCount;
            if (MP3V0)
                totalFiles += fileCount;
            if (MP3V2)
                totalFiles += fileCount;
            if (FLAC)
                totalFiles += fileCount;
            _fileNumber = 0;

            if (MP3320)
                if (!(ProcessDirectory(_albumData.MP3320, "-b 320", totalFiles))) return;
            if (MP3V0)
                if (!(ProcessDirectory(_albumData.MP3V0, "-V 0", totalFiles))) return;
            if (MP3V2)
                if (!(ProcessDirectory(_albumData.MP3V2, "-V 2", totalFiles))) return;
            if (FLAC)
                if (!(ProcessDirectory(_albumData.FLAC, null, totalFiles))) return;
        }

        private void _createProgressDialog_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            mainWindow.IsEnabled = true;
            MessageBox.Show("Transcode and torrent creation complete!");
        }
        private void browseBtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            dialog.SelectedPath = _albumData.FlacFolder;
            var result = dialog.ShowDialog();
            _albumData.FlacFolder = dialog.SelectedPath;
        }

        private async void metadataBtn_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.IsEnabled = false;
            await HelperMethods.RunProcessAsync(Rippy.Properties.Settings.Default.MP3TagLocation, $"\"{_albumData.FlacFolder}\"");
            mainWindow.IsEnabled = true;
            UpdateMetadata();
        }

        private void createBtn_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.IsEnabled = false;
            _createProgressDialog.Show();
        }

        private void UpdateMetadata()
        {
            var tag = GetMetadata(_albumData.FlacFolder);
            _albumData.Album = tag?.Album ?? "";
            _albumData.Artist = (!string.IsNullOrEmpty(tag?.JoinedAlbumArtists) ? tag?.JoinedAlbumArtists : tag?.JoinedArtists) ?? "";
            _albumData.Year = tag?.Year.ToString() ?? "";
        }

        private TagLib.Tag GetMetadata(string folder)
        {
            try
            {
                var file = Directory.EnumerateFiles(folder).Where(x => x.ToLower().EndsWith("flac")).FirstOrDefault();
                if (file != null)
                {
                    FlacFolderTbx.Background = Brushes.White;
                    return new TagLib.Flac.File(file).Tag;
                }
                FlacFolderTbx.Background = Brushes.PaleVioletRed;
                return null;
            } catch (Exception)
            {
                FlacFolderTbx.Background = Brushes.PaleVioletRed;
                return null;
            }
        }

        private void FlacFolder_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateMetadata();
        }

        private void settingsBtn_Click(object sender, RoutedEventArgs e)
        {
            if (pnlLeftMenu.IsEnabled)
            {
                pnlLeftMenu.IsEnabled = false;
                ((System.Windows.Media.Animation.Storyboard)Resources["sbHideLeftMenu"]).Begin(pnlLeftMenu);
                MainWindowGrid.IsEnabled = true;
            }
            else
            {
                pnlLeftMenu.IsEnabled = true;
                ((System.Windows.Media.Animation.Storyboard)Resources["sbShowLeftMenu"]).Begin(pnlLeftMenu);
                MainWindowGrid.IsEnabled = false;
            }
        }

        private void dbBrowseBtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaOpenFileDialog();
            dialog.FileName = _albumData.FlacFolder;
            var result = dialog.ShowDialog();
            Rippy.Properties.Settings.Default.DBPowerampLocation = dialog.FileName;
        }

        private void mp3TagBrowseBtn_Click(object sender, RoutedEventArgs e)
        {

            var dialog = new Ookii.Dialogs.Wpf.VistaOpenFileDialog();
            dialog.FileName = _albumData.FlacFolder;
            var result = dialog.ShowDialog();
            Rippy.Properties.Settings.Default.MP3TagLocation = dialog.FileName;
        }

        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            Rippy.Properties.Settings.Default.Save();
        }

        private void trackersTbx_TextChanged(object sender, TextChangedEventArgs e)
        {
            trackersCobx?.GetBindingExpression(ComboBox.ItemsSourceProperty)?.UpdateSource();
        }
    }
}
