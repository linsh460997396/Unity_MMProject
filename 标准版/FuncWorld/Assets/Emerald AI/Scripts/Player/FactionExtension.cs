using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EmeraldAI
{
    [HelpURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/getting-started/setting-up-a-player-with-emerald-ai")]
    public class FactionExtension : MonoBehaviour, IFaction
    {
        public bool HideSettingsFoldout;
        public bool FactionFoldout = true;
        [SerializeField] public int CurrentFaction = 0;
        [SerializeField] public static List<string> StringFactionList = new List<string>();

        public int GetFaction()
        {
            return CurrentFaction;
        }
    }
}