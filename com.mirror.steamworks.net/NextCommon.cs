#if !DISABLESTEAMWORKS
using Steamworks;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Mirror.FizzySteam
{
    public abstract class NextCommon
    {

        protected Config config;

        protected const int MAX_MESSAGES = 256;

        public NextCommon()
        {
            // Load configuration from lan_config.json in running directory
            string configPath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "lan_config.json");
            if (System.IO.File.Exists(configPath))
            {
                try
                {
                    string configText = System.IO.File.ReadAllText(configPath);
                    config = JsonUtility.FromJson<Config>(configText);
                    Debug.Log($"Loaded LAN config from {configPath}");
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"Failed to load config from {configPath}: {ex.Message}");
                    config = new Config { lan = false, connect_ip = "", connect_listen_ip = "", listen_port = 0 };
                }
            }
            else
            {
                // Create default config file if it doesn't exist
                config = new Config { lan = false, connect_ip = "", connect_listen_ip = "", listen_port = 0 };
                try
                {
                    string defaultConfigJson = JsonUtility.ToJson(config, true);
                    System.IO.File.WriteAllText(configPath, defaultConfigJson);
                    Debug.Log($"Created default config file at {configPath}");
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"Failed to create default config file at {configPath}: {ex.Message}");
                }
            }
        }

        protected EResult SendSocket(HSteamNetConnection conn, byte[] data, int channelId)
        {
            Array.Resize(ref data, data.Length + 1);
            data[data.Length - 1] = (byte)channelId;

            GCHandle pinnedArray = GCHandle.Alloc(data, GCHandleType.Pinned);
            IntPtr pData = pinnedArray.AddrOfPinnedObject();
            int sendFlag = channelId == Channels.Unreliable ? Constants.k_nSteamNetworkingSend_Unreliable : Constants.k_nSteamNetworkingSend_Reliable;
#if UNITY_SERVER
            EResult res = SteamGameServerNetworkingSockets.SendMessageToConnection(conn, pData, (uint)data.Length, sendFlag, out long _);
#else
            EResult res = SteamNetworkingSockets.SendMessageToConnection(conn, pData, (uint)data.Length, sendFlag, out long _);
#endif
            if (res != EResult.k_EResultOK)
            {
                Debug.LogWarning($"Send issue: {res}");
            }

            pinnedArray.Free();
            return res;
        }

        protected (byte[], int) ProcessMessage(IntPtr ptrs)
        {
            SteamNetworkingMessage_t data = Marshal.PtrToStructure<SteamNetworkingMessage_t>(ptrs);
            byte[] managedArray = new byte[data.m_cbSize];
            Marshal.Copy(data.m_pData, managedArray, 0, data.m_cbSize);
            SteamNetworkingMessage_t.Release(ptrs);

            int channel = managedArray[managedArray.Length - 1];
            Array.Resize(ref managedArray, managedArray.Length - 1);
            return (managedArray, channel);
        }
    }
}
#endif // !DISABLESTEAMWORKS