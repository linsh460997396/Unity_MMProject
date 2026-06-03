using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Linq;

namespace CellSpace.WebSocket
{
    public enum MessageType : ushort
    {
        None = 0,
        
        // 连接相关
        Connect = 1,
        ConnectResponse = 2,
        Disconnect = 3,
        
        // 团块数据
        RequestCellData = 10,
        SendCellData = 11,
        RequestCellDelta = 12,
        SendCellDelta = 13,
        
        // 方块操作
        PlaceBlock = 20,
        ChangeBlock = 21,
        DestroyBlock = 22,
        
        // 玩家位置
        UpdatePlayerPosition = 30,
        UpdatePlayerRange = 31,
        
        // 同步消息
        SyncWorldState = 40,
        RequestFullState = 41,
        
        // 网络监控
        Ping = 90,
        Pong = 91,
    }

    public struct NetworkMessage
    {
        public MessageType type;
        public byte[] data;
        
        public byte[] Serialize()
        {
            byte[] typeBytes = BitConverter.GetBytes((ushort)type);
            byte[] lengthBytes = BitConverter.GetBytes(data != null ? data.Length : 0);
            
            int totalLength = typeBytes.Length + lengthBytes.Length + (data?.Length ?? 0);
            byte[] result = new byte[totalLength];
            
            Buffer.BlockCopy(typeBytes, 0, result, 0, typeBytes.Length);
            Buffer.BlockCopy(lengthBytes, 0, result, typeBytes.Length, lengthBytes.Length);
            
            if (data != null && data.Length > 0)
            {
                Buffer.BlockCopy(data, 0, result, typeBytes.Length + lengthBytes.Length, data.Length);
            }
            
            return result;
        }
        
        public static NetworkMessage Deserialize(byte[] rawData)
        {
            NetworkMessage message = new NetworkMessage();
            
            if (rawData.Length >= 4)
            {
                message.type = (MessageType)BitConverter.ToUInt16(rawData, 0);
                int dataLength = BitConverter.ToInt32(rawData, 2);
                
                if (dataLength > 0 && rawData.Length >= 4 + dataLength)
                {
                    message.data = new byte[dataLength];
                    Buffer.BlockCopy(rawData, 4, message.data, 0, dataLength);
                }
            }
            
            return message;
        }
    }

    public struct PlayerInfo
    {
        public string playerId;
        public int chunkX;
        public int chunkY;
        public int chunkZ;
        public int viewRange;
        
