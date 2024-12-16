using UnityEngine;

namespace CellSpace
{

    public class Client : MonoBehaviour
    {

        void OnServerDisconnected()
        {
            Destroy(this.gameObject);
        }

        public void OnConnectedToServer()
        {
            Debug.Log("Client: Connected to server.");
            if (CPEngine.EnableMultiplayer == false) Debug.LogWarning("CellSpace: Multiplayer is disabled. Unexpected behavior may occur.");
            CPEngine.SaveCellData = false; // disable local saving for client
        }

        // ===== network communication ============

        public static void UpdatePlayerPosition(int x, int y, int z)
        {
            if (CPEngine.HorizontalMode)
            {
                CPEngine.Network.GetComponent<NetworkView>().RPC("UpdatePlayerPosition", RPCMode.Server, Network.player, x, y, 0);
            }
            else
            {
                CPEngine.Network.GetComponent<NetworkView>().RPC("UpdatePlayerPosition", RPCMode.Server, Network.player, x, y, z);
            }
        }
        public static void UpdatePlayerPosition(int x, int y)
        {
            CPEngine.Network.GetComponent<NetworkView>().RPC("UpdatePlayerPosition", RPCMode.Server, Network.player, x, y, 0);
        }
        public static void UpdatePlayerPosition(CPIndex index)
        {
            if (CPEngine.HorizontalMode)
            {
                CPEngine.Network.GetComponent<NetworkView>().RPC("UpdatePlayerPosition", RPCMode.Server, Network.player, index.x, index.y, 0);
            }
            else
            {
                CPEngine.Network.GetComponent<NetworkView>().RPC("UpdatePlayerPosition", RPCMode.Server, Network.player, index.x, index.y, index.z);
            }
        }
        public static void UpdatePlayerRange(int range)
        {
            CPEngine.Network.GetComponent<NetworkView>().RPC("UpdatePlayerRange", RPCMode.Server, Network.player, range);
        }

        [RPC]
        public void ReceiveCellData(int chunkx, int chunky, int chunkz, byte[] data)
        {
            GameObject chunkObject;
            if (CPEngine.HorizontalMode)
            {
                chunkObject = CellChunkManager.GetChunk(chunkx, chunky); // find the chunk
            }
            else
            {
                chunkObject = CellChunkManager.GetChunk(chunkx, chunky, chunkz); // find the chunk
            }
            if (chunkObject == null) return; // abort if chunk isn'transform spawned anymore
            CellChunk chunk = chunkObject.GetComponent<CellChunk>();
            CellChunkDataFiles.DecompressData(chunk, GetString(data)); //decompress data                                                        
            //CellChunkManager.DataReceivedCount ++; // let CellChunkManager know that we have received the data
            chunk.CellsDone = true; // let CellChunk know that it can update it's mesh
            CellChunk.CurrentChunkDataRequests--;
        }
        [RPC]
        public void ReceiveCellData(int chunkx, int chunky, byte[] data)
        {
            GameObject chunkObject = CellChunkManager.GetChunk(chunkx, chunky); // find the chunk
            if (chunkObject == null) return; // abort if chunk isn'transform spawned anymore
            CellChunk chunk = chunkObject.GetComponent<CellChunk>();
            CellChunkDataFiles.DecompressData(chunk, GetString(data)); //decompress data                                                        
            //CellChunkManager.DataReceivedCount ++; // let CellChunkManager know that we have received the data
            chunk.CellsDone = true; // let CellChunk know that it can update it's mesh
            CellChunk.CurrentChunkDataRequests--;
        }

        public void SendPlaceBlock(CellInfo info, ushort data)
        {   // sends a voxel change to the server, which then redistributes it to other clients

            // convert to ints
            int chunkx = info.chunk.ChunkIndex.x;
            int chunky = info.chunk.ChunkIndex.y;
            int chunkz = 0;
            if (!CPEngine.HorizontalMode)
            {
                chunkz = info.chunk.ChunkIndex.z;
            }
            // send to server
            if (Network.isServer)
            {
                GetComponent<Server>().ServerPlaceBlock(Network.player, info.index.x, info.index.y, info.index.z, chunkx, chunky, chunkz, (int)data);
            }
            else
            {
                GetComponent<NetworkView>().RPC("ServerPlaceBlock", RPCMode.Server, Network.player, info.index.x, info.index.y, info.index.z, chunkx, chunky, chunkz, (int)data);
            }
        }

