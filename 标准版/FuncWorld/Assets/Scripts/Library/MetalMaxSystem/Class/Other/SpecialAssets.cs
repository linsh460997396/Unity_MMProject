//#define UNITY_STANDALONE //BepInEx制作UnityMOD时可手动启用
#if UNITY_EDITOR || UNITY_STANDALONE

using UnityEngine;

[CreateAssetMenu(fileName = "SpecialAssets", menuName = "CreateSpecialAssets", order = 1)]
public class SpecialAssets : ScriptableObject, ISerializationCallbackReceiver
{
    //编辑器中分组,拖拽精灵图集到此(展开shift可多选)
    public Sprite[] sprites;

    //着色器数组
    public Shader[] shaders;

    //材质数组
    public Material[] materials;

    public ScriptableObject[] scriptableObjects;

    public void OnBeforeSerialize() { }

    public void OnAfterDeserialize() { }
}

#endif

//直接对类实例化(字段都是默认值),注意跟SO数据文件的区别
//SpecialAssets specialAssets = ScriptableObject.CreateInstance<SpecialAssets>();

//以下是用AssetBundleLoader同步加载外部地址的指定名称AB包内的SpecialAssets
//public static AssetBundle ab_SpecialAssets;
//SpecialAssets specialAssets  = AssetBundleLoader.LoadFromMemory<SpecialAssets>(Application.dataPath + "/Res/SpecialAssets.ab", "SpecialAssets", out ab_SpecialAssets);

//以下是用AssetBundleLoader异步加载外部地址的指定名称AB包内的SpecialAssets
//AssetBundleLoader.Instance.LoadAllFromMemoryAsync<SpecialAssets>(Application.dataPath + "/Res/SpecialAssets.ab");
//if (AssetBundleLoader.currentObjectGroup != null)
//{
//    Debug.Log("AssetBundleLoader.currentObjectGroup.Length => " + AssetBundleLoader.currentObjectGroup.Length.ToString());
//    for (int i = 0; i < AssetBundleLoader.currentObjectGroup.Length; i++)
//    {
//        Debug.Log("查询AB包中第" + i.ToString() + "个元素成功！");
//        Debug.Log("元素 " + i + " Name: " + AssetBundleLoader.currentObjectGroup[i].name);
//    }
//}
//_specialAssets = AssetBundleLoader.currentObjectGroup[0] as SpecialAssets;
