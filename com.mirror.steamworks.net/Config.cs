#if !DISABLESTEAMWORKS
using System;
using System.IO;
using UnityEngine;

namespace Mirror.FizzySteam
{
    [Serializable]
    public class Config
    {
        private static Config instance;

        public bool lan;
        public string connect_ip;
        public string connect_listen_ip;
        public int listen_port;

        public static Config Instance
        {
            get
            {
                if (instance == null)
                {
                    LoadOrCreate();
                }

                return instance;
            }
        }

        public static void EnsureLoaded()
        {
            _ = Instance;
        }

        private static void LoadOrCreate()
        {
            string configPath = Path.Combine(Directory.GetCurrentDirectory(), "lan_config.json");

            if (File.Exists(configPath))
            {
                try
                {
                    string configText = File.ReadAllText(configPath);
                    instance = JsonUtility.FromJson<Config>(configText);
                    if (instance == null)
                    {
                        instance = CreateDefault();
                    }

                    Debug.Log($"Loaded LAN config from {configPath}");
                    return;
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Failed to load config from {configPath}: {ex.Message}");
                }
            }

            instance = CreateDefault();
            try
            {
                string defaultConfigJson = JsonUtility.ToJson(instance, true);
                File.WriteAllText(configPath, defaultConfigJson);
                Debug.Log($"Created default config file at {configPath}");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to create default config file at {configPath}: {ex.Message}");
            }
        }

        private static Config CreateDefault()
        {
            return new Config
            {
                lan = false,
                connect_ip = "",
                connect_listen_ip = "",
                listen_port = 0
            };
        }
    }
}
#endif
