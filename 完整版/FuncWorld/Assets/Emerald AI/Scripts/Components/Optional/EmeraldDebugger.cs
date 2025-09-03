using UnityEngine;

namespace EmeraldAI
{
    [HelpURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-optional/debugger-component")]
    public class EmeraldDebugger : MonoBehaviour
    {
        #region Debugger Variables
        public YesOrNo EnableDebuggingTools = YesOrNo.Yes;
        public YesOrNo DrawLineOfSightLines = YesOrNo.Yes;
        public YesOrNo DrawNavMeshPath = YesOrNo.Yes;
        public Color NavMeshPathColor = Color.blue;
        public YesOrNo DrawNavMeshDestination = YesOrNo.Yes;
        public Color NavMeshDestinationColor = Color.red;
        public YesOrNo DrawLookAtPoints = YesOrNo.Yes;
        public YesOrNo DrawUndetectedTargetsLine = YesOrNo.Yes;
        public YesOrNo DebugLogTargets = YesOrNo.Yes;
        public YesOrNo DebugLogObstructions = YesOrNo.Yes;

        public YesOrNo DrawFootstepPositions = YesOrNo.Yes;
        public YesOrNo DebugLogFootsteps = YesOrNo.Yes;
        #endregion

        #region Private Variables
        EmeraldSystem EmeraldComponent;
        EmeraldInverseKinematics IKComponent;
        Color DebugLineColor = Color.green;
        Vector3 TargetDirection;
        Transform DestinationObject;
        #endregion

        #region Editor Variables
        public bool HideSettingsFoldout;
        public bool SettingsFoldout;
        #endregion

        void Start()
        {
            InitializeDebugger();
        }

        /// <summary>
        /// Initialize the Debugger Component.
        /// </summary>
        void InitializeDebugger ()
        {
            EmeraldComponent = GetComponent<EmeraldSystem>();
            IKComponent = GetComponent<EmeraldInverseKinematics>();
            EmeraldComponent.DetectionComponent.OnEnemyTargetDetected += DebugDetectedEnemyTarget; //Subscribe to the OnEnemyTargetDetected delegate for DebugDetectedEnemyTarget
            EmeraldComponent.DetectionComponent.OnPlayerDetected += DebugDetectedPlayerTarget; //Subscribe to the OnEnemyTargetDetected delegate for DebugDetectedPlayerTarget
        }

        public void DebuggerUpdate()
        {
            if (!enabled) return;

            DebugObstructions();
            DrawTargetRaycastLines();
            DrawUndetectedTargetsLineInternal();
            DrawNavMeshPathInternal();
            DrawNavMeshDestinationInternal();
        }

        /// <summary>
        /// Debug logs a message to the Unity Console for testing purposes.
        /// </summary>
        public void DebugLogMessage(string Message)
        {
            Debug.Log(Message);
        }

        /// <summary>
        /// Debug logs an AI's current obstruction to the Untiy Console.
        /// </summary>
        void DebugObstructions()
        {
            if (EnableDebuggingTools == YesOrNo.No || DebugLogObstructions == YesOrNo.No || !EmeraldComponent.CombatTarget && !EmeraldComponent.LookAtTarget) return;

            Transform CurrentObstruction = EmeraldComponent.DetectionComponent.CurrentObstruction;

            if (!EmeraldComponent.CombatComponent.DeathDelayActive && EmeraldComponent.CombatTarget && EmeraldComponent.CombatTarget.localScale != Vector3.one * 0.003f && CurrentObstruction) 
            {
                Debug.Log("<b>" + "<color=green>" + gameObject.name + " - Current Obstruction: " + "</color>" + "<color=red>" + CurrentObstruction.name + "</color>" + "</b>");
            }
        }

        /// <summary>
        /// Debug logs detected enemy target's information to the Untiy Console.
        /// </summary>
        void DebugDetectedEnemyTarget()
        {
            if (EnableDebuggingTools == YesOrNo.No || DebugLogTargets == YesOrNo.No) return;

            if (!EmeraldComponent.CombatComponent.DeathDelayActive)
            {
                if (EmeraldComponent.CombatTarget != null)
                {
                    Debug.Log("<b>" + "<color=green>" + gameObject.name + " - Current Combat Target: " + "</color>" + "<color=red>" + EmeraldComponent.CombatTarget.gameObject.name + "</color>" + "</b>" + "  |" +
                        "<b>" + "<color=green>" + "  Relation Type: " + "</color>" + "<color=red>" + EmeraldComponent.DetectionComponent.GetTargetFactionRelation(EmeraldComponent.CombatTarget) + "</color>" + "</b>");
                }
            }
        }

        /// <summary>
        /// Debug logs detected player target's information to the Untiy Console.
        /// </summary>
        void DebugDetectedPlayerTarget()
        {
            if (EnableDebuggingTools == YesOrNo.No || DebugLogTargets == YesOrNo.No) return;

            if (!EmeraldComponent.CombatComponent.DeathDelayActive)
            {
                if (EmeraldComponent.LookAtTarget != null)
                {
                    Debug.Log("<b>" + "<color=green>" + gameObject.name + " - Current Look At Target: " + "</color>" + "<color=red>" + EmeraldComponent.LookAtTarget.gameObject.name + "</color>" + "</b>" + "  |" +
                        "<b>" + "<color=green>" + "  Relation Type: " + "</color>" + "<color=green>" + EmeraldComponent.DetectionComponent.GetTargetFactionRelation(EmeraldComponent.LookAtTarget) + "</color>" + "</b>");
                }
            }
        }

        /// <summary>
        /// Draws the raycast lines between targets and look at targets.
        /// </summary>
        void DrawTargetRaycastLines()
        {
            if (EnableDebuggingTools == YesOrNo.No || DrawLineOfSightLines == YesOrNo.No || EmeraldComponent.CurrentTargetInfo.TargetSource == null) return;

            Transform HeadTransform = EmeraldComponent.DetectionComponent.HeadTransform;

            if (EmeraldComponent.CombatTarget != null)
            {
                TargetDirection = EmeraldComponent.CurrentTargetInfo.CurrentICombat.DamagePosition() - HeadTransform.position;
                Debug.DrawRay(new Vector3(HeadTransform.position.x, HeadTransform.position.y, HeadTransform.position.z), TargetDirection, DebugLineColor);
            }
            else if (EmeraldComponent.LookAtTarget != null)
            {
                Vector3 LookAtTargetDir = EmeraldComponent.CurrentTargetInfo.CurrentICombat.DamagePosition() - HeadTransform.position;
                Debug.DrawRay(new Vector3(EmeraldComponent.DetectionComponent.HeadTransform.position.x, HeadTransform.position.y, HeadTransform.position.z), LookAtTargetDir, DebugLineColor);
            }

            if (EmeraldComponent.DetectionComponent.TargetObstructed || EmeraldComponent.AnimationComponent.IsTurning)
            {
                DebugLineColor = Color.red;
            }
            else
            {
                DebugLineColor = Color.green;
            }
        }

        /// <summary>
        /// Draws lines towards each undetected targets while the AI is still in its Alert state.
        /// </summary>
        void DrawUndetectedTargetsLineInternal()
        {
            if (EnableDebuggingTools == YesOrNo.No || DrawUndetectedTargetsLine == YesOrNo.No) return;

            if (EmeraldComponent.DetectionComponent.CurrentDetectionState == EmeraldDetection.DetectionStates.Alert && EmeraldComponent.CombatTarget == null && !EmeraldComponent.AnimationComponent.IsDead)
            {
                foreach (Collider C in EmeraldComponent.DetectionComponent.LineOfSightTargets.ToArray())
                {
                    Vector3 direction = C.bounds.center - EmeraldComponent.DetectionComponent.HeadTransform.position;
                    Debug.DrawRay(EmeraldComponent.DetectionComponent.HeadTransform.position, direction, new Color(1, 0.549f, 0));
                }
            }
        }

        /// <summary>
        /// Draws the AI's current path with a line.
        /// </summary>
        void DrawNavMeshPathInternal ()
        {
            if (EnableDebuggingTools == YesOrNo.No || DrawNavMeshPath == YesOrNo.No) return;

            for (int i = 0; i < EmeraldComponent.m_NavMeshAgent.path.corners.Length; i++)
            {
                if (i > 0) Debug.DrawLine(EmeraldComponent.m_NavMeshAgent.path.corners[i - 1] + Vector3.up * 0.5f, EmeraldComponent.m_NavMeshAgent.path.corners[i] + Vector3.up * 0.5f, NavMeshPathColor);
                else Debug.DrawLine(EmeraldComponent.m_NavMeshAgent.path.corners[0] + Vector3.up * 0.5f, EmeraldComponent.m_NavMeshAgent.path.corners[i] + Vector3.up * 0.5f, NavMeshPathColor);
            }
        }

        /// <summary>
        /// Draws the AI's current destination.
        /// </summary>
        void DrawNavMeshDestinationInternal ()
        {
            if (EnableDebuggingTools == YesOrNo.No || DrawNavMeshDestination == YesOrNo.No) return;

            DrawCircle(EmeraldComponent.m_NavMeshAgent.destination, 0.25f, NavMeshDestinationColor);
            Debug.DrawLine(EmeraldComponent.m_NavMeshAgent.destination + Vector3.up * 0.5f, EmeraldComponent.m_NavMeshAgent.destination, NavMeshDestinationColor);
        }

        /// <summary>
        /// Draws the AI's current look at point when using the IK Component.
        /// </summary>
        void DrawLookAtPointsInternal()
        {
            if (DrawLookAtPoints == YesOrNo.No || !IKComponent || !IKComponent.m_AimSource || EmeraldComponent.AnimationComponent.IsDead) return;

            Gizmos.color = new Color(1, 0, 0, 0.35f);
            Gizmos.DrawSphere(IKComponent.m_AimSource.position, 0.12f);
            Gizmos.color = Color.white;
        }

        void DrawCircle(Vector3 center, float radius, Color color)
        {
            Vector3 prevPos = center + new Vector3(radius, 0, 0);
            for (int i = 0; i < 30; i++)
            {
                float angle = (float)(i + 1) / 30.0f * Mathf.PI * 2.0f;
                Vector3 newPos = center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
                Debug.DrawLine(prevPos, newPos, color);
                prevPos = newPos;
            }
        }

        private void OnDrawGizmos()
        {
            DrawLookAtPointsInternal();
        }
    }
}