using UnityEngine;

[CreateAssetMenu(fileName = "SpecialAssets", menuName = "CreateSpecialAssets", order = 1)]
public class SpecialAssets : ScriptableObject
{
    //编辑器中分组,拖拽精灵图集到此(展开shift可多选)
    public Sprite[] sprites;
}
