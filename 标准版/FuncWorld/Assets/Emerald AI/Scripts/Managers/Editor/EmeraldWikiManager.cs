using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace EmeraldAI.Utility
{
    public class EmeraldWikiManager : EditorWindow
    {
        [MenuItem("Window/Emerald AI/Offical Emerald AI Wiki/Home", false, 250)]
        public static void Home()
        {
            Application.OpenURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/");
        }

        [MenuItem("Window/Emerald AI/Offical Emerald AI Wiki/Getting Started", false, 250)]
        public static void GettingStarted()
        {
            Application.OpenURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/getting-started/getting-started");
        }

        [MenuItem("Window/Emerald AI/Offical Emerald AI Wiki/Upgrading to URP or HDRP", false, 251)]
        public static void UpgradingToURPAndHDRP()
        {
            Application.OpenURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/getting-started/upgrading-to-urp-and-hdrp");
        }

        [MenuItem("Window/Emerald AI/Offical Emerald AI Wiki/Animation Component", false, 300)]
        public static void AnimationComponent()
        {
            Application.OpenURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-required/animation-component");
        }

        [MenuItem("Window/Emerald AI/Offical Emerald AI Wiki/Behaviors Component", false, 300)]
        public static void BehaviorsComponent()
        {
            Application.OpenURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-required/behaviors-component");
        }

        [MenuItem("Window/Emerald AI/Offical Emerald AI Wiki/Combat Component", false, 300)]
        public static void CombatComponent()
        {
            Application.OpenURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-required/combat-component");
        }

        [MenuItem("Window/Emerald AI/Offical Emerald AI Wiki/Detection Component", false, 300)]
        public static void DetectionComponent()
        {
            Application.OpenURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-required/detection-component");
        }

        [MenuItem("Window/Emerald AI/Offical Emerald AI Wiki/Movement Component", false, 300)]
        public static void MovementComponent()
        {
            Application.OpenURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-required/movement-component");
        }

        [MenuItem("Window/Emerald AI/Offical Emerald AI Wiki/Health Component", false, 300)]
        public static void HealthComponent()
        {
            Application.OpenURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-required/health-component");
        }

        [MenuItem("Window/Emerald AI/Offical Emerald AI Wiki/Sounds Component", false, 300)]
        public static void SoundsComponent()
        {
            Application.OpenURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-required/sounds-component");
        }

        [MenuItem("Window/Emerald AI/Offical Emerald AI Wiki/Debugger Component", false, 400)]
        public static void DebuggerComponent()
        {
            Application.OpenURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-required/debugger-component");
        }

        [MenuItem("Window/Emerald AI/Offical Emerald AI Wiki/Events Component", false, 400)]
        public static void EventsComponent()
        {
            Application.OpenURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-required/events-component");
        }

        [MenuItem("Window/Emerald AI/Offical Emerald AI Wiki/Inverse Kinematics Component", false, 400)]
        public static void IKComponent()
        {
            Application.OpenURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-optional/inverse-kinematics-component");
        }

        [MenuItem("Window/Emerald AI/Offical Emerald AI Wiki/Items Component", false, 400)]
        public static void ItemsComponent()
        {
            Application.OpenURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-required/items-component");
        }

        [MenuItem("Window/Emerald AI/Offical Emerald AI Wiki/Location Based Damage Component", false, 400)]
        public static void LBDComponent()
        {
            Application.OpenURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-optional/location-based-damage-component");
        }

        [MenuItem("Window/Emerald AI/Offical Emerald AI Wiki/Optimization Component", false, 400)]
        public static void OptimizationComponent()
        {
            Application.OpenURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-required/optimization-component");
        }

        [MenuItem("Window/Emerald AI/Offical Emerald AI Wiki/Sound Detector Component", false, 400)]
        public static void SoundDetectorComponent()
        {
            Application.OpenURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-optional/sound-detector-component");
        }

        [MenuItem("Window/Emerald AI/Offical Emerald AI Wiki/Target Position Modifier Component", false, 400)]
        public static void TPMComponent()
        {
            Application.OpenURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-optional/target-position-modifier-component");
        }

        [MenuItem("Window/Emerald AI/Offical Emerald AI Wiki/UI Component", false, 400)]
        public static void UIComponent()
        {
            Application.OpenURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-required/ui-component");
        }

        [MenuItem("Window/Emerald AI/Offical Emerald AI Wiki/Weapon Collisions Component", false, 400)]
        public static void WeaponCollisionsComponent()
        {
            Application.OpenURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-optional/weapon-collisions-component");
        }

        [MenuItem("Window/Emerald AI/Integrations/FPS Engine", false, 251)]
        public static void FPSEngineIntegration()
        {
            Application.OpenURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/integrations/integrations/fps-engine");
        }

        [MenuItem("Window/Emerald AI/Integrations/Invector", false, 252)]
        public static void InvectorIntegration()
        {
            Application.OpenURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/integrations/invector");
        }

        [MenuItem("Window/Emerald AI/Integrations/Final IK", false, 253)]
        public static void FinalIKIntegration()
        {
            Application.OpenURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/integrations/final-ik");
        }

        [MenuItem("Window/Emerald AI/Integrations/Dialogue System", false, 253)]
        public static void DialogueSystemIntegration()
        {
            Application.OpenURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/integrations/integrations/dialogue-system");
        }

        [MenuItem("Window/Emerald AI/Integrations/Quest Machine", false, 254)]
        public static void QuestMachineIntegration()
        {
            Application.OpenURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/integrations/integrations/quest-machine");
        }

        [MenuItem("Window/Emerald AI/Integrations/Love Hate", false, 255)]
        public static void LoveHateIntegration()
        {
            Application.OpenURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/integrations/integrations/love-hate");
        }

        [MenuItem("Window/Emerald AI/Support/AI Generated Solutions", false, 253)]
        public static void AIGeneratedSolutions()
        {
            Application.OpenURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/help/using-the-wiki-search-tool");
        }

        [MenuItem("Window/Emerald AI/Support/Solutions to Possible Issues", false, 253)]
        public static void SolutionsToPossibleIssues()
        {
            Application.OpenURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/help/solutions-to-possible-issues");
        }

        [MenuItem("Window/Emerald AI/Support/Contact", false, 253)]
        public static void ContactSupport()
        {
            Application.OpenURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/help/support");
        }

        [MenuItem("Window/Emerald AI/Report a Bug", false, 300)]
        public static void ReportBug()
        {
            Application.OpenURL("https://github.com/Black-Horizon-Studios/Emerald-AI-2024/issues");
        }
    }
}
