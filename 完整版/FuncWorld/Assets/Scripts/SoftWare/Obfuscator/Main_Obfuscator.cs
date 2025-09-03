using TMPro;
using UnityEngine;

namespace GalaxyObfuscator
{
    public class Main_Obfuscator : MonoBehaviour
    {
        GameObject label_headTip;
        void Awake()
        {
            //MetalMaxSystem.DllLoader.SetDllDirectory("C:/Library");
        }

        void Start()
        {
            label_headTip = GameObject.Find("label_headTip");
            label_headTip.GetComponent<TextMeshProUGUI>().text = "处理中...";
        }
    }
}

