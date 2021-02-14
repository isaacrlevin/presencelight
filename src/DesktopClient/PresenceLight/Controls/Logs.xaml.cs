using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

using PresenceLight.Core;

using Serilog.Events;

namespace PresenceLight.Controls
{
    /// <summary>
    /// Interaction logic for Logs.xaml
    /// </summary>
    public partial class Logs : UserControl
    {
        public Logs()
        {
            InitializeComponent();
            this.dgLiveLogs.DataContext = _events;
            this.dgLogFiles.DataContext = LogFiles;

            PresenceEventsLogSink.PresenceEventsLogHandler += Handler;
            InitializeFileWatcher();
        }

        ObservableCollection<LogEvent> _events = new();
        ObservableCollection<FileInfo> LogFiles = new();

        static object logsLockObject = new();
        public string? LogFilePath { get; set; }
        private System.IO.FileSystemWatcher? _watcher;
        private Queue<Serilog.Events.LogEvent> _logs = new(25);
        private void DG_Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            var hyperlink = e.OriginalSource as Hyperlink;

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            var filename = (FileInfo)hyperlink.DataContext;
#pragma warning restore CS8602 // Dereference of a possibly null reference.

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

        private void Handler(object? sender, Serilog.Events.LogEvent e)
        {

            _logs.Enqueue(e);

            UpdateCollection(e);

        }
        protected override void OnRender(DrawingContext drawingContext)
        {
            if (!fileWatcherInitialized)
                InitializeFileWatcher();

            base.OnRender(drawingContext);
        }

        bool fileWatcherInitialized = false;

        private void InitializeFileWatcher()
        {
            //TODO:  May consider making this a bit mode robust in the future.. Assumes this config
            //       IS always the second item in the config file.
            // _logFilePath = _configuration["Serilog:WriteTo:1:Args:Path"];


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
        private void UpdateCollection(LogEvent e)
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                lock (logsLockObject)
                {

                    _events.Add(e);
                    if (_events.Count > MaxRowCount)
                    {
                        var oldEvents = _events.OrderByDescending(a => a.Timestamp).Skip(MaxRowCount).ToArray();
                        oldEvents.ToList().ForEach(oe =>
                        {
                            _events.Remove(oe);
                        });
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
