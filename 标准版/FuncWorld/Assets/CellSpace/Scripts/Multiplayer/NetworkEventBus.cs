using System;

namespace CellSpace.WebSocket
{
    public static class NetworkEventBus
    {
        public static event Action<BlockData> OnBlockPlaced;
        public static event Action<BlockData> OnBlockChanged;
        public static event Action<BlockData> OnBlockDestroyed;
        public static event Action<ChunkDataResponse> OnChunkDataReceived;
        public static event Action<ChunkDelta> OnChunkDeltaReceived;
        public static event Action<NetworkMessage> OnUnknownMessage;

        public static void RaiseBlockPlaced(BlockData data)
        {
            OnBlockPlaced?.Invoke(data);
        }

        public static void RaiseBlockChanged(BlockData data)
        {
            OnBlockChanged?.Invoke(data);
        }

        public static void RaiseBlockDestroyed(BlockData data)
        {
            OnBlockDestroyed?.Invoke(data);
        }

        public static void RaiseChunkDataReceived(ChunkDataResponse data)
        {
            OnChunkDataReceived?.Invoke(data);
        }

        public static void RaiseChunkDeltaReceived(ChunkDelta data)
        {
            OnChunkDeltaReceived?.Invoke(data);
        }

        public static void DispatchMessage(NetworkMessage msg)
        {
            switch (msg.type)
            {
                case MessageType.PlaceBlock:
                    RaiseBlockPlaced(BlockData.Deserialize(msg.data));
                    break;
                case MessageType.ChangeBlock:
                    RaiseBlockChanged(BlockData.Deserialize(msg.data));
                    break;
                case MessageType.DestroyBlock:
                    RaiseBlockDestroyed(BlockData.Deserialize(msg.data));
                    break;
                case MessageType.SendCellData:
                    RaiseChunkDataReceived(ChunkDataResponse.Deserialize(msg.data));
                    break;
                case MessageType.SendCellDelta:
                    RaiseChunkDeltaReceived(ChunkDelta.Deserialize(msg.data));
                    break;
                default:
                    OnUnknownMessage?.Invoke(msg);
                    break;
            }
        }
    }
}
