using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using UnityEngine;
using CellSpace;

namespace CellSpace.WebSocket
{
    public class WebSocketServer : MonoBehaviour
    {
        public int port = 8765;
        public bool enableDebugLog = false;
        public float autosaveTime = 0f;
        
        // 连接配置
        public int maxConnections = 100;
        
        // 流量控制配置
        public int maxMessagesPerSecond = 100;
        
        // Ping配置
        public float pingTimeout = 30f;

        private TcpListener tcpListener;
        private Thread listenThread;
        private List<WebSocketConnection> connections = new List<WebSocketConnection>();
        private float autosaveTimer;
        private Dictionary<string, PlayerInfo> playerPositions = new Dictionary<string, PlayerInfo>();
        
        // 流量控制
        private Dictionary<string, int> messageCounts = new Dictionary<string, int>();
        private float messageCountTimer;
        
        // Ping超时追踪
        private Dictionary<string, float> pingTimers = new Dictionary<string, float>();

        public static WebSocketServer Instance { get; private set; }

        public int ConnectionCount => connections.Count;

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
            // 自动保存
            if (autosaveTime > 0.0001f)
            {
                autosaveTimer += Time.deltaTime;
                if (autosaveTimer >= autosaveTime)
                {
                    autosaveTimer = 0;
                    CPEngine.SaveWorld();
                }
            }
            
            // 流量控制计时器重置
            messageCountTimer += Time.deltaTime;
            if (messageCountTimer >= 1f)
            {
                messageCountTimer = 0;
                messageCounts.Clear();
            }
            
            // Ping超时检测
            CheckPingTimeouts();
        }

        private void CheckPingTimeouts()
        {
            List<string> timedOutPlayers = new List<string>();
            
            foreach (var kvp in pingTimers)
            {
                pingTimers[kvp.Key] += Time.deltaTime;
                if (pingTimers[kvp.Key] >= pingTimeout)
                {
                    timedOutPlayers.Add(kvp.Key);
                    if (enableDebugLog)
                        Debug.LogWarning($"Player {kvp.Key} timed out");
                }
            }
            
            foreach (string playerId in timedOutPlayers)
            {
                pingTimers.Remove(playerId);
                // 可以在这里添加断开连接或其他处理逻辑
            }
        }

        public void StartServer()
        {
            if (tcpListener == null)
            {
                tcpListener = new TcpListener(IPAddress.Any, port);
                tcpListener.Start();
                listenThread = new Thread(new ThreadStart(ListenForClients));
                listenThread.IsBackground = true;
                listenThread.Start();

                if (enableDebugLog)
                    Debug.Log("WebSocket Server started on port " + port);
            }
        }

        public void StopServer()
        {
            if (tcpListener != null)
            {
                tcpListener.Stop();
                tcpListener = null;

                foreach (var conn in connections.ToArray())
                {
                    conn.Close();
                }
                connections.Clear();

                if (enableDebugLog)
                    Debug.Log("WebSocket Server stopped");
            }
        }

        private void ListenForClients()
        {
            while (tcpListener != null)
            {
                try
                {
                    TcpClient client = tcpListener.AcceptTcpClient();
                    
                    // 检查连接数限制
                    lock (connections)
                    {
                        if (connections.Count >= maxConnections)
                        {
                            if (enableDebugLog)
                                Debug.LogWarning($"Max connections reached ({maxConnections}), rejecting new client");
                            client.Close();
                            continue;
                        }
                    }
                    
                    WebSocketConnection connection = new WebSocketConnection(client, this);

                    lock (connections)
                    {
                        connections.Add(connection);
                    }

                    if (enableDebugLog)
                        Debug.Log("Client connected: " + client.Client.RemoteEndPoint);
                }
                catch (SocketException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    if (enableDebugLog)
                        Debug.LogError("Error accepting client: " + ex.Message);
                }
            }
        }

        internal void OnClientConnected(WebSocketConnection connection)
        {
            if (enableDebugLog)
                Debug.Log("Client connected: " + connection.RemoteEndPoint);
            
            // 初始化Ping计时器
            pingTimers[connection.PlayerId] = 0f;
        }

