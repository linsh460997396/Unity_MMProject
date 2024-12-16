using UnityEngine;
using System.Collections.Generic;

namespace CellSpace
{
    [RequireComponent(typeof(Client))]
    public class Server : MonoBehaviour
    {
        public bool EnableDebugLog;
        public float AutosaveTime; // how often to autosave voxel data; 0 is never
        private float autosaveTimer;
        private Dictionary<NetworkPlayer, CPIndex> PlayerPositions; // stores the index of each player's origin chunk. Changes will only be sent if the change is within their radius
        private Dictionary<NetworkPlayer, int> PlayerChunkSpawnDistances; // chunk spawn distance for each player

        void Awake()
        {
            if (CPEngine.EnableMultiplayer == false) Debug.LogWarning("CellSpace: Multiplayer is disabled. Unexpected behavior may occur.");
            CPEngine.Network = this.gameObject;
            ResetPlayerData();
            if (Network.isServer)
            {
                Debug.Log("Server: Server initialized.");
            }
        }

        void ResetPlayerData()
        {
            PlayerPositions = new Dictionary<NetworkPlayer, CPIndex>();// reset/initialize player origins
            PlayerChunkSpawnDistances = new Dictionary<NetworkPlayer, int>(); // reset/initialize player chunk spawn distances
        }

        void OnPlayerConnected()
        {
            if (EnableDebugLog) Debug.Log("Server: Player connected to server.");
        }

        void OnPlayerDisconnected_(NetworkPlayer player)
        {
            if (CPEngine.MultiplayerTrackPosition)
            {
                PlayerPositions.Remove(player);
                PlayerChunkSpawnDistances.Remove(player);
            }
            if (EnableDebugLog) Debug.Log("Server: Player disconnected from server.");
        }

        // ===== send chunk data

        [RPC]
        public void SendCellData(NetworkPlayer player, int chunkx, int chunky, int chunkz)
        {
            if (CPEngine.HorizontalMode)
            {
                SendCellData(player, chunkx, chunky);
            }
            else
            {
             // >> You can check whether the request for voxel data is valid here <<
             // if (true) {
                CellChunk chunk = CellChunkManager.SpawnChunkFromServer(chunkx, chunky, chunkz).GetComponent<CellChunk>(); // get the chunk (spawn it if it's not spawned already)
                chunk.Lifetime = 0f; // refresh the chunk's lifetime
                string data = CellChunkDataFiles.CompressData(chunk); // get data from the chunk and compress it
                byte[] dataBytes = GetBytes(data); // convert to byte array (sending strings over RPC doesn'transform work too well)
                GetComponent<NetworkView>().RPC("ReceiveCellData", player, chunkx, chunky, chunkz, dataBytes); // send compressed data to the player who requested it   
                                                                                                               // }
            }
        }
        [RPC]
        public void SendCellData(NetworkPlayer player, int chunkx, int chunky)
        {
            // >> You can check whether the request for voxel data is valid here <<
            // if (true) {
            CellChunk chunk = CellChunkManager.SpawnChunkFromServer(chunkx, chunky).GetComponent<CellChunk>(); // get the chunk (spawn it if it's not spawned already)
            chunk.Lifetime = 0f; // refresh the chunk's lifetime
            string data = CellChunkDataFiles.CompressData(chunk); // get data from the chunk and compress it
            byte[] dataBytes = GetBytes(data); // convert to byte array (sending strings over RPC doesn'transform work too well)
            GetComponent<NetworkView>().RPC("ReceiveCellData", player, chunkx, chunky, dataBytes); // send compressed data to the player who requested it   
            // }
        }

        // ===== receive / distribute voxel changes

