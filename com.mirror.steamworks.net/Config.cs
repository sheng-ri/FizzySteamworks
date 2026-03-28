#if !DISABLESTEAMWORKS
using UnityEngine;

namespace Mirror.FizzySteam
{
    [System.Serializable]
    public class Config
    {
        public bool lan;
        public string connect_ip;
        public string connect_listen_ip;
        public int listen_port;
    }
}
#endif