        public void SendChangeBlock(CellInfo info, ushort data)
        {
            // convert to ints
            int chunkx = info.chunk.ChunkIndex.x;
            int chunky = info.chunk.ChunkIndex.y;
            int chunkz = 0; //2D模式下z值为0
            if (!CPEngine.HorizontalMode)
            {
                chunkz = info.chunk.ChunkIndex.z;
            }
            // send to server
            if (Network.isServer)
            {
                GetComponent<Server>().ServerChangeBlock(Network.player, info.index.x, info.index.y, info.index.z, chunkx, chunky, chunkz, (int)data);
            }
            else
            {
                GetComponent<NetworkView>().RPC("ServerChangeBlock", RPCMode.Server, Network.player, info.index.x, info.index.y, info.index.z, chunkx, chunky, chunkz, (int)data);
            }
        }

        [RPC]
        public void ReceivePlaceBlock(NetworkPlayer sender, int x, int y, int z, int chunkx, int chunky, int chunkz, int data)
        {   // receives a change sent by other client or server
            if (CPEngine.HorizontalMode)
            {
                ReceivePlaceBlock(sender, x, y, chunkx, chunky, data);
            }
            else
            {
                GameObject chunkObject = CellChunkManager.GetChunk(chunkx, chunky, chunkz);
                if (chunkObject != null)
                {
                    CPIndex voxelIndex = new CPIndex(x, y, z);
                    CellInfo info = new CellInfo(voxelIndex, chunkObject.GetComponent<CellChunk>());
                    // apply change
                    if (data == 0)
                    {
                        Cell.DestroyBlockMultiplayer(info, sender);
                    }
                    else
                    {
                        Cell.PlaceBlockMultiplayer(info, (ushort)data, sender);
                    }
                }
            }
        }
        [RPC]
        public void ReceivePlaceBlock(NetworkPlayer sender, int x, int y, int chunkx, int chunky, int data)
        {   // receives a change sent by other client or server
            GameObject chunkObject = CellChunkManager.GetChunk(chunkx, chunky);
            if (chunkObject != null)
            {
                // convert back to CellInfo
                CPIndex voxelIndex = new CPIndex(x, y);
                CellInfo info = new CellInfo(voxelIndex, chunkObject.GetComponent<CellChunk>());
                // apply change
                if (data == 0)
                {
                    Cell.DestroyBlockMultiplayer(info, sender);
                }
                else
                {
                    Cell.PlaceBlockMultiplayer(info, (ushort)data, sender);
                }
            }
        }

        [RPC]
        public void ReceiveChangeBlock(NetworkPlayer sender, int x, int y, int z, int chunkx, int chunky, int chunkz, int data)
        {   // receives a change sent by other client or server
            if (CPEngine.HorizontalMode)
            {
                ReceiveChangeBlock(sender, x, y, chunkx, chunky, data);
            }
            else
            {
                GameObject chunkObject = CellChunkManager.GetChunk(chunkx, chunky, chunkz);
                if (chunkObject != null)
                {

                    // convert back to CellInfo
                    CPIndex voxelIndex = new CPIndex(x, y, z);
                    CellInfo info = new CellInfo(voxelIndex, chunkObject.GetComponent<CellChunk>());

                    // apply change
                    Cell.ChangeBlockMultiplayer(info, (ushort)data, sender);
                }
            }
        }
        [RPC]
        public void ReceiveChangeBlock(NetworkPlayer sender, int x, int y, int chunkx, int chunky, int data)
        {   // receives a change sent by other client or server
            GameObject chunkObject = CellChunkManager.GetChunk(chunkx, chunky);
            if (chunkObject != null)
            {
                // convert back to CellInfo
                CPIndex voxelIndex = new CPIndex(x, y);
                CellInfo info = new CellInfo(voxelIndex, chunkObject.GetComponent<CellChunk>());
                // apply change
                Cell.ChangeBlockMultiplayer(info, (ushort)data, sender);
            }
        }

        // convert back to string
        static string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

    }

}