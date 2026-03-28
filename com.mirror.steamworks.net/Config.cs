#if !DISABLESTEAMWORKS
using System;
using System.IO;
using UnityEngine;

namespace Mirror.FizzySteam
{
    [Serializable]
    public class Config
    {
        private static readonly object SyncRoot = new object();
        private static readonly string ConfigPath = Path.Combine(Directory.GetCurrentDirectory(), "lan_config.json");

        private static Config instance;

        public bool lan;
        public bool auto_reload;
        public string connect_ip;
        public int connect_port;
        public string listen_ip;
        public int listen_port;

        public static Config Instance
        {
            get
            {
                lock (SyncRoot)
                {
                    if (instance == null)
                    {
                        LoadOrCreate();
                    }

                    return instance;
                }
            }
        }

        public static void EnsureLoaded()
        {
            _ = Instance;
        }

        private static void LoadOrCreate()
        {
            if (TryLoadFromDisk(out Config loaded))
            {
                instance = loaded;
                UpdateAutoReloadWatcher();
                Debug.Log($"Loaded LAN config from {ConfigPath}");
                return;
            }

            instance = CreateDefault();
            try
            {
                string defaultConfigJson = JsonUtility.ToJson(instance, true);
                File.WriteAllText(ConfigPath, defaultConfigJson);
                Debug.Log($"Created default config file at {ConfigPath}");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to create default config file at {ConfigPath}: {ex.Message}");
            }

            UpdateAutoReloadWatcher();
        }

        private static bool TryLoadFromDisk(out Config loaded)
        {
            loaded = null;

            if (!File.Exists(ConfigPath))
            {
                return false;
            }

            try
            {
                string configText = File.ReadAllText(ConfigPath);
                loaded = JsonUtility.FromJson<Config>(configText);
                if (loaded == null)
                {
                    loaded = CreateDefault();
                }
                else
                {
                    loaded.NormalizeAfterLoad();
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to load config from {ConfigPath}: {ex.Message}");
                return false;
            }
        }

        private static void UpdateAutoReloadWatcher()
        {
            if (instance != null && instance.auto_reload)
            {
                ConfigFileWatcher.Start(ConfigPath, OnConfigFileChanged, OnAutoReloadWatcherError);
            }
            else
            {
                ConfigFileWatcher.Stop();
            }
        }

        private static void OnConfigFileChanged()
        {
            lock (SyncRoot)
            {
                if (TryLoadFromDisk(out Config reloaded))
                {
                    instance = reloaded;
                    UpdateAutoReloadWatcher();
                    Debug.Log($"Auto-reloaded LAN config from {ConfigPath}");
                }
            }
        }

        private static void OnAutoReloadWatcherError(Exception ex)
        {
            Debug.LogWarning($"Config auto-reload watcher error: {ex?.Message}");
        }

        private static Config CreateDefault()
        {
            return new Config
            {
                lan = false,
                auto_reload = false,
                connect_ip = "",
                connect_listen_ip = "",
                connect_port = 0,
                listen_ip = "",
                listen_port = 0
            };
        }

        private void NormalizeAfterLoad()
        {
            connect_ip = connect_ip ?? string.Empty;
            listen_ip = listen_ip ?? string.Empty;
            connect_listen_ip = connect_listen_ip ?? string.Empty;

            if (string.IsNullOrWhiteSpace(listen_ip) && !string.IsNullOrWhiteSpace(connect_listen_ip))
            {
                listen_ip = connect_listen_ip;
            }

            if (connect_port < 0)
            {
                connect_port = 0;
            }

            if (listen_port < 0)
            {
                listen_port = 0;
            }
        }
    }
}
#endif
