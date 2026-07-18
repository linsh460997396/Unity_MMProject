using BestHTTP.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace CellSpace.WebSocket
{
    public class WebSocketClient : MonoBehaviour
    {
        public string serverAddress = "ws://localhost:8765";
        public bool enableDebugLog = false;
        
        // 断线重连配置
        public float reconnectDelay = 5f;
        public int maxReconnectAttempts = 5;
        
        // Ping配置
        public float pingInterval = 5f;
        public float pingTimeout = 10f;
        
        // 消息队列配置
        public int maxQueueSize = 1000;
        public int sendBatchSize = 10;
        public float sendInterval = 0.1f;

        private BestHTTP.WebSocket.WebSocket webSocket;
        private string playerId;
        
        // 断线重连相关
        private float reconnectTimer;
        private int reconnectAttempts;
        private bool isReconnecting;
        
        // Ping相关
        private float pingTimer;
        private Dictionary<int, float> pingTimestamps = new Dictionary<int, float>();
        private int pingSequence = 0;
        private float currentPing;
        
        // 消息队列
        private Queue<NetworkMessage> messageQueue = new Queue<NetworkMessage>();
        private float sendTimer;
        
        // 内存优化:复用的 MemoryStream
        private MemoryStream reusableStream = new MemoryStream(256);
        
        // 区块请求去重
        private HashSet<Vector3Int> pendingChunkRequests = new HashSet<Vector3Int>();

        public static WebSocketClient Instance { get; private set; }

        public bool IsConnected => webSocket != null && webSocket.IsOpen;
        public float CurrentPing => currentPing;

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
            // 处理断线重连
            HandleReconnect();
            
            // 处理Ping
            HandlePing();
            
            // 处理Ping超时
            HandlePingTimeout();
            
            // 处理消息队列
            HandleMessageQueue();
        }

        private void HandleReconnect()
        {
            if (!IsConnected && !isReconnecting && reconnectAttempts < maxReconnectAttempts)
            {
                reconnectTimer += Time.deltaTime;
                float currentDelay = GetCurrentReconnectDelay();
                
                if (reconnectTimer >= currentDelay)
                {
                    reconnectTimer = 0;
                    reconnectAttempts++;
                    isReconnecting = true;
                    
                    if (enableDebugLog)
                        Debug.Log($"Attempting reconnect ({reconnectAttempts}/{maxReconnectAttempts}), delay: {currentDelay:F1}s");
                    
                    Connect();
                }
            }
        }

        private float GetCurrentReconnectDelay()
        {
            // 指数退避:5s, 10s, 20s, 40s, 80s...
            return reconnectDelay * Mathf.Pow(2, reconnectAttempts);
        }

        private void HandlePing()
        {
            if (IsConnected)
            {
                pingTimer += Time.deltaTime;
                if (pingTimer >= pingInterval)
                {
                    pingTimer = 0;
                    SendPing();
                }
            }
        }

        private void HandlePingTimeout()
        {
            if (pingTimestamps.Count == 0) return;
            
            var toRemove = new List<int>();
            
            foreach (var kvp in pingTimestamps)
            {
                if (Time.time - kvp.Value > pingTimeout)
                    toRemove.Add(kvp.Key);
            }
            
            foreach (var seq in toRemove)
            {
                pingTimestamps.Remove(seq);
                if (enableDebugLog)
                    Debug.Log($"Ping {seq} timed out");
            }
        }

        private void HandleMessageQueue()
        {
            if (IsConnected)
            {
                sendTimer += Time.deltaTime;
                if (sendTimer >= sendInterval && messageQueue.Count > 0)
                {
                    sendTimer = 0;
                    SendMessageBatch();
                }
            }
        }

        private void SendMessageBatch()
        {
            int count = Math.Min(sendBatchSize, messageQueue.Count);
            for (int i = 0; i < count; i++)
            {
                NetworkMessage msg = messageQueue.Dequeue();
                SendMessageDirect(msg);
            }
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
            webSocket.OnOpen += OnConnected;
            webSocket.OnMessage += OnMessageReceived;
            webSocket.OnError += OnError;
            webSocket.OnClosed += OnDisconnected;
            webSocket.Open();

            if (enableDebugLog)
                Debug.Log("Connecting to server: " + serverAddress);
        }

        private void CleanupWebSocket()
        {
            if (webSocket != null)
            {
                webSocket.OnOpen -= OnConnected;
                webSocket.OnMessage -= OnMessageReceived;
                webSocket.OnError -= OnError;
                webSocket.OnClosed -= OnDisconnected;
                
                if (webSocket.IsOpen)
                    webSocket.Close();
                    
                webSocket = null;
            }
        }

        public void Disconnect()
        {
            CleanupWebSocket();
            isReconnecting = false;
            pendingChunkRequests.Clear();

            if (enableDebugLog)
                Debug.Log("Disconnected from server");
        }

        private void OnConnected(BestHTTP.WebSocket.WebSocket ws)
        {
            reconnectAttempts = 0;
            isReconnecting = false;
            
            if (enableDebugLog)
                Debug.Log("Connected to server");

            CPEngine.saveCellData = false;

            NetworkMessage connectMsg = new NetworkMessage
            {
                type = MessageType.Connect,
                data = new byte[0]
            };

            SendMessage(connectMsg);
            
            // 重连成功后请求全量状态同步
            RequestFullStateSync();
        }

        private void RequestFullStateSync()
        {
            NetworkMessage msg = new NetworkMessage
            {
                type = MessageType.RequestFullState,
                data = new byte[0]
            };
            SendMessage(msg);
            
            // 请求当前视野范围内的区块数据
            RequestSurroundingChunks();
        }

        private void RequestSurroundingChunks()
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                Vector3 cameraPos = mainCam.transform.position;
                CPIndex currentChunk = CPEngine.PositionToChunkIndex(cameraPos);
                int viewRange = CPEngine.chunkSpawnDistance;
                
                for (int dx = -viewRange; dx <= viewRange; dx++)
                {
                    for (int dy = -viewRange; dy <= viewRange; dy++)
                    {
                        int dz = CPEngine.horizontalMode ? 0 : 0;
                        SendRequestCellData(currentChunk.x + dx, currentChunk.y + dy, currentChunk.z + dz);
                    }
                }
            }
        }

        private void OnMessageReceived(BestHTTP.WebSocket.WebSocket ws, string message)
        {
            OnMessageReceived(ws, Encoding.UTF8.GetBytes(message));
        }

        private void OnMessageReceived(BestHTTP.WebSocket.WebSocket ws, byte[] message)
        {
            // 记录接收数据统计
            NetworkMonitor.Instance?.RecordBytesReceived(message.Length);
            
            try
            {
                NetworkMessage netMsg = NetworkMessage.Deserialize(message);

                switch (netMsg.type)
                {
                    case MessageType.ConnectResponse:
                        HandleConnectResponse(netMsg.data);
                        break;

                    case MessageType.SendCellData:
                        HandleCellData(netMsg.data);
                        break;

                    case MessageType.SendCellDelta:
                        HandleCellDelta(netMsg.data);
                        break;

                    case MessageType.PlaceBlock:
                        HandlePlaceBlock(netMsg.data);
                        break;

                    case MessageType.ChangeBlock:
                        HandleChangeBlock(netMsg.data);
                        break;

                    case MessageType.DestroyBlock:
                        HandleDestroyBlock(netMsg.data);
                        break;

                    case MessageType.Pong:
                        HandlePong(netMsg.data);
                        break;
                }
            }
            catch (Exception ex)
            {
                if (enableDebugLog)
                    Debug.LogError("Error processing message: " + ex.Message);
            }
        }

        private void OnError(BestHTTP.WebSocket.WebSocket ws, Exception ex)
        {
            if (enableDebugLog)
                Debug.LogError("WebSocket error: " + ex.Message);
        }

        private void OnDisconnected(BestHTTP.WebSocket.WebSocket ws, ushort code, string message)
        {
            if (enableDebugLog)
                Debug.Log("Disconnected from server: " + message);

            // 不要销毁对象,保留用于重连
        }

        private void HandleConnectResponse(byte[] data)
        {
            playerId = Encoding.UTF8.GetString(data);
            if (enableDebugLog)
                Debug.Log("Player ID received: " + playerId);
        }

        private void HandleCellData(byte[] data)
        {
            ChunkDataResponse response = ChunkDataResponse.Deserialize(data);
            
            // 移除待处理请求
            var key = new Vector3Int(response.chunkX, response.chunkY, response.chunkZ);
            pendingChunkRequests.Remove(key);

            GameObject chunkObject = null;
            if (CPEngine.horizontalMode)
            {
                chunkObject = CellChunkManager.GetChunk(response.chunkX, response.chunkY);
            }
            else
            {
                chunkObject = CellChunkManager.GetChunk(response.chunkX, response.chunkY, response.chunkZ);
            }

            if (chunkObject != null)
            {
                CellChunk chunk = chunkObject.GetComponent<CellChunk>();
                string decompressedData = Encoding.UTF8.GetString(response.compressedData);
                CellChunkDataFiles.DecompressData(chunk, decompressedData);
                chunk.cellsDone = true;
                CellChunk.CurrentChunkDataRequests--;
            }

            if (enableDebugLog)
                Debug.Log("Received cell data for chunk: " + response.chunkX + "," + response.chunkY + "," + response.chunkZ);
        }

        private void HandleCellDelta(byte[] data)
        {
            ChunkDelta delta = ChunkDelta.Deserialize(data);

            GameObject chunkObject = null;
            if (CPEngine.horizontalMode)
            {
                chunkObject = CellChunkManager.GetChunk(delta.chunkX, delta.chunkY);
            }
            else
            {
                chunkObject = CellChunkManager.GetChunk(delta.chunkX, delta.chunkY, delta.chunkZ);
            }

            if (chunkObject != null)
            {
                CellChunk chunk = chunkObject.GetComponent<CellChunk>();
                foreach (var change in delta.changes)
                {
                    CPIndex voxelIndex = new CPIndex(change.x, change.y, change.z);
                    chunk.SetCellSimple(voxelIndex, change.blockId);
                }
                chunk.cellsDone = true;
            }

            if (enableDebugLog)
                Debug.Log("Received cell delta for chunk: " + delta.chunkX + "," + delta.chunkY + "," + delta.chunkZ);
        }

        private void HandlePlaceBlock(byte[] data)
        {
            BlockData blockData = BlockData.Deserialize(data);
            ApplyBlockChange(blockData, false);
        }

        private void HandleChangeBlock(byte[] data)
        {
            BlockData blockData = BlockData.Deserialize(data);
            ApplyBlockChange(blockData, true);
        }

        private void HandleDestroyBlock(byte[] data)
        {
            BlockData blockData = BlockData.Deserialize(data);
            blockData.blockId = 0;
            ApplyBlockChange(blockData, false);
        }

        private void HandlePong(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            using (BinaryReader reader = new BinaryReader(ms))
            {
                int sequence = reader.ReadInt32();

                if (pingTimestamps.TryGetValue(sequence, out float sendTime))
                {
                    float latency = (Time.time - sendTime) * 1000;
                    currentPing = latency;
                    pingTimestamps.Remove(sequence);

                    if (enableDebugLog)
                        Debug.Log($"Ping: {currentPing:F1} ms");
                }
            }
        }

        private void ApplyBlockChange(BlockData blockData, bool isChangeBlock)
        {
            GameObject chunkObject = null;
            if (CPEngine.horizontalMode)
            {
                chunkObject = CellChunkManager.GetChunk(blockData.chunkX, blockData.chunkY);
            }
            else
            {
                chunkObject = CellChunkManager.GetChunk(blockData.chunkX, blockData.chunkY, blockData.chunkZ);
            }

            if (chunkObject != null)
            {
                CPIndex voxelIndex = new CPIndex(blockData.x, blockData.y, blockData.z);
                CellInfo info = new CellInfo(voxelIndex, chunkObject.GetComponent<CellChunk>());

                if (isChangeBlock)
                {
                    Cell.ChangeBlockMultiplayerWebSocket(info, blockData.blockId, blockData.playerId);
                }
                else
                {
                    if (blockData.blockId == 0)
                    {
                        Cell.DestroyBlockMultiplayerWebSocket(info, blockData.playerId);
                    }
                    else
                    {
                        Cell.PlaceBlockMultiplayerWebSocket(info, blockData.blockId, blockData.playerId);
                    }
                }
            }
        }

        private void SendPing()
        {
            int sequence = pingSequence++;
            pingTimestamps[sequence] = Time.time;

            // 使用复用的 MemoryStream
            reusableStream.SetLength(0);
            using (BinaryWriter writer = new BinaryWriter(reusableStream, Encoding.UTF8, leaveOpen: true))
            {
                writer.Write(sequence);
            }

            NetworkMessage msg = new NetworkMessage
            {
                type = MessageType.Ping,
                data = reusableStream.ToArray()
            };

            SendMessage(msg);
        }

        public void SendRequestCellData(int chunkX, int chunkY, int chunkZ)
        {
            var key = new Vector3Int(chunkX, chunkY, chunkZ);
            
            // 去重检查
            if (pendingChunkRequests.Contains(key))
            {
                if (enableDebugLog)
                    Debug.Log($"Chunk request already pending: {chunkX},{chunkY},{chunkZ}");
                return;
            }
            
            pendingChunkRequests.Add(key);
            
            ChunkDataRequest request = new ChunkDataRequest
            {
                playerId = playerId ?? "",
                chunkX = chunkX,
                chunkY = chunkY,
                chunkZ = chunkZ
            };

            NetworkMessage msg = new NetworkMessage
            {
                type = MessageType.RequestCellData,
                data = request.Serialize()
            };

            SendMessage(msg);
        }

        public void SendPlaceBlock(CellInfo info, ushort blockId)
        {
            BlockData blockData = CreateBlockData(info, blockId);

            NetworkMessage msg = new NetworkMessage
            {
                type = MessageType.PlaceBlock,
                data = blockData.Serialize()
            };

            SendMessage(msg);
        }

        public void SendChangeBlock(CellInfo info, ushort blockId)
        {
            BlockData blockData = CreateBlockData(info, blockId);

            NetworkMessage msg = new NetworkMessage
            {
                type = MessageType.ChangeBlock,
                data = blockData.Serialize()
            };

            SendMessage(msg);
        }

        public void SendDestroyBlock(CellInfo info)
        {
            BlockData blockData = CreateBlockData(info, 0);

            NetworkMessage msg = new NetworkMessage
            {
                type = MessageType.DestroyBlock,
                data = blockData.Serialize()
            };

            SendMessage(msg);
        }

        public void SendUpdatePlayerPosition(int chunkX, int chunkY, int chunkZ)
        {
            // 使用复用的 MemoryStream
            reusableStream.SetLength(0);
            using (BinaryWriter writer = new BinaryWriter(reusableStream, Encoding.UTF8, leaveOpen: true))
            {
                writer.Write(chunkX);
                writer.Write(chunkY);
                writer.Write(chunkZ);
            }

            NetworkMessage msg = new NetworkMessage
            {
                type = MessageType.UpdatePlayerPosition,
                data = reusableStream.ToArray()
            };

            SendMessage(msg);
        }

        public void SendUpdatePlayerRange(int range)
        {
            NetworkMessage msg = new NetworkMessage
            {
                type = MessageType.UpdatePlayerRange,
                data = BitConverter.GetBytes(range)
            };

            SendMessage(msg);
        }

        private BlockData CreateBlockData(CellInfo info, ushort blockId)
        {
            return new BlockData
            {
                playerId = playerId ?? "",
                x = info.index.x,
                y = info.index.y,
                z = CPEngine.horizontalMode ? 0 : info.index.z,
                chunkX = info.chunk.chunkIndex.x,
                chunkY = info.chunk.chunkIndex.y,
                chunkZ = CPEngine.horizontalMode ? 0 : info.chunk.chunkIndex.z,
                blockId = blockId
            };
        }

        public void SendMessage(NetworkMessage message)
        {
            if (IsConnected)
            {
                if (messageQueue.Count < maxQueueSize)
                {
                    messageQueue.Enqueue(message);
                }
                else
                {
                    if (enableDebugLog)
                        Debug.LogWarning("Network queue full, dropping message");
                }
            }
            else
            {
                // 未连接时也加入队列,等待重连后发送
                if (messageQueue.Count < maxQueueSize)
                {
                    messageQueue.Enqueue(message);
                }
            }
        }

        private void SendMessageDirect(NetworkMessage message)
        {
            if (webSocket != null && webSocket.IsOpen)
            {
                byte[] data = message.Serialize();
                webSocket.Send(data);
                NetworkMonitor.Instance?.RecordBytesSent(data.Length);
            }
        }

        void OnDestroy()
        {
            Disconnect();
            if (reusableStream != null)
            {
                reusableStream.Dispose();
            }
        }
    }
}