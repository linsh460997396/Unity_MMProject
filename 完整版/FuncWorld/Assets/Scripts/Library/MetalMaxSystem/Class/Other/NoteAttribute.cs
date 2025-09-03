using System;

namespace MetalMaxSystem
{
    /// <summary>
    /// 支持为类或方法添加未完成、正在施工等说明提示.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)] public class NoteAttribute : Attribute
    {
        public string Note { get; }
        public NoteAttribute(string note) => Note = note;
    }
}

//示范用法
//[MetalMaxSystem.Note("待优化算法,预计v2.0重构")]
//public class DataProcessor { }