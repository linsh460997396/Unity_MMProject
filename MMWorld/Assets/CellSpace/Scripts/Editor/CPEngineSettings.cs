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

            if (CPEngine.unityEditorInteraction == false)
            {
                EditorGUILayout.LabelField("CPEngine.unityEditorInteraction == false!");
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

                CPEngine.lWorldName = EditorGUILayout.TextField("World name", CPEngine.lWorldName);

                GUILayout.Space(20);

                GUILayout.Label("CellChunk settings");

                CPEngine.lChunkSpawnDistance = EditorGUILayout.IntField("CellChunk spawn distance", CPEngine.lChunkSpawnDistance);
                CPEngine.lChunkDespawnDistance = EditorGUILayout.IntField("CellChunk despawn distance", CPEngine.lChunkDespawnDistance);
                CPEngine.lHeightRange = EditorGUILayout.IntField("CellChunk height range", CPEngine.lHeightRange);
                CPEngine.lChunkSideLength = EditorGUILayout.IntField("CellChunk side length", CPEngine.lChunkSideLength);
                CPEngine.lTextureUnitX[0] = EditorGUILayout.FloatField("textureUnitX[0]", CPEngine.lTextureUnitX[0]);
                CPEngine.lTextureUnitY[0] = EditorGUILayout.FloatField("textureUnitY[0]", CPEngine.lTextureUnitY[0]);
                CPEngine.lTextureUnitX[1] = EditorGUILayout.FloatField("textureUnitX[1]", CPEngine.lTextureUnitX[1]);
                CPEngine.lTextureUnitY[1] = EditorGUILayout.FloatField("textureUnitY[1]", CPEngine.lTextureUnitY[1]);
                CPEngine.lTextureUnitX[2] = EditorGUILayout.FloatField("textureUnitX[2]", CPEngine.lTextureUnitX[2]);
                CPEngine.lTextureUnitY[2] = EditorGUILayout.FloatField("textureUnitY[2]", CPEngine.lTextureUnitY[2]);
                CPEngine.lTextureUnitX[3] = EditorGUILayout.FloatField("textureUnitX[3]", CPEngine.lTextureUnitX[3]);
                CPEngine.lTextureUnitY[3] = EditorGUILayout.FloatField("textureUnitY[3]", CPEngine.lTextureUnitY[3]);
                CPEngine.lTexturePadX = EditorGUILayout.FloatField("texturePadX", CPEngine.lTexturePadX);
                CPEngine.lTexturePadY = EditorGUILayout.FloatField("texturePadY", CPEngine.lTexturePadY);
                CPEngine.lGenerateMeshes = EditorGUILayout.Toggle("Generate meshes", CPEngine.lGenerateMeshes);
                CPEngine.lGenerateColliders = EditorGUILayout.Toggle("Generate colliders", CPEngine.lGenerateColliders);
                CPEngine.lShowBorderFaces = EditorGUILayout.Toggle("Show border faces", CPEngine.lShowBorderFaces);

                GUILayout.Space(20);
                GUILayout.Label("Events settings");
                CPEngine.lSendCameraLookEvents = EditorGUILayout.Toggle("Send camera look events", CPEngine.lSendCameraLookEvents);
                CPEngine.lSendCursorEvents = EditorGUILayout.Toggle("Send cursor events", CPEngine.lSendCursorEvents);

                GUILayout.Space(20);
                GUILayout.Label("Data settings");
                CPEngine.lSaveCellData = EditorGUILayout.Toggle("Save/load voxel data", CPEngine.lSaveCellData);

                GUILayout.Space(20);
                GUILayout.Label("Multiplayer");
                CPEngine.lEnableMultiplayer = EditorGUILayout.Toggle("Enable multiplayer", CPEngine.lEnableMultiplayer);
                CPEngine.lMultiplayerTrackPosition = EditorGUILayout.Toggle("Track player position", CPEngine.lMultiplayerTrackPosition);
                CPEngine.lChunkTimeout = EditorGUILayout.FloatField("CellChunk timeout (0=off)", CPEngine.lChunkTimeout);
                CPEngine.lMaxChunkDataRequests = EditorGUILayout.IntField("Max chunk data requests", CPEngine.lMaxChunkDataRequests);
                GUILayout.Label("(0=off)");

                GUILayout.Space(20);
                GUILayout.Label("Performance");
                CPEngine.lTargetFPS = EditorGUILayout.IntField("Target FPS", CPEngine.lTargetFPS);
                CPEngine.lMaxChunkSaves = EditorGUILayout.IntField("CellChunk saves limit", CPEngine.lMaxChunkSaves);

                if (GUI.changed)
                {
                    // 检查GameObject是否是预制体的实例
                    if (PrefabUtility.GetPrefabInstanceStatus(engine.gameObject) == PrefabInstanceStatus.Connected)
                    {
                        //Debug.Log(selectedObject.name + " is an instance of a prefab.");
                        //修改后对预制体覆盖,如果没有关联会报错（可无视)
                        PrefabUtility.ReplacePrefab(engine.gameObject, PrefabUtility.GetPrefabParent(engine.gameObject), ReplacePrefabOptions.ConnectToPrefab);
                    }
                }
                EditorGUILayout.EndVertical();
            }
        }
    }
}