        internal void OnClientDisconnected(WebSocketConnection connection, string reason)
        {
            string playerId = connection.PlayerId;
            if (playerPositions.ContainsKey(playerId))
            {
                playerPositions.Remove(playerId);
            }
            
            if (pingTimers.ContainsKey(playerId))
            {
                pingTimers.Remove(playerId);
            }

            lock (connections)
            {
                connections.Remove(connection);
            }

            if (enableDebugLog)
                Debug.Log("Client disconnected: " + connection.RemoteEndPoint + ", reason: " + reason);
        }

        internal void OnMessageReceived(WebSocketConnection connection, byte[] message)
        {
            // 记录接收数据统计
            NetworkMonitor.Instance?.RecordBytesReceived(message.Length);
            
            string playerId = connection.PlayerId;

            // 检查消息频率
            if (!messageCounts.ContainsKey(playerId))
                messageCounts[playerId] = 0;

            messageCounts[playerId]++;

            if (messageCounts[playerId] > maxMessagesPerSecond)
            {
                if (enableDebugLog)
                    Debug.LogWarning($"Player {playerId} exceeding message rate limit");
                return;
            }
            
            // 更新Ping计时器
            if (pingTimers.ContainsKey(playerId))
            {
                pingTimers[playerId] = 0f;
            }

            try
            {
                NetworkMessage netMsg = NetworkMessage.Deserialize(message);

                switch (netMsg.type)
                {
                    case MessageType.Connect:
                        HandleConnect(connection, playerId);
                        break;

                    case MessageType.RequestCellData:
                        HandleRequestCellData(connection, netMsg.data);
                        break;

                    case MessageType.PlaceBlock:
                        HandlePlaceBlock(connection, netMsg.data);
                        break;

                    case MessageType.ChangeBlock:
                        HandleChangeBlock(connection, netMsg.data);
                        break;

                    case MessageType.DestroyBlock:
                        HandleDestroyBlock(connection, netMsg.data);
                        break;

                    case MessageType.UpdatePlayerPosition:
                        HandleUpdatePlayerPosition(connection, playerId, netMsg.data);
                        break;

                    case MessageType.UpdatePlayerRange:
                        HandleUpdatePlayerRange(connection, playerId, netMsg.data);
                        break;

                    case MessageType.RequestFullState:
                        HandleRequestFullState(connection, playerId);
                        break;

                    case MessageType.Ping:
                        HandlePing(connection, netMsg.data);
                        break;
                }
            }
            catch (Exception ex)
            {
                if (enableDebugLog)
                    Debug.LogError("Error processing message: " + ex.Message);
            }
        }

        private void HandleConnect(WebSocketConnection connection, string playerId)
        {
            PlayerInfo info = new PlayerInfo
            {
                playerId = playerId,
                chunkX = 0,
                chunkY = 0,
                chunkZ = 0,
                viewRange = CPEngine.chunkSpawnDistance
            };

            if (!playerPositions.ContainsKey(playerId))
            {
                playerPositions.Add(playerId, info);
            }

            NetworkMessage response = new NetworkMessage
            {
                type = MessageType.ConnectResponse,
                data = Encoding.UTF8.GetBytes(playerId)
            };

            SendMessage(connection, response);

            if (enableDebugLog)
                Debug.Log("Player connected: " + playerId);
        }

        private void HandleRequestCellData(WebSocketConnection connection, byte[] data)
        {
            ChunkDataRequest request = ChunkDataRequest.Deserialize(data);

            CellChunk chunk = null;
            if (CPEngine.horizontalMode)
            {
                chunk = CellChunkManager.SpawnChunkFromServer(request.chunkX, request.chunkY).GetComponent<CellChunk>();
            }
            else
            {
                chunk = CellChunkManager.SpawnChunkFromServer(request.chunkX, request.chunkY, request.chunkZ).GetComponent<CellChunk>();
            }

            chunk.lifetime = 0f;
            string compressedData = CellChunkDataFiles.CompressData(chunk);
            byte[] dataBytes = System.Text.Encoding.UTF8.GetBytes(compressedData);

            ChunkDataResponse response = new ChunkDataResponse
            {
                chunkX = request.chunkX,
                chunkY = request.chunkY,
                chunkZ = request.chunkZ,
                compressedData = dataBytes
            };

            NetworkMessage msg = new NetworkMessage
            {
                type = MessageType.SendCellData,
                data = response.Serialize()
            };

            SendMessage(connection, msg);

            if (enableDebugLog)
                Debug.Log("Sent cell data for chunk: " + request.chunkX + "," + request.chunkY + "," + request.chunkZ);
        }

