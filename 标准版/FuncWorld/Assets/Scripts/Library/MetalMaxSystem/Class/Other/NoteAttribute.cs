#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace MetalMaxSystem
{
    // 注意：如果要用于字段并在Inspector显示,通常建议继承 PropertyAttribute
    // 但如果仅作为元数据,继承 Attribute 也可以,只是需要更复杂的 Editor 脚本来读取
    [System.AttributeUsage(System.AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public class NoteAttribute : PropertyAttribute // 改为继承 PropertyAttribute 以便更容易绘制
    {
        public string Note { get; }
        public NoteType Type { get; }

        public NoteAttribute(string note, NoteType type = NoteType.Info)
        {
            Note = note;
            Type = type;
        }
    }

    public enum NoteType
    {
        Info,   // 普通信息
        Warning,// 警告
        Error   // 错误
    }

    /// <summary>
    /// 用于绘制不绑定具体字段值的装饰性 UI（如标题、间隔线、备注）,它不会干扰字段本身的序列化值
    /// </summary>
    [CustomPropertyDrawer(typeof(NoteAttribute))]
    public class NoteDrawer : DecoratorDrawer
    {
        public override void OnGUI(Rect position)
        {
            var noteAttr = attribute as NoteAttribute;
            if (noteAttr == null) return;

            // 调整高度和样式
            float height = EditorGUIUtility.singleLineHeight * 2;
            Rect rect = new Rect(position.x, position.y, position.width, height);

            GUIStyle style = new GUIStyle(EditorStyles.helpBox);

            // 根据类型设置颜色
            switch (noteAttr.Type)
            {
                case NoteType.Warning:
                    style.normal.textColor = Color.yellow;
                    break;
                case NoteType.Error:
                    style.normal.textColor = Color.red;
                    break;
                default:
                    style.normal.textColor = Color.cyan;
                    break;
            }

            // 绘制背景框和文字
            EditorGUI.HelpBox(rect, noteAttr.Note, MessageType.None);
            // 或者使用 GUI.Label 进行更自定义的绘制
        }

        public override float GetHeight()
        {
            var noteAttr = attribute as NoteAttribute;
            // 简单估算高度,实际项目中可能需要根据文本长度计算
            return EditorGUIUtility.singleLineHeight * 2 + 4;
        }
    }
}

#else

using System;

namespace MetalMaxSystem
{
    /// <summary>
    /// 支持为类或方法添加未完成、正在施工等说明提示.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public class NoteAttribute : Attribute
    {
        public string Note { get; }
        public NoteAttribute(string note) => Note = note;
    }
}

#endif

//示范用法(会显示在inspector)
//public class ExampleUsage : MonoBehaviour
//{
//    [MetalMaxSystem.Note("这是一个普通的备注信息", NoteType.Info)]
//    public int playerId;

//    [MetalMaxSystem.Note("警告：修改此值会导致存档损坏！", NoteType.Warning)]
//    public string saveFilePath;

//    [MetalMaxSystem.Note("错误：此字段已弃用,请使用 NewHealth", NoteType.Error)]
//    public float oldHealth;
//}