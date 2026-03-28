#if !DISABLESTEAMWORKS
using System;
using System.Runtime.InteropServices;
using Steamworks;
using UnityEngine;

namespace Mirror.FizzySteam
{
    public abstract class NextCommon : IDisposable
    {
        private const int MAX_PACKET_SIZE = Constants.k_cbMaxSteamNetworkingSocketsMessageSizeSend;

        protected const int MAX_MESSAGES = 256;

        protected Config config;

        private readonly byte[] buffer;
        private readonly GCHandle pinnedBuffer;
        private readonly IntPtr bufferPtr;

        private bool disposed = false;

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

            buffer = new byte[MAX_PACKET_SIZE];
            pinnedBuffer = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            bufferPtr = pinnedBuffer.AddrOfPinnedObject();
        }

        protected EResult SendSocket(HSteamNetConnection conn, ArraySegment<byte> segment, int channelId)
        {
            int packetSize = segment.Count + 1;

            if (packetSize > MAX_PACKET_SIZE)
            {
                Debug.LogError($"Attempted to send oversize packet with {packetSize} bytes on channel with ID {channelId}.");
                return EResult.k_EResultFail;
            }

            Array.Copy(segment.Array, segment.Offset, buffer, 0, segment.Count);
            buffer[segment.Count] = (byte)channelId;

            int sendFlag = channelId == Channels.Unreliable ? Constants.k_nSteamNetworkingSend_Unreliable : Constants.k_nSteamNetworkingSend_Reliable;

#if UNITY_SERVER
            EResult res = SteamGameServerNetworkingSockets.SendMessageToConnection(conn, pData, (uint)data.Length, sendFlag, out long _);
#else
            EResult res = SteamNetworkingSockets.SendMessageToConnection(conn, bufferPtr, (uint)packetSize, sendFlag, out long _);
#endif

            if (res != EResult.k_EResultOK)
            {
                Debug.LogWarning($"Send issue: {res}");
            }

            return res;
        }

        protected (ArraySegment<byte>, int) ProcessMessage(IntPtr ptrs)
        {
            SteamNetworkingMessage_t data = Marshal.PtrToStructure<SteamNetworkingMessage_t>(ptrs);
            int packetSize = data.m_cbSize;

            // Not sure if this is a good way to handle oversize packets
            //if (packetSize > MAX_PACKET_SIZE)
            //{
            //    Debug.LogWarning($"Attempted to process oversize packet with {packetSize} bytes. Receiving an unreliable packet of size zero.");
            //    return (ArraySegment<byte>.Empty, Channels.Unreliable);
            //}

            Marshal.Copy(data.m_pData, buffer, 0, data.m_cbSize);
            SteamNetworkingMessage_t.Release(ptrs);

            var segment = new ArraySegment<byte>(buffer, 0, packetSize - 1);
            int channelId = buffer[packetSize - 1];
            return (segment, channelId);
        }

        public virtual void Dispose()
        {
            if (disposed)
            {
                return;
            }

            pinnedBuffer.Free();
            disposed = true;
        }
    }
}
#endif // !DISABLESTEAMWORKS