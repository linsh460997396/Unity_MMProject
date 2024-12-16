using UnityEngine;
using UnityEditor;

namespace CellSpace
{
    public class CPEngineSettings : EditorWindow
    {
        GameObject EngineInstance;

        // ==== Find CPEngine =====
        bool FindEngine()
        { // returns false if engine not found, else true
            foreach (Object obj in FindObjectsOfType<CPEngine>())
            {
                if (obj != null)
                {
                    CPEngine engine = obj as CPEngine;
                    EngineInstance = engine.gameObject;
                    return true;
                }
            }
            return false;
        }

        [MenuItem("CellSpace/设置")]
        static void Init()
        {
            CPEngineSettings window = (CPEngineSettings)GetWindow(typeof(CPEngineSettings));
            window.Show();
        }

        public void OnGUI()
        {

            if (FindEngine() == false)
            {
                EditorGUILayout.LabelField("Cannot find an CPEngine game object in the scene!");
                return;
            }

            else if (EngineInstance.GetComponent<CellChunkManager>() == null)
            {
                EditorGUILayout.LabelField("The CPEngine game object does not have a CellChunkManager component!");
                return;
            }

            else
            {

                CPEngine engine = EngineInstance.GetComponent<CPEngine>();

                EditorGUILayout.BeginVertical();

                GUILayout.Space(10);

                engine.lWorldName = EditorGUILayout.TextField("World name", engine.lWorldName);

                GUILayout.Space(20);

                GUILayout.Label("CellChunk settings");

                engine.lChunkSpawnDistance = EditorGUILayout.IntField("CellChunk spawn distance", engine.lChunkSpawnDistance);
                engine.lChunkDespawnDistance = EditorGUILayout.IntField("CellChunk despawn distance", engine.lChunkDespawnDistance);
                engine.lHeightRange = EditorGUILayout.IntField("CellChunk height range", engine.lHeightRange);
                engine.lChunkSideLength = EditorGUILayout.IntField("CellChunk side length", engine.lChunkSideLength);
                engine.lTextureUnitX[0] = EditorGUILayout.FloatField("TextureUnitX[0]", engine.lTextureUnitX[0]);
                engine.lTextureUnitY[0] = EditorGUILayout.FloatField("TextureUnitY[0]", engine.lTextureUnitY[0]);
                engine.lTextureUnitX[1] = EditorGUILayout.FloatField("TextureUnitX[1]", engine.lTextureUnitX[1]);
                engine.lTextureUnitY[1] = EditorGUILayout.FloatField("TextureUnitY[1]", engine.lTextureUnitY[1]);
                engine.lTextureUnitX[2] = EditorGUILayout.FloatField("TextureUnitX[2]", engine.lTextureUnitX[2]);
                engine.lTextureUnitY[2] = EditorGUILayout.FloatField("TextureUnitY[2]", engine.lTextureUnitY[2]);
                engine.lTextureUnitX[3] = EditorGUILayout.FloatField("TextureUnitX[3]", engine.lTextureUnitX[3]);
                engine.lTextureUnitY[3] = EditorGUILayout.FloatField("TextureUnitY[3]", engine.lTextureUnitY[3]);
                engine.lTexturePadX = EditorGUILayout.FloatField("TexturePadX", engine.lTexturePadX);
                engine.lTexturePadY = EditorGUILayout.FloatField("TexturePadY", engine.lTexturePadY);
                engine.lGenerateMeshes = EditorGUILayout.Toggle("Generate meshes", engine.lGenerateMeshes);
                engine.lGenerateColliders = EditorGUILayout.Toggle("Generate colliders", engine.lGenerateColliders);
                engine.lShowBorderFaces = EditorGUILayout.Toggle("Show border faces", engine.lShowBorderFaces);

                GUILayout.Space(20);
                GUILayout.Label("Events settings");
                engine.lSendCameraLookEvents = EditorGUILayout.Toggle("Send camera look events", engine.lSendCameraLookEvents);
                engine.lSendCursorEvents = EditorGUILayout.Toggle("Send cursor events", engine.lSendCursorEvents);

                GUILayout.Space(20);
                GUILayout.Label("Data settings");
                engine.lSaveCellData = EditorGUILayout.Toggle("Save/load voxel data", engine.lSaveCellData);

                GUILayout.Space(20);
                GUILayout.Label("Multiplayer");
                engine.lEnableMultiplayer = EditorGUILayout.Toggle("Enable multiplayer", engine.lEnableMultiplayer);
                engine.lMultiplayerTrackPosition = EditorGUILayout.Toggle("Track player position", engine.lMultiplayerTrackPosition);
                engine.lChunkTimeout = EditorGUILayout.FloatField("CellChunk timeout (0=off)", engine.lChunkTimeout);
                engine.lMaxChunkDataRequests = EditorGUILayout.IntField("Max chunk data requests", engine.lMaxChunkDataRequests);
                GUILayout.Label("(0=off)");

                GUILayout.Space(20);
                GUILayout.Label("Performance");
                engine.lTargetFPS = EditorGUILayout.IntField("Target FPS", engine.lTargetFPS);
                engine.lMaxChunkSaves = EditorGUILayout.IntField("CellChunk saves limit", engine.lMaxChunkSaves);

                if (GUI.changed)
                {
                    // 检查GameObject是否是预制体的实例
                    if (PrefabUtility.GetPrefabInstanceStatus(engine.gameObject) == PrefabInstanceStatus.Connected)
                    {
                        //Debug.Log(selectedObject.name + " is an instance of a prefab.");
                        //修改后对预制体覆盖，如果没有关联会报错（可无视）
                        PrefabUtility.ReplacePrefab(engine.gameObject, PrefabUtility.GetPrefabParent(engine.gameObject), ReplacePrefabOptions.ConnectToPrefab);
                    }
                }
                EditorGUILayout.EndVertical();
            }
        }
    }
}