        public byte[] Serialize()
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                writer.Write(playerId);
                writer.Write(chunkX);
                writer.Write(chunkY);
                writer.Write(chunkZ);
                writer.Write(viewRange);
                return ms.ToArray();
            }
        }
        
        public static PlayerInfo Deserialize(byte[] data)
        {
            PlayerInfo info = new PlayerInfo();
            using (MemoryStream ms = new MemoryStream(data))
            using (BinaryReader reader = new BinaryReader(ms))
            {
                info.playerId = reader.ReadString();
                info.chunkX = reader.ReadInt32();
                info.chunkY = reader.ReadInt32();
                info.chunkZ = reader.ReadInt32();
                info.viewRange = reader.ReadInt32();
            }
            return info;
        }
    }

    public struct BlockData
    {
        public string playerId;
        public int x;
        public int y;
        public int z;
        public int chunkX;
        public int chunkY;
        public int chunkZ;
        public ushort blockId;
        
        public byte[] Serialize()
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                writer.Write(playerId);
                writer.Write(x);
                writer.Write(y);
                writer.Write(z);
                writer.Write(chunkX);
                writer.Write(chunkY);
                writer.Write(chunkZ);
                writer.Write(blockId);
                return ms.ToArray();
            }
        }
        
        public static BlockData Deserialize(byte[] data)
        {
            BlockData info = new BlockData();
            using (MemoryStream ms = new MemoryStream(data))
            using (BinaryReader reader = new BinaryReader(ms))
            {
                info.playerId = reader.ReadString();
                info.x = reader.ReadInt32();
                info.y = reader.ReadInt32();
                info.z = reader.ReadInt32();
                info.chunkX = reader.ReadInt32();
                info.chunkY = reader.ReadInt32();
                info.chunkZ = reader.ReadInt32();
                info.blockId = reader.ReadUInt16();
            }
            return info;
        }
    }

    public struct ChunkDataRequest
    {
        public string playerId;
        public int chunkX;
        public int chunkY;
        public int chunkZ;
        
        public byte[] Serialize()
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                writer.Write(playerId);
                writer.Write(chunkX);
                writer.Write(chunkY);
                writer.Write(chunkZ);
                return ms.ToArray();
            }
        }
        
        public static ChunkDataRequest Deserialize(byte[] data)
        {
            ChunkDataRequest request = new ChunkDataRequest();
            using (MemoryStream ms = new MemoryStream(data))
            using (BinaryReader reader = new BinaryReader(ms))
            {
                request.playerId = reader.ReadString();
                request.chunkX = reader.ReadInt32();
                request.chunkY = reader.ReadInt32();
                request.chunkZ = reader.ReadInt32();
            }
            return request;
        }
    }

    public struct ChunkDataResponse
    {
        public int chunkX;
        public int chunkY;
        public int chunkZ;
        public byte[] compressedData;
        
        public byte[] Serialize()
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                writer.Write(chunkX);
                writer.Write(chunkY);
                writer.Write(chunkZ);
                writer.Write(compressedData != null ? compressedData.Length : 0);
                if (compressedData != null && compressedData.Length > 0)
                {
                    writer.Write(compressedData);
                }
                return ms.ToArray();
            }
        }
        
        public static ChunkDataResponse Deserialize(byte[] data)
        {
            ChunkDataResponse response = new ChunkDataResponse();
            using (MemoryStream ms = new MemoryStream(data))
            using (BinaryReader reader = new BinaryReader(ms))
            {
                response.chunkX = reader.ReadInt32();
                response.chunkY = reader.ReadInt32();
                response.chunkZ = reader.ReadInt32();
                int dataLength = reader.ReadInt32();
                if (dataLength > 0)
                {
                    response.compressedData = reader.ReadBytes(dataLength);
                }
            }
            return response;
        }
    }

    public struct BlockChange
    {
        public int x;
        public int y;
        public int z;
        public ushort blockId;

        public byte[] Serialize()
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                writer.Write(x);
                writer.Write(y);
                writer.Write(z);
                writer.Write(blockId);
                return ms.ToArray();
            }
        }

        public static BlockChange Deserialize(BinaryReader reader)
        {
            BlockChange change = new BlockChange();
            change.x = reader.ReadInt32();
            change.y = reader.ReadInt32();
            change.z = reader.ReadInt32();
            change.blockId = reader.ReadUInt16();
            return change;
        }
    }

    public struct ChunkDelta
    {
        public int chunkX;
        public int chunkY;
        public int chunkZ;
        public BlockChange[] changes;

        public byte[] Serialize()
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                writer.Write(chunkX);
                writer.Write(chunkY);
                writer.Write(chunkZ);
                writer.Write(changes != null ? changes.Length : 0);
                if (changes != null)
                {
                    foreach (var change in changes)
                    {
                        writer.Write(change.x);
                        writer.Write(change.y);
                        writer.Write(change.z);
                        writer.Write(change.blockId);
                    }
                }
                return ms.ToArray();
            }
        }

        public static ChunkDelta Deserialize(byte[] data)
        {
            ChunkDelta delta = new ChunkDelta();
            using (MemoryStream ms = new MemoryStream(data))
            using (BinaryReader reader = new BinaryReader(ms))
            {
                delta.chunkX = reader.ReadInt32();
                delta.chunkY = reader.ReadInt32();
                delta.chunkZ = reader.ReadInt32();
                int count = reader.ReadInt32();
                delta.changes = new BlockChange[count];
                for (int i = 0; i < count; i++)
                {
                    delta.changes[i] = BlockChange.Deserialize(reader);
                }
            }
            return delta;
        }
    }

    public static class MessageSigner
    {
        private static byte[] secretKey = Encoding.UTF8.GetBytes("CellSpace-Network-Security-Key-2026");

        public static void SetSecretKey(string key)
        {
            secretKey = Encoding.UTF8.GetBytes(key);
        }

        public static byte[] Sign(byte[] data)
        {
            using (HMACSHA256 hmac = new HMACSHA256(secretKey))
            {
                return hmac.ComputeHash(data);
            }
        }

        public static bool Verify(byte[] data, byte[] signature)
        {
            byte[] computedSignature = Sign(data);
            return computedSignature.SequenceEqual(signature);
        }

        public static byte[] SignMessage(NetworkMessage message)
        {
            byte[] messageData = message.Serialize();
            return Sign(messageData);
        }

        public static bool VerifyMessage(NetworkMessage message, byte[] signature)
        {
            byte[] messageData = message.Serialize();
            return Verify(messageData, signature);
        }
    }
}