        [RPC]
        public void ServerPlaceBlock(NetworkPlayer sender, int x, int y, int z, int chunkx, int chunky, int chunkz, int data)
        {
            if (CPEngine.HorizontalMode)
            {
                ServerPlaceBlock(sender, x, y, chunkx, chunky, data);
            }
            else
            {
                if (EnableDebugLog) Debug.Log("Server: Received PlaceBlock from player " + sender.ToString());
                // You can check whether the change sent by the client is valid here
                // if (true) {
                DistributeChange(sender, x, y, z, chunkx, chunky, chunkz, data, false);
                // }
            }
        }
        [RPC]
        public void ServerPlaceBlock(NetworkPlayer sender, int x, int y, int chunkx, int chunky, int data)
        {
            if (EnableDebugLog) Debug.Log("Server: Received PlaceBlock from player " + sender.ToString());
            // You can check whether the change sent by the client is valid here
            // if (true) {
            DistributeChange(sender, x, y, chunkx, chunky, data, false);
            // }
        }

        [RPC]
        public void ServerChangeBlock(NetworkPlayer sender, int x, int y, int z, int chunkx, int chunky, int chunkz, int data)
        {
            if (EnableDebugLog) Debug.Log("Server: Received ChangeBlock from player " + sender.ToString());
            // You can check whether the change sent by the client is valid here
            // if (true) {
            if (CPEngine.HorizontalMode)
            {
                DistributeChange(sender, x, y, chunkx, chunky, data, true);
            }
            else
            {
                DistributeChange(sender, x, y, z, chunkx, chunky, chunkz, data, true);
            }
            // }
        }
        [RPC]
        public void ServerChangeBlock(NetworkPlayer sender, int x, int y, int chunkx, int chunky, int data)
        {
            if (EnableDebugLog) Debug.Log("Server: Received ChangeBlock from player " + sender.ToString());
            // You can check whether the change sent by the client is valid here
            // if (true) {
            DistributeChange(sender, x, y, chunkx, chunky, data, true);
            // }
        }

        void DistributeChange(NetworkPlayer sender, int x, int y, int z, int chunkx, int chunky, int chunkz, int data, bool isChangeBlock)
        { // sends a change in the voxel data to all clients
            if (CPEngine.HorizontalMode)
            {
                DistributeChange(sender, x, y, chunkx, chunky, data, isChangeBlock);
            }
            else
            {
             // update server
                ApplyOnServer(x, y, z, chunkx, chunky, chunkz, data, isChangeBlock); // the server can'transform send RPCs to itself so we'll need to call them directly

                // send to every client
                foreach (NetworkPlayer player in Network.connections)
                {
                    if (player != Network.player)
                    { // skip server
                        if (CPEngine.MultiplayerTrackPosition == false || IsWithinRange(player, new CPIndex(chunkx, chunky, chunkz)))
                        { // check if the change is within range of each player

                            if (EnableDebugLog) Debug.Log("Server: Sending cell change to player " + player.ToString());

                            if (isChangeBlock)
                            {
                                GetComponent<NetworkView>().RPC("ReceiveChangeBlock", player, sender, x, y, z, chunkx, chunky, chunkz, data);
                            }
                            else
                            {
                                GetComponent<NetworkView>().RPC("ReceivePlaceBlock", player, sender, x, y, z, chunkx, chunky, chunkz, data);
                            }
                        }
                    }
                }
            }
        }
        void DistributeChange(NetworkPlayer sender, int x, int y, int chunkx, int chunky, int data, bool isChangeBlock)
        { // sends a change in the voxel data to all clients

            // update server
            ApplyOnServer(x, y, chunkx, chunky, data, isChangeBlock); // the server can'transform send RPCs to itself so we'll need to call them directly

            // send to every client
            foreach (NetworkPlayer player in Network.connections)
            {
                if (player != Network.player)
                { // skip server
                    if (CPEngine.MultiplayerTrackPosition == false || IsWithinRange(player, new CPIndex(chunkx, chunky)))
                    { // check if the change is within range of each player

                        if (EnableDebugLog) Debug.Log("Server: Sending cell change to player " + player.ToString());

                        if (isChangeBlock)
                        {
                            GetComponent<NetworkView>().RPC("ReceiveChangeBlock", player, sender, x, y, chunkx, chunky, data);
                        }
                        else
                        {
                            GetComponent<NetworkView>().RPC("ReceivePlaceBlock", player, sender, x, y, chunkx, chunky, data);
                        }
                    }
                }
            }
        }

