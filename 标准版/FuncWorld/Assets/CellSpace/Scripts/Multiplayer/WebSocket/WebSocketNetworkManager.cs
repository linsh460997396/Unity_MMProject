using UnityEngine;

namespace CellSpace.WebSocket
{
    public class WebSocketNetworkManager : MonoBehaviour
    {
        public bool isServer = false;
        public string serverAddress = "ws://localhost:8765";
        public int serverPort = 8765;
        public bool enableDebugLog = false;
        
        private WebSocketServer server;
        private WebSocketClient client;
        
        public static WebSocketNetworkManager Instance { get; private set; }
        
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
        
        public void Initialize()
        {
            if (isServer)
            {
                StartServer();
            }
            else
            {
                StartClient();
            }
        }
        
        private void StartServer()
        {
            // 避免重复添加组件
            server = gameObject.GetComponent<WebSocketServer>();
            if (server == null)
            {
                server = gameObject.AddComponent<WebSocketServer>();
            }
            server.port = serverPort;
            server.enableDebugLog = enableDebugLog;
            server.StartServer();
            
            if (enableDebugLog)
                Debug.Log("WebSocket Network Manager initialized as Server");
        }
        
        private void StartClient()
        {
            // 避免重复添加组件
            client = gameObject.GetComponent<WebSocketClient>();
            if (client == null)
            {
                client = gameObject.AddComponent<WebSocketClient>();
            }
            client.serverAddress = serverAddress;
            client.enableDebugLog = enableDebugLog;
            client.Connect();
            
            if (enableDebugLog)
                Debug.Log("WebSocket Network Manager initialized as Client");
        }
        
        public void Shutdown()
        {
            if (server != null)
            {
                server.StopServer();
                Destroy(server);
                server = null;
            }
            
            if (client != null)
            {
                client.Disconnect();
                Destroy(client);
                client = null;
            }
            
            if (enableDebugLog)
                Debug.Log("WebSocket Network Manager shutdown");
        }
        
        public bool IsConnected()
        {
            if (isServer)
            {
                return true;
            }
            
            return client != null && client.IsConnected;
        }
        
        public void SendRequestCellData(int chunkX, int chunkY, int chunkZ)
        {
            if (client != null)
            {
                client.SendRequestCellData(chunkX, chunkY, chunkZ);
            }
        }
        
        public void SendPlaceBlock(CellInfo info, ushort blockId)
        {
            if (client != null)
            {
                client.SendPlaceBlock(info, blockId);
            }
        }
        
        public void SendChangeBlock(CellInfo info, ushort blockId)
        {
            if (client != null)
            {
                client.SendChangeBlock(info, blockId);
            }
        }
        
        public void SendDestroyBlock(CellInfo info)
        {
            if (client != null)
            {
                client.SendDestroyBlock(info);
            }
        }
        
        public void SendUpdatePlayerPosition(int chunkX, int chunkY, int chunkZ)
        {
            if (client != null)
            {
                client.SendUpdatePlayerPosition(chunkX, chunkY, chunkZ);
            }
        }
        
        public void SendUpdatePlayerRange(int range)
        {
            if (client != null)
            {
                client.SendUpdatePlayerRange(range);
            }
        }
        
        void OnDestroy()
        {
            Shutdown();
        }
    }
}