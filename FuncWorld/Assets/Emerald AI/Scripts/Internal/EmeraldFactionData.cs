using System.Collections.Generic;
using UnityEngine;

namespace EmeraldAI
{
    [System.Serializable]
    public class EmeraldFactionData : ScriptableObject
    {
        [SerializeField]
        public List<string> FactionNameList = new List<string>();
    }
}