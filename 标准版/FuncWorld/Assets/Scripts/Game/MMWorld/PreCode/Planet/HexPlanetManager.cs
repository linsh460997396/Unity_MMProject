using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MMWorld.HexSphere
{
    public class HexPlanetManager : MonoBehaviour
    {
        public HexPlanet hexPlanet;
        private HexPlanet _prevHexPlanet;

        public void UpdateRenderObjects()
        {
            foreach (Transform child in transform)
            {
                StartCoroutine(Destroy(child.gameObject));
            }

            if (hexPlanet == null)
            {
                return;
            }

            HexPlanetHexGenerator.GeneratePlanetTilesAndChunks(hexPlanet);

            for (int i = 0; i < hexPlanet.chunks.Count; i++)
            {
                GameObject chunkGO = new GameObject("Chunk " + i);
                chunkGO.transform.SetParent(transform);
                chunkGO.transform.localPosition = Vector3.zero;
                MeshFilter mf = chunkGO.AddComponent<MeshFilter>();
                MeshCollider mc = chunkGO.AddComponent<MeshCollider>();

                MeshRenderer mr = chunkGO.AddComponent<MeshRenderer>();
                mr.sharedMaterial = hexPlanet.chunkMaterial;

                HexChunkRenderer hcr = chunkGO.AddComponent<HexChunkRenderer>();
                hcr.SetHexChunk(hexPlanet, i);
                hcr.UpdateMesh();

                int hexPlanetLayer = LayerMask.NameToLayer("HexPlanet");
                if (hexPlanetLayer == -1)
                {
                    Debug.LogWarning("Layer \"HexPlanet\" must be created in the Layer Manager!");
                    hexPlanetLayer = 0;
                }
                chunkGO.layer = hexPlanetLayer;
            }
        }

        IEnumerator Destroy(GameObject go)
        {
            yield return new WaitForSeconds(0.1f);
            DestroyImmediate(go);
        }
    }
}