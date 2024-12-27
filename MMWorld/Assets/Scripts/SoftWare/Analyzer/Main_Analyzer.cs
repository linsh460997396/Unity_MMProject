using MetalMaxSystem.Unity;

namespace Analyzer
{
    class Main_Analyzer : TextureAnalyzer
    {
        //加MonoBehaviour的必须是实例类，可继承使用MonoBehaviour下的方法，只有继承MonoBehaviour的脚本才能被附加到游戏物体上成为其组件，并且可以使用协程和摧毁引擎对象
        static string savePathFrontStr01 = "C:/Users/linsh/Desktop/MapSP/"; //输出纹理集图片的目录前缀字符
        static string savePathFrontStr02 = "C:/Users/linsh/Desktop/MapIndex/"; //输出纹理文本的目录前缀字符
        string folderPath = "C:/Users/linsh/Desktop/地图"; //填写要扫描的文件夹
        bool sliceStart = false;

        void Start()
        {
            //应用示范
            //StartSliceTextureAndSetSpriteIDMultiMergerAsync(folderPath, "*.png", 0.9f, savePathFrontStr01, savePathFrontStr02, 10, 16, 16, 8); //仅支持png和jpg，文件夹下多个图片合批特征图
            //StartSliceTextureAndSetSpriteIDAsync(folderPath, "*.png", 0.9f, savePathFrontStr01, savePathFrontStr02, 10, 16, 16, 8); //仅支持png和jpg，文件夹下每个图片独立特征图
        }

        void Update()
        {
            if (!sliceStart)
            {
                sliceStart = true;
                StartSliceTextureAndSetSpriteIDMultiMergerAsync(folderPath, "*.png", 0.9f, savePathFrontStr01, savePathFrontStr02, 10, 16, 16, 8); //仅支持png和jpg，文件夹下多个图片合批特征图
            }
        }
    }
}