        bool IsWithinRange(NetworkPlayer player, CPIndex chunkIndex)
        { // checks if the player is within the range of the chunk
            if (Mathf.Abs(PlayerPositions[player].x - chunkIndex.x) > PlayerChunkSpawnDistances[player])
            {
                return false;
            }
            if (Mathf.Abs(PlayerPositions[player].y - chunkIndex.y) > PlayerChunkSpawnDistances[player])
            {
                return false;
            }
            if (!CPEngine.HorizontalMode)
            {
                if (Mathf.Abs(PlayerPositions[player].z - chunkIndex.z) > PlayerChunkSpawnDistances[player])
                {
                    return false;
                }
            }
            return true;
        }


        void ApplyOnServer(int x, int y, int z, int chunkx, int chunky, int chunkz, int data, bool isChangeBlock)
        { // updates the voxel data stored on the server with the change sent by client
            if (CPEngine.HorizontalMode)
            {
                ApplyOnServer(x, y, chunkx, chunky, data, isChangeBlock);
            }
            else
            {
                CellChunk chunk = CellChunkManager.SpawnChunkFromServer(chunkx, chunky, chunkz).GetComponent<CellChunk>();// if chunk is not loaded, load it
                chunk.Lifetime = 0f; // refresh the chunk's lifetime
                if (isChangeBlock)
                {
                    GetComponent<Client>().ReceiveChangeBlock(Network.player, x, y, z, chunkx, chunky, chunkz, data);
                }
                else
                {
                    GetComponent<Client>().ReceivePlaceBlock(Network.player, x, y, z, chunkx, chunky, chunkz, data);
                }
            }
        }
        void ApplyOnServer(int x, int y, int chunkx, int chunky, int data, bool isChangeBlock)
        { // updates the voxel data stored on the server with the change sent by client
            CellChunk chunk = CellChunkManager.SpawnChunkFromServer(chunkx, chunky).GetComponent<CellChunk>();// if chunk is not loaded, load it
            chunk.Lifetime = 0f; // refresh the chunk's lifetime
            if (isChangeBlock)
            {
                GetComponent<Client>().ReceiveChangeBlock(Network.player, x, y, chunkx, chunky, data);
            }
            else
            {
                GetComponent<Client>().ReceivePlaceBlock(Network.player, x, y, chunkx, chunky, data);
            }
        }

        [RPC]
        public void UpdatePlayerPosition(NetworkPlayer player, int chunkx, int chunky, int chunkz)
        { // sent by client
            if (CPEngine.HorizontalMode)
            {
                UpdatePlayerPosition(player, chunkx, chunky);
            }
            else
            {
                PlayerPositions[player] = new CPIndex(chunkx, chunky, chunkz);
                if (EnableDebugLog) Debug.Log("Server: Updated player position. Player " + player.ToString() + ", position: " + new Vector3(chunkx, chunky, chunkz).ToString());
            }

        }
        [RPC]
        public void UpdatePlayerPosition(NetworkPlayer player, int chunkx, int chunky)
        { // sent by client
            PlayerPositions[player] = new CPIndex(chunkx, chunky);
            if (EnableDebugLog) Debug.Log("Server: Updated player position. Player " + player.ToString() + ", position: " + new Vector3(chunkx, chunky, 0f).ToString());
        }

        [RPC]
        public void UpdatePlayerRange(NetworkPlayer player, int range)
        { // sent by client
            PlayerChunkSpawnDistances[player] = range;
            if (EnableDebugLog) Debug.Log("Server: Updated player range. Player: " + player.ToString() + ", range: " + range.ToString());
        }

        // ===== save data

        void Update()
        {

            if (AutosaveTime > 0.0001f)
            {
                if (autosaveTimer >= AutosaveTime)
                {
                    autosaveTimer = 0;
                    CPEngine.SaveWorld();
                }
                else
                {
                    autosaveTimer += Time.deltaTime;
                }
            }
        }


        // convert string to byte array
        static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }



    }

}