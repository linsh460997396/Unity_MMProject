using UnityEngine;

namespace EmeraldAI.Utility
{
    public class VisibilityCheck : MonoBehaviour
    {
        #region Variables
        [HideInInspector] public EmeraldOptimization EmeraldOptimization;
        [HideInInspector] public EmeraldSystem EmeraldComponent;
        IDamageable m_IDamageable;
        bool SystemActivated = false;
        float DeactivateTimer;
        #endregion

        void Start()
        {
            m_IDamageable = EmeraldOptimization.GetComponent<IDamageable>();
            Invoke("InitializeDelay", 1);
        }

        /// <summary>
        /// Used to delay the initialization on start to avoid multiple renderer events from triggering.
        /// </summary>
        void InitializeDelay()
        {
            SystemActivated = true;
        }

        /// <summary>
        /// Checks each of an AI's LOD renderers, if an AI is using LODs and the optimization setting.
        /// </summary>
        public void CheckAIRenderers()
        {
            if (EmeraldOptimization.OptimizedState == EmeraldOptimization.OptimizedStates.Inactive && EmeraldOptimization.Initialized)
            {
                if (!EmeraldOptimization.Renderer1.isVisible && EmeraldOptimization.TotalLODsRef == EmeraldOptimization.TotalLODsEnum.One)
                {
                    DeactivateTimer += Time.deltaTime;

                    if (EmeraldOptimization.UseDeactivateDelay == YesOrNo.Yes && DeactivateTimer >= EmeraldOptimization.DeactivateDelay || EmeraldOptimization.UseDeactivateDelay == YesOrNo.No)
                    {
                        Deactivate();
                    }
                }
                else if (!EmeraldOptimization.Renderer1.isVisible && !EmeraldOptimization.Renderer2.isVisible && EmeraldOptimization.TotalLODsRef == EmeraldOptimization.TotalLODsEnum.Two)
                {
                    DeactivateTimer += Time.deltaTime;

                    if (EmeraldOptimization.UseDeactivateDelay == YesOrNo.Yes && DeactivateTimer >= EmeraldOptimization.DeactivateDelay || EmeraldOptimization.UseDeactivateDelay == YesOrNo.No)
                    {
                        Deactivate();
                    }
                }
                else if (!EmeraldOptimization.Renderer1.isVisible && !EmeraldOptimization.Renderer2.isVisible && !EmeraldOptimization.Renderer3.isVisible && EmeraldOptimization.TotalLODsRef == EmeraldOptimization.TotalLODsEnum.Three)
                {
                    DeactivateTimer += Time.deltaTime;

                    if (EmeraldOptimization.UseDeactivateDelay == YesOrNo.Yes && DeactivateTimer >= EmeraldOptimization.DeactivateDelay || EmeraldOptimization.UseDeactivateDelay == YesOrNo.No)
                    {
                        Deactivate();
                    }
                }
                else if (!EmeraldOptimization.Renderer1.isVisible && !EmeraldOptimization.Renderer2.isVisible && !EmeraldOptimization.Renderer3.isVisible && !EmeraldOptimization.Renderer4.isVisible && EmeraldOptimization.TotalLODsRef == EmeraldOptimization.TotalLODsEnum.Four)
                {
                    DeactivateTimer += Time.deltaTime;

                    if (EmeraldOptimization.UseDeactivateDelay == YesOrNo.Yes && DeactivateTimer >= EmeraldOptimization.DeactivateDelay || EmeraldOptimization.UseDeactivateDelay == YesOrNo.No)
                    {
                        Deactivate();
                    }
                }
            }
            else if (EmeraldOptimization.OptimizedState == EmeraldOptimization.OptimizedStates.Active)
            {
                if (EmeraldOptimization.TotalLODsRef == EmeraldOptimization.TotalLODsEnum.Two)
                {
                    if (EmeraldOptimization.Renderer1.isVisible || EmeraldOptimization.Renderer2.isVisible)
                    {
                        Activate();
                    }
                }
                else if (EmeraldOptimization.TotalLODsRef == EmeraldOptimization.TotalLODsEnum.Three)
                {
                    if (EmeraldOptimization.Renderer1.isVisible || EmeraldOptimization.Renderer2.isVisible || EmeraldOptimization.Renderer3.isVisible)
                    {
                        Activate();
                    }
                }
                else if (EmeraldOptimization.TotalLODsRef == EmeraldOptimization.TotalLODsEnum.Four)
                {
                    if (EmeraldOptimization.Renderer1.isVisible || EmeraldOptimization.Renderer2.isVisible || EmeraldOptimization.Renderer3.isVisible || EmeraldOptimization.Renderer4.isVisible)
                    {
                        Activate();
                    }
                }
            }
        }

        /// <summary>
        /// Controls when an AI is deactivated while using the optimization system.
        /// </summary>
        public void Deactivate()
        {
            if (!EmeraldComponent.AnimationComponent.IsDead && EmeraldComponent.CombatTarget == null && !EmeraldComponent.MovementComponent.ReturningToStartInProgress && EmeraldComponent.DetectionComponent.CurrentDetectionState == EmeraldDetection.DetectionStates.Unaware)
            {
                EmeraldComponent.CombatComponent.TargetDetectionActive = false;
                EmeraldComponent.AIBoxCollider.enabled = false;
                EmeraldComponent.DetectionComponent.enabled = false;
                EmeraldComponent.enabled = false;
                EmeraldOptimization.OptimizedState = EmeraldOptimization.OptimizedStates.Active;
                if (EmeraldComponent.UIComponent != null) EmeraldComponent.UIComponent.SetUI(false);
                if (EmeraldComponent.InverseKinematicsComponent != null) EmeraldComponent.InverseKinematicsComponent.DisableInverseKinematics();
                EmeraldComponent.m_NavMeshAgent.destination = transform.position + transform.forward * 0.5f;
                EmeraldComponent.AIAnimator.SetFloat("Speed", 0);
                EmeraldComponent.AIAnimator.enabled = false;
                DeactivateTimer = 0;
            }
        }

        /// <summary>
        /// Controls when an AI is activated while using the optimization system.
        /// </summary>
        public void Activate()
        {
            if (m_IDamageable.Health > 0)
            {
                EmeraldComponent.CombatComponent.TargetDetectionActive = true;
                EmeraldComponent.AIBoxCollider.enabled = true;
                EmeraldComponent.DetectionComponent.enabled = true;
                EmeraldComponent.enabled = true;
                EmeraldComponent.AIAnimator.enabled = true;
                if (EmeraldComponent.InverseKinematicsComponent != null) EmeraldComponent.InverseKinematicsComponent.EnableInverseKinematics();
                EmeraldOptimization.OptimizedState = EmeraldOptimization.OptimizedStates.Inactive;
            }
        }

        /// <summary>
        /// Called when the AI's renderer becomes invisible.
        /// </summary>
        void OnBecameInvisible()
        {
            if (EmeraldComponent.AnimationComponent.IsDead) return;
            Invoke("Deactivate", EmeraldOptimization.DeactivateDelay);
        }

        /// <summary>
        /// Called once on start to allow the AI to be activated, but only if it is visible.
        /// </summary>
        void OnWillRenderObject()
        {
            if (EmeraldOptimization.OptimizedState == EmeraldOptimization.OptimizedStates.Active)
            {
                Activate();
            }
        }

        /// <summary>
        /// Called each time the AI's renderer becomes visible.
        /// </summary>
        void OnBecameVisible()
        {
            if (SystemActivated)
            {
                CancelInvoke();
                Activate();
            }
        }
    }
}