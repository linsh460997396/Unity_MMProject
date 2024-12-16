using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.PackageManager.UI;

namespace CellSpace
{
    /// <summary>
    /// 单元编辑器功能类
    /// </summary>
    public class CPBlockEditor : EditorWindow
    {
        private GameObject SelectedBlock; // in the scene
        private GameObject SelectedPrefab; // prefab of the selected block
        private GameObject[] Blocks; // prefabs of blocks. null if prefab doesn'transform exist.
        private ushort LastId;
        private bool BlocksGotten, ReplaceBlockDialog;
        private Vector2 BlockListScroll, BlockEditorScroll;
        private GameObject EngineInstance;

        // ==== Find CPEngine =====
        private bool FindEngine()
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

        // ==== Editor window ====

        [MenuItem("CellSpace/单元编辑器")]
        static void Init()
        {
            CPBlockEditor window = (CPBlockEditor)GetWindow(typeof(CPBlockEditor));
            window.Show();
        }

        // ==== GUI ====
        public void OnGUI()
        {
            if (EngineInstance == null)
            {
                if (FindEngine() == false)
                {
                    EditorGUILayout.LabelField("Cannot find an CPEngine game object in the scene!");
                    //EditorUtility.DisplayDialog("提示", "未找到CPEngine组件，请确保场景中至少存在一个CPEngine组件。", "确定");
                    return;
                }
            }
            if (!BlocksGotten)
            {
                GetBlocks();
            }
            GUILayout.Space(10);
            CPEngine engine = EngineInstance.GetComponent<CPEngine>();
            engine.lBlocksPath = EditorGUILayout.TextField("Cell 预制体路径", engine.lBlocksPath);
            //engine.lBlocksPath = @"Assets\CellSpace\Res\Cells\";
            if (GUI.changed)
            {
                PrefabUtility.ReplacePrefab(engine.gameObject, PrefabUtility.GetPrefabParent(engine.gameObject), ReplacePrefabOptions.ConnectToPrefab);
            }
            GUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();

            // block list
            EditorGUILayout.BeginVertical(GUILayout.Width(190));

            GUILayout.Space(10);
            // new block
            if (GUILayout.Button("New block", GUILayout.Width(145), GUILayout.Height(30)))
            {
                CreateBlock();
            }
            GUILayout.Space(10);

            BlockListScroll = EditorGUILayout.BeginScrollView(BlockListScroll);
            int i = 0;
            int lastbutton = 0;
            foreach (GameObject block in Blocks)
            {
                if (block != null)
                {

                    // block button

                    if (i - 1 != lastbutton)
                    { // block space
                        GUILayout.Space(10);
                    }
                    lastbutton = i;

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(i.ToString());

                    Cell voxel = block.GetComponent<Cell>();

                    // selected button
                    if (SelectedPrefab != null && block.name == SelectedPrefab.name)
                    {
                        GUILayout.Box(voxel.VName, GUILayout.Width(140));
                    }

                    // unselected button
                    else if (GUILayout.Button(voxel.VName, GUILayout.Width(140)))
                    {
                        SelectBlock(block);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                i++;

            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            // block editor		
            EditorGUILayout.BeginVertical();
            BlockEditorScroll = EditorGUILayout.BeginScrollView(BlockEditorScroll);
            GUILayout.Space(20);
            if (SelectedBlock == null)
            {
                EditorGUILayout.LabelField("Select a block...");
            }

            if (SelectedBlock != null)
            {
                Cell selectedCell = SelectedBlock.GetComponent<Cell>();

                // name
                selectedCell.VName = EditorGUILayout.TextField("Name", selectedCell.VName);

                // id
                selectedCell.SetID((ushort)EditorGUILayout.IntField("ID", selectedCell.GetID()));

                GUILayout.Space(10);

                // mesh
                selectedCell.VCustomMesh = EditorGUILayout.Toggle("Custom mesh", selectedCell.VCustomMesh);
                selectedCell.VMesh = (Mesh)EditorGUILayout.ObjectField("Mesh", selectedCell.VMesh, typeof(Mesh), false);

                // texture
                if (selectedCell.VCustomMesh == false)
                {
                    if (selectedCell.VTexture.Length < 6)
                    {
                        selectedCell.VTexture = new Vector2[6];
                    }

                    selectedCell.VCustomSides = EditorGUILayout.Toggle("Define side textures", selectedCell.VCustomSides);

                    if (selectedCell.VCustomSides)
                    {
                        selectedCell.VTexture[0] = EditorGUILayout.Vector2Field("Top ", selectedCell.VTexture[0]);
                        selectedCell.VTexture[1] = EditorGUILayout.Vector2Field("Bottom ", selectedCell.VTexture[1]);
                        selectedCell.VTexture[2] = EditorGUILayout.Vector2Field("Right ", selectedCell.VTexture[2]);
                        selectedCell.VTexture[3] = EditorGUILayout.Vector2Field("Left ", selectedCell.VTexture[3]);
                        selectedCell.VTexture[4] = EditorGUILayout.Vector2Field("Forward ", selectedCell.VTexture[4]);
                        selectedCell.VTexture[5] = EditorGUILayout.Vector2Field("Back ", selectedCell.VTexture[5]);
                    }
                    else
                    {
                        selectedCell.VTexture[0] = EditorGUILayout.Vector2Field("Texture ", selectedCell.VTexture[0]);
                    }
                }

                // rotation
                else
                {
                    selectedCell.VRotation = (MeshRotation)EditorGUILayout.EnumPopup("Mesh Rotation", selectedCell.VRotation);
                }

                GUILayout.Space(10);

                // material index
                selectedCell.VSubmeshIndex = EditorGUILayout.IntField("Material Index", selectedCell.VSubmeshIndex);
                //如果一个团块预制网格渲染器组件有>1个材质，那么Cell预制体可以使用索引1或2的材质上面的纹理，如果<0则索引为0
                if (selectedCell.VSubmeshIndex < 0) selectedCell.VSubmeshIndex = 0;

                // transparency
                selectedCell.VTransparency = (Transparency)EditorGUILayout.EnumPopup("Transparency", selectedCell.VTransparency);

                // collision
                selectedCell.VColliderType = (ColliderType)EditorGUILayout.EnumPopup("Collider", selectedCell.VColliderType);


                GUILayout.Space(10);



                // components


                GUILayout.Label("Components");
                foreach (Object component in SelectedBlock.GetComponents<Component>())
                {
                    if (component is Transform == false && component is Cell == false)
                    {
                        GUILayout.Label(component.GetType().ToString());
                    }

                }


                GUILayout.Space(20);

                // apply
                if (GUILayout.Button("Apply", GUILayout.Height(80)))
                {

                    if (SelectedPrefab != null
                        && SelectedPrefab.GetComponent<Cell>().GetID() != selectedCell.GetID() // if id was changed
                        && GetBlock(selectedCell.GetID()) != null)
                    { // and there is already a block with this id

                        ReplaceBlockDialog = true;

                    }
                    else
                    {
                        ReplaceBlockDialog = false;
                        UpdateBlock();
                        ApplyBlocks();
                        GetBlocks();
                    }
                }
                if (ReplaceBlockDialog)
                {
                    GUILayout.Label("A block with this ID already exists!" + SelectedPrefab.GetComponent<Cell>().GetID() + selectedCell.GetID());
                }
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        public void OnDestroy()
        {
            DestroyImmediate(SelectedBlock);
        }

        // ==== block logic ====

        private void SelectBlock(GameObject block)
        {

            DestroyImmediate(SelectedBlock); // destroy previously selected block		
            try
            {
                SelectedBlock = Instantiate(block, new Vector3(0, 0, 0), Quaternion.identity) as GameObject; // instantiate the newly selected block so we can work on it
            }
            catch (System.Exception)
            {
                Debug.LogError("CellSpace: Cannot find the cell prefab! Have you deleted the cell 0 prefab?");
            }
            SelectedPrefab = block;
            SelectedBlock.name = block.name;

            Selection.objects = new Object[] { SelectedBlock };

            //Debug.Log ("Selected block ID is:" + block.GetComponent<Cell>().GetID().ToString());
        }

        private void CreateBlock()
        { // instantiates a new block with a free name

            SelectBlock(GetBlock(0)); // select the empty block
            SelectedBlock.name = "cell_" + (LastId + 1); // set name

            SelectedPrefab = null; // there is no selected prefab yet!
            ReplaceBlockDialog = false;

            UpdateBlock();
            ApplyBlocks();
            GetBlocks();
        }

        private void UpdateBlock()
        { // replaces selected block prefab with the scene instance (also sets the prefab name)

            SelectedBlock.name = "cell_" + SelectedBlock.GetComponent<Cell>().GetID();
            GameObject newPrefab = PrefabUtility.CreatePrefab(GetPrefabPath(SelectedBlock), SelectedBlock);
            SelectedPrefab = newPrefab;

        }

        void Update()
        {
            if (Input.GetKeyDown("gameObject"))
            {
                CreateBlock();
                UpdateBlock();
                ApplyBlocks();
            }
        }

        // ==== get, apply ====

        private string GetPath()
        {
            try
            {
                return EngineInstance.GetComponent<CPEngine>().lBlocksPath;
            }
            catch (System.Exception)
            {
                Debug.LogError("CPEngine prefab not found!");
                return null;
            }
        }
        private string GetBlockPath(ushort data)
        { // converts block id to prefab path		
            return GetPath() + "cell_" + data + ".prefab";
        }
        private string GetPrefabPath(GameObject block)
        {
            return GetPath() + block.name.Split("("[0])[0] + ".prefab";
        }

        private void GetBlocks()
        { // populates the Blocks array		

            Blocks = new GameObject[ushort.MaxValue];
            for (ushort i = 0; i < ushort.MaxValue; i++)
            {
                GameObject block = GetBlock(i);
                if (block != null)
                {
                    Blocks[i] = block;
                    LastId = i;
                }
            }

            BlocksGotten = true;
        }

        private GameObject GetBlock(ushort data)
        { // returns the prefab of the block with a given index

            Object blockObject = AssetDatabase.LoadAssetAtPath(GetBlockPath(data), typeof(Object)); //typeof(Object)表示加载的资源可以是任何类型（因为Object是Unity中所有对象的基类）
            GameObject block = null;

            if (blockObject != null)
            {
                block = (GameObject)blockObject;
            }
            else
            {
                return null;
            }

            if (block != null && block.GetComponent<Cell>() != null)
            {
                return block;
            }
            else
            {
                return null;
            }
        }

        private void ApplyBlocks()
        { // gets all valid voxel prefabs and applies them to the lBlocks array in the CPEngine GameObject

            List<GameObject> voxels = new List<GameObject>();
            int empty = 0; // count of empty items between non-empty items

            for (ushort i = 0; i < ushort.MaxValue; i++)
            {

                Object voxel = AssetDatabase.LoadAssetAtPath(GetBlockPath(i), typeof(Object));

                if (voxel != null)
                {
                    while (empty > 0)
                    { // add empty spaces
                        voxels.Add(null);
                        empty--;
                    }
                    voxels.Add((GameObject)voxel); // add item
                }
                else
                {
                    empty++;
                }
            }

            CPEngine engine = EngineInstance.GetComponent<CPEngine>();
            engine.lBlocks = voxels.ToArray();
            PrefabUtility.ReplacePrefab(engine.gameObject, PrefabUtility.GetPrefabParent(engine.gameObject), ReplacePrefabOptions.ConnectToPrefab);
        }

        private void SaveBlocks()
        {
            UpdateBlock();
            ApplyBlocks();
        }

    }

}
