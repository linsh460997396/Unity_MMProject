using UnityEditor;

namespace EmeraldAI.Utility
{
    [InitializeOnLoad]
    public class EmeraldDefine
    {
        const string EmeraldAIDefinesString = "EMERALD_AI_2024_PRESENT";

        static EmeraldDefine()
        {
            InitializeEmeraldAIDefines();
        }

        static void InitializeEmeraldAIDefines()
        {
            var BTG = EditorUserBuildSettings.selectedBuildTargetGroup;
            string EmeraldAIDef = PlayerSettings.GetScriptingDefineSymbolsForGroup(BTG);

            if (!EmeraldAIDef.Contains(EmeraldAIDefinesString))
            {
                if (string.IsNullOrEmpty(EmeraldAIDef))
                {
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(BTG, EmeraldAIDefinesString);
                }
                else
                {
                    if (EmeraldAIDef[EmeraldAIDef.Length - 1] != ';')
                    {
                        EmeraldAIDef += ';';
                    }

                    EmeraldAIDef += EmeraldAIDefinesString;
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(BTG, EmeraldAIDef);
                }
            }
        }
    }
}
