using System;

namespace CellSpace.WebSocket
{
    public interface INetworkService
    {
        bool IsConnected { get; }
        
        void Connect();
        
        void Disconnect();
        
        void SendMessage(NetworkMessage msg);
        
        event Action<NetworkMessage> OnMessageReceived;
        
        event Action OnConnected;
        
        event Action OnDisconnected;
    }
}
