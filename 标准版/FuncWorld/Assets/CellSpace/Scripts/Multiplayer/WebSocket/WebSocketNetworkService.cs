using System;
using BestHTTP.WebSocket;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace CellSpace.WebSocket
{
    public class WebSocketNetworkService : MonoBehaviour, INetworkService
    {
        public string serverAddress = "ws://localhost:8765";
        public bool enableDebugLog = false;

        private BestHTTP.WebSocket.WebSocket webSocket;
        private string playerId;

        public bool IsConnected => webSocket != null && webSocket.IsOpen;

        public event Action<NetworkMessage> OnMessageReceived;
        public event Action OnConnected;
        public event Action OnDisconnected;

        void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void Connect()
        {
            // 先清理旧连接
            CleanupWebSocket();

            if (webSocket != null && webSocket.IsOpen)
            {
                if (enableDebugLog)
                    Debug.Log("Already connected to server");
                return;
            }

            webSocket = new BestHTTP.WebSocket.WebSocket(new Uri(serverAddress));
            webSocket.OnOpen += HandleConnected;
            webSocket.OnMessage += HandleMessageReceived;
            webSocket.OnError += HandleError;
            webSocket.OnClosed += HandleDisconnected;
            webSocket.Open();

            if (enableDebugLog)
                Debug.Log("Connecting to server: " + serverAddress);
        }

        private void CleanupWebSocket()
        {
            if (webSocket != null)
            {
                webSocket.OnOpen -= HandleConnected;
                webSocket.OnMessage -= HandleMessageReceived;
                webSocket.OnError -= HandleError;
                webSocket.OnClosed -= HandleDisconnected;

                if (webSocket.IsOpen)
                    webSocket.Close();

                webSocket = null;
            }
        }

        public void Disconnect()
        {
            CleanupWebSocket();

            if (enableDebugLog)
                Debug.Log("Disconnected from server");
        }

        public void SendMessage(NetworkMessage message)
        {
            if (webSocket != null && webSocket.IsOpen)
            {
                webSocket.Send(message.Serialize());
            }
        }

        private void HandleConnected(BestHTTP.WebSocket.WebSocket ws)
        {
            if (enableDebugLog)
                Debug.Log("Connected to server");

            NetworkMessage connectMsg = new NetworkMessage
            {
                type = MessageType.Connect,
                data = new byte[0]
            };

            SendMessage(connectMsg);

            OnConnected?.Invoke();
        }

        private void HandleMessageReceived(BestHTTP.WebSocket.WebSocket ws, string message)
        {
            HandleMessageReceived(ws, Encoding.UTF8.GetBytes(message));
        }

        private void HandleMessageReceived(BestHTTP.WebSocket.WebSocket ws, byte[] message)
        {
            try
            {
                NetworkMessage netMsg = NetworkMessage.Deserialize(message);
                
                if (netMsg.type == MessageType.ConnectResponse)
                {
                    playerId = Encoding.UTF8.GetString(netMsg.data);
                    if (enableDebugLog)
                        Debug.Log("Player ID received: " + playerId);
                }

                OnMessageReceived?.Invoke(netMsg);
            }
            catch (Exception ex)
            {
                if (enableDebugLog)
                    Debug.LogError("Error processing message: " + ex.Message);
            }
        }

        private void HandleError(BestHTTP.WebSocket.WebSocket ws, Exception ex)
        {
            if (enableDebugLog)
                Debug.LogError("WebSocket error: " + ex.Message);
        }

        private void HandleDisconnected(BestHTTP.WebSocket.WebSocket ws, ushort code, string message)
        {
            if (enableDebugLog)
                Debug.Log("Disconnected from server: " + message);

            OnDisconnected?.Invoke();
        }

        void OnDestroy()
        {
            Disconnect();
        }
    }
}
