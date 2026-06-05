using UnityEngine;

namespace CellSpace.WebSocket
{
    public class NetworkManager : MonoBehaviour
    {
        public NetMode netMode;
        
        private INetworkService currentService;

        public static NetworkManager Instance { get; private set; }

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

        void Start()
        {
            InitializeNetworkService();
        }

        private void InitializeNetworkService()
        {
            // 先清理旧的服务
            CleanupNetworkService();

            switch (netMode)
            {
                case NetMode.unet:
                    // 可以在这里添加UNet实现
                    break;
                case NetMode.bestHttp:
                    // 避免重复添加组件
                    var existingService = gameObject.GetComponent<WebSocketNetworkService>();
                    if (existingService != null)
                    {
                        currentService = existingService;
                    }
                    else
                    {
                        currentService = gameObject.AddComponent<WebSocketNetworkService>();
                    }
                    break;
            }

            if (currentService != null)
            {
                currentService.OnMessageReceived += HandleMessage;
                currentService.OnConnected += HandleConnected;
                currentService.OnDisconnected += HandleDisconnected;
            }
        }

        private void CleanupNetworkService()
        {
            if (currentService != null)
            {
                currentService.OnMessageReceived -= HandleMessage;
                currentService.OnConnected -= HandleConnected;
                currentService.OnDisconnected -= HandleDisconnected;

                // 销毁组件
                if (currentService is Component component)
                {
                    Destroy(component);
                }

                currentService = null;
            }
        }

        private void HandleMessage(NetworkMessage msg)
        {
            // 通过事件总线分发消息
            NetworkEventBus.DispatchMessage(msg);
        }

        private void HandleConnected()
        {
            Debug.Log("Network connected");
        }

        private void HandleDisconnected()
        {
            Debug.Log("Network disconnected");
        }

        public void Connect()
        {
            currentService?.Connect();
        }

        public void Disconnect()
        {
            currentService?.Disconnect();
        }

        public void SendMessage(NetworkMessage msg)
        {
            currentService?.SendMessage(msg);
        }

        void OnDestroy()
        {
            CleanupNetworkService();
        }
    }

    public enum NetMode
    {
        none,
        unet,
        netCode,
        bestHttp,
        other
    }
}