        private void HandlePlaceBlock(WebSocketConnection connection, byte[] data)
        {
            BlockData blockData = BlockData.Deserialize(data);
            DistributeBlockChange(blockData, false);
        }

        private void HandleChangeBlock(WebSocketConnection connection, byte[] data)
        {
            BlockData blockData = BlockData.Deserialize(data);
            DistributeBlockChange(blockData, true);
        }

        private void HandleDestroyBlock(WebSocketConnection connection, byte[] data)
        {
            BlockData blockData = BlockData.Deserialize(data);
            blockData.blockId = 0;
            DistributeBlockChange(blockData, false);
        }

        private void DistributeBlockChange(BlockData blockData, bool isChangeBlock)
        {
            ApplyOnServer(blockData, isChangeBlock);

            MessageType msgType = isChangeBlock ? MessageType.ChangeBlock : MessageType.PlaceBlock;

            List<WebSocketConnection> conns;
            lock (connections)
            {
                conns = connections.ToList();
            }

            foreach (var conn in conns)
            {
                if (conn.IsOpen)
                {
                    string playerId = conn.PlayerId;

                    if (CPEngine.multiplayerTrackPosition)
                    {
                        if (playerPositions.TryGetValue(playerId, out PlayerInfo info))
                        {
                            CPIndex chunkIndex = new CPIndex(blockData.chunkX, blockData.chunkY, blockData.chunkZ);
                            if (!IsWithinRange(info, chunkIndex))
                                continue;
                        }
                    }

                    NetworkMessage msg = new NetworkMessage
                    {
                        type = msgType,
                        data = blockData.Serialize()
                    };

                    SendMessage(conn, msg);
                }
            }
        }

