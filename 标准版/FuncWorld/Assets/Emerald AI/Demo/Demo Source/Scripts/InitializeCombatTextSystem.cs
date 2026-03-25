using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EmeraldAI.Example
{
    /// <summary>
    /// A script for initializing the CombatTextSystem, given that there are no AI in the scene.
    /// </summary>
    public class InitializeCombatTextSystem : MonoBehaviour
    {
        private void Awake()
        {
            SetupCombatText();
        }

        void SetupCombatText()
        {
            GameObject m_CombatTextSystem = Instantiate((GameObject)Resources.Load("Combat Text System") as GameObject, Vector3.zero, Quaternion.identity);
            m_CombatTextSystem.name = "Combat Text System";
            GameObject m_CombatTextCanvas = Instantiate((GameObject)Resources.Load("Combat Text Canvas") as GameObject, Vector3.zero, Quaternion.identity);
            m_CombatTextCanvas.name = "Combat Text Canvas";
            CombatTextSystem.Instance.CombatTextCanvas = m_CombatTextCanvas;
            EmeraldSystem.CombatTextSystemObject = m_CombatTextCanvas;
            CombatTextSystem.Instance.Initialize();
        }
    }
}