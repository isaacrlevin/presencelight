using System;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Threading;
using PresenceLight.Core;

namespace PresenceLight.Pages
{

    public partial class LogsPage
    {
        MainWindowModern parentWindow;
        public LogsPage()
        {
            InitializeComponent();

            parentWindow = Application.Current.Windows.OfType<MainWindowModern>().First();

            LogFilePath = App.Configuration?["Serilog:WriteTo:1:Args:Path"] ?? "";
            dgLogFiles.DataContext = LogFiles;
            dgLiveLogs.DataContext = parentWindow._events;
            InitializeFileWatcher();
        }

        ObservableCollection<FileInfo> LogFiles = new();

        static object logsLockObject = new();
        public string? LogFilePath { get; set; }
        private System.IO.FileSystemWatcher? _watcher;
        private void DG_Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            var hyperlink = e.OriginalSource as Hyperlink;
            var filename = (FileInfo)hyperlink.DataContext;
            ExploreFile(filename.FullName);
        }

        static bool ExploreFile(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
            {
                return false;
            }
            //Clean up file path so it can be navigated OK
            filePath = System.IO.Path.GetFullPath(filePath);
            System.Diagnostics.Process.Start("explorer.exe", string.Format("/select,\"{0}\"", filePath));
            return true;
        }

        bool fileWatcherInitialized = false;

        private void InitializeFileWatcher()
        {

            if (string.IsNullOrWhiteSpace(LogFilePath))
                return;
            LogFilePath = Environment.ExpandEnvironmentVariables(LogFilePath);
            if (LogFilePath.Contains('/'))
                LogFilePath = LogFilePath.Replace('/', '\\');

            var fi = new FileInfo(LogFilePath);
            if (!string.IsNullOrWhiteSpace(fi.Extension))
            {
                LogFilePath = fi.DirectoryName;
            }

            fileWatcherInitialized = true;

            if (string.IsNullOrWhiteSpace(LogFilePath))
                return;

            var di = new System.IO.DirectoryInfo(LogFilePath);

            if (di.Exists)
                di.GetFiles().ToList().ForEach(d => LogFiles.Add(d));

            _watcher = new System.IO.FileSystemWatcher(LogFilePath);

            _watcher.Deleted += Watcher_Changed;
            _watcher.Created += Watcher_Changed;
            _watcher.Changed += Watcher_Changed;

            _watcher.EnableRaisingEvents = true;
        }

        private void Watcher_Changed(object sender, System.IO.FileSystemEventArgs e)
        {
            UpdateCollection(e);
        }

        int MaxRowCount
        {
            get
            {
                try
                {
                    return Convert.ToInt32((dgLiveLogs.ActualHeight * .95) / dgLiveLogs.RowHeight);

                }
                catch
                {

                    return 10;
                }
            }
        }

        private void UpdateCollection(System.IO.FileSystemEventArgs e)
        {

            if (Application.Current.Dispatcher.CheckAccess())
            {
                lock (logsLockObject)
                {
                    switch (e.ChangeType)
                    {
                        case System.IO.WatcherChangeTypes.Created:
                            LogFiles.Add(new System.IO.FileInfo(e.FullPath));


                            break;

                        case System.IO.WatcherChangeTypes.Changed:
                            var foundLog = LogFiles.FirstOrDefault(A => A.Name.Equals(e.Name, StringComparison.CurrentCultureIgnoreCase));
                            if (foundLog != null) LogFiles.Remove(foundLog);
                            LogFiles.Add(new System.IO.FileInfo(e.FullPath));

                            break;

                        case System.IO.WatcherChangeTypes.Deleted:

                            var deletedLog = LogFiles.FirstOrDefault(A => A.Name.Equals(e.Name, StringComparison.CurrentCultureIgnoreCase));
                            if (deletedLog != null)
                                LogFiles.Remove(deletedLog);

                            break;
                    }


                }
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                     new Action(() =>
                     {
                         UpdateCollection(e);
                     }));
            }

        }      
    }
}
