#if !DISABLESTEAMWORKS
using System;
using System.IO;
using System.Threading;

namespace Mirror.FizzySteam
{
    internal static class ConfigFileWatcher
    {
        private static readonly object SyncRoot = new object();

        private static FileSystemWatcher watcher;
        private static Action onChanged;
        private static Action<Exception> onError;
        private static bool isReloading;

        public static void Start(string configPath, Action changedCallback, Action<Exception> errorCallback)
        {
            lock (SyncRoot)
            {
                if (watcher != null)
                {
                    return;
                }

                onChanged = changedCallback;
                onError = errorCallback;

                string directory = Path.GetDirectoryName(configPath);
                string fileName = Path.GetFileName(configPath);

                watcher = new FileSystemWatcher(directory ?? ".", fileName)
                {
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.CreationTime | NotifyFilters.FileName,
                    IncludeSubdirectories = false,
                    EnableRaisingEvents = true
                };

                watcher.Changed += HandleChanged;
                watcher.Created += HandleChanged;
                watcher.Renamed += HandleChanged;
                watcher.Error += HandleError;
            }
        }

        public static void Stop()
        {
            lock (SyncRoot)
            {
                if (watcher == null)
                {
                    return;
                }

                watcher.EnableRaisingEvents = false;
                watcher.Changed -= HandleChanged;
                watcher.Created -= HandleChanged;
                watcher.Renamed -= HandleChanged;
                watcher.Error -= HandleError;
                watcher.Dispose();
                watcher = null;
                onChanged = null;
                onError = null;
                isReloading = false;
            }
        }

        private static void HandleChanged(object sender, FileSystemEventArgs _)
        {
            Action callback;

            lock (SyncRoot)
            {
                if (isReloading)
                {
                    return;
                }

                isReloading = true;
                callback = onChanged;
            }

            try
            {
                // Avoid reading a partially written file.
                Thread.Sleep(50);
                callback?.Invoke();
            }
            finally
            {
                lock (SyncRoot)
                {
                    isReloading = false;
                }
            }
        }

        private static void HandleError(object sender, ErrorEventArgs args)
        {
            Exception ex = args.GetException();
            onError?.Invoke(ex);
        }
    }
}
#endif