        private void ApplyOnServer(BlockData blockData, bool isChangeBlock)
        {
            CellChunk chunk = null;
            if (CPEngine.horizontalMode)
            {
                chunk = CellChunkManager.SpawnChunkFromServer(blockData.chunkX, blockData.chunkY).GetComponent<CellChunk>();
            }
            else
            {
                chunk = CellChunkManager.SpawnChunkFromServer(blockData.chunkX, blockData.chunkY, blockData.chunkZ).GetComponent<CellChunk>();
            }

            chunk.lifetime = 0f;

            CPIndex voxelIndex = new CPIndex(blockData.x, blockData.y, blockData.z);
            CellInfo info = new CellInfo(voxelIndex, chunk);

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

        private void HandleUpdatePlayerPosition(WebSocketConnection connection, string playerId, byte[] data)
        {
            if (playerPositions.TryGetValue(playerId, out PlayerInfo info))
            {
                using (System.IO.MemoryStream ms = new System.IO.MemoryStream(data))
                using (System.IO.BinaryReader reader = new System.IO.BinaryReader(ms))
                {
                    info.chunkX = reader.ReadInt32();
                    info.chunkY = reader.ReadInt32();
                    info.chunkZ = reader.ReadInt32();
                }
                playerPositions[playerId] = info;

                if (enableDebugLog)
                    Debug.Log("Player position updated: " + playerId + " -> " + info.chunkX + "," + info.chunkY + "," + info.chunkZ);
            }
        }

        private void HandleUpdatePlayerRange(WebSocketConnection connection, string playerId, byte[] data)
        {
            if (playerPositions.TryGetValue(playerId, out PlayerInfo info))
            {
                int range = BitConverter.ToInt32(data, 0);
                info.viewRange = range;
                playerPositions[playerId] = info;

                if (enableDebugLog)
                    Debug.Log("Player range updated: " + playerId + " -> " + info.viewRange);
            }
        }

        private void HandleRequestFullState(WebSocketConnection connection, string playerId)
        {
            if (enableDebugLog)
                Debug.Log("Player " + playerId + " requested full state sync");
            
            // 可以在这里实现全量状态同步逻辑
        }

        private void HandlePing(WebSocketConnection connection, byte[] data)
        {
            NetworkMessage msg = new NetworkMessage
            {
                type = MessageType.Pong,
                data = data
            };
            SendMessage(connection, msg);
        }

        private bool IsWithinRange(PlayerInfo player, CPIndex chunkIndex)
        {
            if (Mathf.Abs(player.chunkX - chunkIndex.x) > player.viewRange)
                return false;
            if (Mathf.Abs(player.chunkY - chunkIndex.y) > player.viewRange)
                return false;
            if (!CPEngine.horizontalMode)
            {
                if (Mathf.Abs(player.chunkZ - chunkIndex.z) > player.viewRange)
                    return false;
            }
            return true;
        }

        private void SendMessage(WebSocketConnection connection, NetworkMessage message)
        {
            if (connection.IsOpen)
            {
                byte[] data = message.Serialize();
                connection.Send(data);
                NetworkMonitor.Instance?.RecordBytesSent(data.Length);
            }
        }

        void OnDestroy()
        {
            StopServer();
        }

        internal class WebSocketConnection
        {
            private TcpClient client;
            private NetworkStream stream;
            private Thread readThread;
            private byte[] buffer = new byte[4096];
            private byte[] frameBuffer = new byte[0];
            private WebSocketServer server;
            private volatile bool isOpen = false;
            
            private byte[] fragmentedBuffer = new byte[0];
            private byte fragmentedOpcode = 0;

            public string PlayerId { get; private set; }
            public EndPoint RemoteEndPoint => client?.Client?.RemoteEndPoint;
            public bool IsOpen => isOpen && client?.Connected == true;

            public WebSocketConnection(TcpClient client, WebSocketServer server)
            {
                this.client = client;
                this.server = server;
                this.PlayerId = client.Client.RemoteEndPoint.ToString();
                this.stream = client.GetStream();

                PerformHandshake();
            }

            private void PerformHandshake()
            {
                try
                {
                    byte[] handshakeBuffer = new byte[1024];
                    int bytesRead = stream.Read(handshakeBuffer, 0, handshakeBuffer.Length);
                    string handshake = Encoding.ASCII.GetString(handshakeBuffer, 0, bytesRead);

                    string key = ExtractKey(handshake);
                    string responseKey = GenerateResponseKey(key);

                    string response = string.Format(
                        "HTTP/1.1 101 Switching Protocols\r\n" +
                        "Upgrade: websocket\r\n" +
                        "Connection: Upgrade\r\n" +
                        "Sec-WebSocket-Accept: {0}\r\n\r\n",
                        responseKey);

                    byte[] responseBytes = Encoding.ASCII.GetBytes(response);
                    stream.Write(responseBytes, 0, responseBytes.Length);
                    stream.Flush();

                    isOpen = true;
                    server.OnClientConnected(this);

                    readThread = new Thread(new ThreadStart(ReadLoop));
                    readThread.IsBackground = true;
                    readThread.Start();
                }
                catch (Exception ex)
                {
                    Close();
                    server.OnClientDisconnected(this, ex.Message);
                }
            }

            private string ExtractKey(string handshake)
            {
                int start = handshake.IndexOf("Sec-WebSocket-Key: ") + 19;
                int end = handshake.IndexOf("\r\n", start);
                return handshake.Substring(start, end - start).Trim();
            }

            private string GenerateResponseKey(string key)
            {
                string guid = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
                string combined = key + guid;

                using (SHA1 sha1 = SHA1.Create())
                {
                    byte[] hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(combined));
                    return Convert.ToBase64String(hash);
                }
            }

            private void ReadLoop()
            {
                try
                {
                    while (isOpen && client.Connected)
                    {
                        int bytesRead = stream.Read(buffer, 0, buffer.Length);
                        if (bytesRead == 0)
                            break;

                        byte[] newData = new byte[frameBuffer.Length + bytesRead];
                        Buffer.BlockCopy(frameBuffer, 0, newData, 0, frameBuffer.Length);
                        Buffer.BlockCopy(buffer, 0, newData, frameBuffer.Length, bytesRead);
                        frameBuffer = newData;

                        ProcessFrames();
                    }
                }
                catch (Exception)
                {
                }
                finally
                {
                    Close();
                }
            }

            private void ProcessFrames()
            {
                while (frameBuffer.Length >= 2)
                {
                    bool fin = (frameBuffer[0] & 0x80) != 0;
                    byte opcode = (byte)(frameBuffer[0] & 0x0F);

                    if (opcode == 0x08)
                    {
                        Close();
                        return;
                    }

                    bool mask = (frameBuffer[1] & 0x80) != 0;
                    int payloadLength = frameBuffer[1] & 0x7F;

                    int offset = 2;
                    byte[] maskKey = new byte[4];

                    if (payloadLength == 126)
                    {
                        if (frameBuffer.Length < 4) return;
                        payloadLength = (frameBuffer[2] << 8) | frameBuffer[3];
                        offset = 4;
                    }
                    else if (payloadLength == 127)
                    {
                        if (frameBuffer.Length < 10) return;
                        payloadLength = (frameBuffer[2] << 24) | (frameBuffer[3] << 16) | (frameBuffer[4] << 8) | frameBuffer[5];
                        offset = 10;
                    }

                    if (mask)
                    {
                        if (frameBuffer.Length < offset + 4) return;
                        Buffer.BlockCopy(frameBuffer, offset, maskKey, 0, 4);
                        offset += 4;
                    }

                    if (frameBuffer.Length < offset + payloadLength) return;

                    byte[] payload = new byte[payloadLength];
                    Buffer.BlockCopy(frameBuffer, offset, payload, 0, payloadLength);

                    if (mask)
                    {
                        for (int i = 0; i < payloadLength; i++)
                        {
                            payload[i] = (byte)(payload[i] ^ maskKey[i % 4]);
                        }
                    }

                    if (opcode == 0x00 && fragmentedBuffer.Length > 0)
                    {
                        byte[] combined = new byte[fragmentedBuffer.Length + payload.Length];
                        Buffer.BlockCopy(fragmentedBuffer, 0, combined, 0, fragmentedBuffer.Length);
                        Buffer.BlockCopy(payload, 0, combined, fragmentedBuffer.Length, payload.Length);
                        
                        if (fin)
                        {
                            if (fragmentedOpcode == 0x01 || fragmentedOpcode == 0x02)
                            {
                                server.OnMessageReceived(this, combined);
                            }
                            fragmentedBuffer = new byte[0];
                            fragmentedOpcode = 0;
                        }
                        else
                        {
                            fragmentedBuffer = combined;
                        }
                    }
                    else if (opcode == 0x01 || opcode == 0x02)
                    {
                        if (fin)
                        {
                            server.OnMessageReceived(this, payload);
                        }
                        else
                        {
                            fragmentedBuffer = payload;
                            fragmentedOpcode = opcode;
                        }
                    }

                    int frameSize = offset + payloadLength;
                    byte[] remaining = new byte[frameBuffer.Length - frameSize];
                    Buffer.BlockCopy(frameBuffer, frameSize, remaining, 0, remaining.Length);
                    frameBuffer = remaining;
                }
            }

            public void Send(byte[] data)
            {
                try
                {
                    if (!isOpen || !client.Connected)
                        return;

                    byte[] frame;

                    if (data.Length <= 125)
                    {
                        frame = new byte[data.Length + 2];
                        frame[0] = 0x82;
                        frame[1] = (byte)data.Length;
                        Buffer.BlockCopy(data, 0, frame, 2, data.Length);
                    }
                    else if (data.Length <= 65535)
                    {
                        frame = new byte[data.Length + 4];
                        frame[0] = 0x82;
                        frame[1] = 126;
                        frame[2] = (byte)(data.Length >> 8);
                        frame[3] = (byte)(data.Length & 0xFF);
                        Buffer.BlockCopy(data, 0, frame, 4, data.Length);
                    }
                    else
                    {
                        frame = new byte[data.Length + 10];
                        frame[0] = 0x82;
                        frame[1] = 127;
                        for (int i = 7; i >= 0; i--)
                        {
                            frame[2 + i] = (byte)(data.Length >> (8 * (7 - i)));
                        }
                        Buffer.BlockCopy(data, 0, frame, 10, data.Length);
                    }

                    stream.Write(frame, 0, frame.Length);
                    stream.Flush();
                }
                catch (Exception)
                {
                    Close();
                }
            }

            public void Close()
            {
                isOpen = false;

                try
                {
                    stream?.Close();
                }
                catch { }

                try
                {
                    client?.Close();
                }
                catch { }

                server.OnClientDisconnected(this, "Closed");
            }
        }
    }
}