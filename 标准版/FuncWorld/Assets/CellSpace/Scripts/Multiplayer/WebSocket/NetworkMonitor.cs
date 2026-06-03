using UnityEngine;

namespace CellSpace.WebSocket
{
    public class NetworkMonitor : MonoBehaviour
    {
        public float updateInterval = 1f;
        public bool enableDebugLog = false;
        
        private int bytesReceived;
        private int bytesSent;
        private int messagesReceived;
        private int messagesSent;
        private float lastUpdateTime;
        
        public struct NetworkStats
        {
            public float bandwidthIn;
            public float bandwidthOut;
            public int messageRateIn;
            public int messageRateOut;
            public int activeConnections;
        }
        
        public static NetworkMonitor Instance { get; private set; }
        public static NetworkStats CurrentStats { get; private set; }

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void Update()
        {
            float timeSinceLastUpdate = Time.time - lastUpdateTime;
            if (timeSinceLastUpdate >= updateInterval)
            {
                UpdateStats(timeSinceLastUpdate);
            }
        }

        private void UpdateStats(float timeDelta)
        {
            CurrentStats = new NetworkStats
            {
                bandwidthIn = bytesReceived / (1024f * timeDelta),
                bandwidthOut = bytesSent / (1024f * timeDelta),
                messageRateIn = Mathf.RoundToInt(messagesReceived / timeDelta),
                messageRateOut = Mathf.RoundToInt(messagesSent / timeDelta),
                activeConnections = WebSocketServer.Instance?.ConnectionCount ?? 0
            };

            bytesReceived = 0;
            bytesSent = 0;
            messagesReceived = 0;
            messagesSent = 0;
            lastUpdateTime = Time.time;

            if (enableDebugLog)
                Debug.Log($"Network Stats: In={CurrentStats.bandwidthIn:F2} KB/s, Out={CurrentStats.bandwidthOut:F2} KB/s, " +
                          $"Msgs In={CurrentStats.messageRateIn}/s, Msgs Out={CurrentStats.messageRateOut}/s, " +
                          $"Connections={CurrentStats.activeConnections}");
        }

        public void RecordBytesReceived(int bytes)
        {
            bytesReceived += bytes;
            messagesReceived++;
        }

        public void RecordBytesSent(int bytes)
        {
            bytesSent += bytes;
            messagesSent++;
        }
    }
}
