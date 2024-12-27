#if UNITY_EDITOR || UNITY_STANDALONE
using UnityEngine;

namespace MetalMaxSystem.Unity
{
    /// <summary>
    /// 在UI界面显示帧数和延迟
    /// </summary>
    public class FPS : MonoBehaviour
    {
        private float deltaTime = 0.0f;

        [Tooltip("用于调整显示区高度，值越大高度越小")] //编辑器界面鼠标悬浮该字段时提示
        public int size = 20;

        private void Awake()
        {
            //Application.targetFrameRate = -1;
        }

        void Update()
        {
            //计算每帧之间的时间差，并进行平滑处理，避免帧率波动带来的突变
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        }

        void OnGUI()
        {
            //在Unity游戏引擎中显示当前的帧率（FPS）和每帧的渲染时间（以毫秒为单位）

            //获取当前屏幕的宽度和高度，用于后续计算显示区域的大小和位置
            int w = Screen.width, h = Screen.height;
            //创建一个新的GUIStyle对象，用于定义文本的样式，包括对齐方式、字体大小和颜色
            GUIStyle style = new GUIStyle();
            //定义一个矩形区域rect，用于指定帧率显示的位置和大小
            Rect rect = new Rect(0, 0, w, h * 2 / size); //设置帧率显示区域的位置和大小  
                                                         //Rect rect = new Rect(0, 0, 200, 100); 
                                                         //设置文本控件锚点的对齐方式为左上角(UI屏幕一般都以左上角为原点)
            style.alignment = TextAnchor.UpperLeft;
            //根据屏幕高度和size变量计算字体大小
            style.fontSize = h * 2 / size;
            //设置文本颜色为深蓝色
            style.normal.textColor = new Color(0.0f, 0.0f, 0.5f, 1.0f);
            //计算当前的帧率（FPS）和每帧的渲染时间（以毫秒为单位）
            float fps = 1.0f / deltaTime;
            float ms = deltaTime * 1000.0f; //秒转毫秒
            string text = string.Format("{0:0.} FPS | {1:0.} ms", fps, ms);
            //在矩形区域绘制文本标签
            GUI.Label(rect, text, style);
        }
    }
}
#endif
