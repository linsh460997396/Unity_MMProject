using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using System.Linq;

namespace EmeraldAI
{
    [HelpURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-optional/inverse-kinematics-component")]
    public class EmeraldInverseKinematics : MonoBehaviour
    {
        #region IK Variables
        public int WanderingLookAtLimit = 75;
        public float WanderingLookSpeed = 4f;
        public int WanderingLookDistance = 12;
        public float WanderingLookHeightOffset = 0;
        public int CombatLookAtLimit = 75;
        public float CombatLookSpeed = 6f;
        public int CombatLookDistance = 12;
        public float CombatLookHeightOffset = 0;
        public List<Rig> UpperBodyRigsList = new List<Rig>();
        public Transform m_AimSource;
        #endregion

        #region Private Variables
        List<MultiAimConstraint> m_MultiAimConstraints = new List<MultiAimConstraint>();
        Coroutine FadeRigCoroutine;
        Transform m_AimSourceParent;
        EmeraldSystem EmeraldComponent;
        Vector3 m_AimGoal;
        Vector3 CurrentTargetPosition;
        RigBuilder m_RigBuilder;
        MultiAimConstraint ChildMultiAimConstraints;
        float AutoFadeInTimer;
        bool FadeInProgress;
        float CurrentTargetAngle;
        #endregion

        #region Editor Variables
        public bool HideSettingsFoldout;
        public bool GeneralIKSettingsFoldout;
        public bool RigSettingsFoldout;
        #endregion

        void Start()
        {
            InitializeIK();
        }

        /// <summary>
        /// Initialize the IK system.
        /// </summary>
        private void InitializeIK()
        {
            EmeraldComponent = GetComponent<EmeraldSystem>();
            m_RigBuilder = GetComponent<RigBuilder>(); //Get the AI's RigBuilder.

            if (m_RigBuilder == null)
            {
                this.enabled = false;
                return;
            }

            EmeraldComponent.HealthComponent.OnDeath += DisableInverseKinematics; //Subscribe to Death delegate to disable Inverse Kinematics on death. 

            //Create an aim source
            m_AimSourceParent = new GameObject("Aim Source Parent").transform;
            m_AimSourceParent.position = EmeraldComponent.DetectionComponent.HeadTransform.position;
            m_AimSourceParent.SetParent(transform);

            //Set up the m_AimSource's position, which is based off an AI's HeadTransform position
            m_AimSource = new GameObject("Aim Source").transform;
            m_AimSource.position = EmeraldComponent.DetectionComponent.HeadTransform.position;
            m_AimSource.localPosition = m_AimSource.localPosition + transform.forward * 2;
            m_AimSource.SetParent(m_AimSourceParent);
            m_AimSource.localEulerAngles = Vector3.zero;
            m_AimGoal = m_AimSourceParent.position + (transform.forward * 3);

            //Initialize the list of MultiAimConstraints (located within each Rig) with the m_AimSource.
            for (int i = 0; i < UpperBodyRigsList.Count; i++)
            {
                var ChildMultiAimConstraints = UpperBodyRigsList[i].GetComponentsInChildren<MultiAimConstraint>();

                if (ChildMultiAimConstraints.Length > 0)
                {
                    for (int j = 0; j < ChildMultiAimConstraints.Length; j++)
                    {
                        m_MultiAimConstraints.Add(ChildMultiAimConstraints[j]);
                    }
                }

                for (int j = 0; j < m_MultiAimConstraints.Count; j++)
                {
                    var m_SourceObjects = m_MultiAimConstraints[j].data.sourceObjects;
                    m_MultiAimConstraints[j].data.maintainOffset = false;
                    m_SourceObjects.SetTransform(0, m_AimSource);

                    //Add a sourceObject if one doesn'transform exist
                    if (m_MultiAimConstraints[j].data.sourceObjects.Count == 0)
                        m_SourceObjects.Add(new WeightedTransform(m_AimSource, 1f));

                    m_MultiAimConstraints[j].data.sourceObjects = m_SourceObjects;
                }   
            }

            m_RigBuilder.Build(); //Rebuild the Rig Builder so the changes can be applied.
        }

        void Update()
        {
            UpdateIK();
        }

        /// <summary>
        /// Updates the IK system by tracking the current Aim Goal and lerping the Aim Source position to it over time.
        /// </summary>
        void UpdateIK ()
        {
            if (transform.localScale == Vector3.one * 0.003f) return;

            if (FadeInProgress && EmeraldComponent.AnimationComponent.IsIdling)
            {
                AutoFadeInTimer += Time.deltaTime;

                if (AutoFadeInTimer > 0.5f)
                {
                    for (int i = 0; i < UpperBodyRigsList.Count; i++)
                    {
                        UpperBodyRigsList[i].weight = Mathf.Lerp(UpperBodyRigsList[i].weight, 1, Time.deltaTime * 5);

                        if (UpperBodyRigsList[UpperBodyRigsList.Count - 1].weight >= 1f)
                        {
                            if (FadeRigCoroutine != null) StopCoroutine(FadeRigCoroutine);
                            AutoFadeInTimer = 0;
                            FadeInProgress = false;
                        }
                    }
                }
            }

            CurrentTargetAngle = EmeraldComponent.CombatComponent.TargetAngle;
            int LookAtLimit = EmeraldComponent.CombatComponent.CombatState ? CombatLookAtLimit : WanderingLookAtLimit;
            float LookSpeed = EmeraldComponent.CombatComponent.CombatState ? CombatLookSpeed : WanderingLookSpeed;
            int LookDistance = EmeraldComponent.CombatComponent.CombatState ? CombatLookDistance : WanderingLookDistance;
            float LookHeightOffset = EmeraldComponent.CombatComponent.CombatState ? CombatLookHeightOffset : WanderingLookHeightOffset;

            if (EmeraldComponent.CurrentTargetInfo.TargetSource != null)
            {
                CurrentTargetPosition = EmeraldComponent.CurrentTargetInfo.CurrentICombat.DamagePosition();
                float Distance = Vector3.Distance(m_AimSourceParent.position, CurrentTargetPosition);

                //Target is within look at and distance range.
                if (CurrentTargetAngle <= LookAtLimit && Distance < LookDistance && !EmeraldComponent.AnimationComponent.IsStunned && !EmeraldComponent.DetectionComponent.TargetObstructed && !EmeraldComponent.AnimationComponent.IsTurning && !EmeraldComponent.AnimationComponent.IsDodging)
                {
                    if (Distance > 0.75f)
                    {
                        float CurrentLookDistance = Vector3.Distance(new Vector3(m_AimSource.position.x, m_AimSource.position.y, CurrentTargetPosition.z), CurrentTargetPosition); //Get the distance between the current look position and the look goal (exluding the z axis)
                        EmeraldComponent.BehaviorsComponent.IsAiming = (CurrentLookDistance > 0.5f);
                        m_AimGoal = new Vector3(CurrentTargetPosition.x, Mathf.Lerp(m_AimGoal.y, CurrentTargetPosition.y, Time.deltaTime * 3f), CurrentTargetPosition.z);
                    }
                    else
                    {
                        m_AimGoal = new Vector3(CurrentTargetPosition.x, m_AimGoal.y, CurrentTargetPosition.z);
                    }
                }
                //Target is not within look at or distance range.
                else
                {
                    if (EmeraldComponent.CombatComponent.CombatState)
                    {
                        EmeraldComponent.BehaviorsComponent.IsAiming = false;
                        if (!EmeraldComponent.AnimationComponent.IsTurningLeft && !EmeraldComponent.AnimationComponent.IsTurningRight)
                        {
                            Vector3 LookPos = m_AimSourceParent.position + (transform.forward * EmeraldComponent.CombatComponent.DistanceFromTarget) + transform.up * LookHeightOffset;
                            m_AimGoal = new Vector3(LookPos.x, Mathf.Lerp(m_AimGoal.y, LookPos.y, Time.deltaTime), LookPos.z);
                        }
                        else if (EmeraldComponent.AnimationComponent.IsTurningLeft)
                        {
                            Vector3 LookPos = m_AimSourceParent.position + (transform.forward * EmeraldComponent.CombatComponent.DistanceFromTarget) + m_AimSourceParent.right * 5 + transform.up * LookHeightOffset;
                            m_AimGoal = new Vector3(LookPos.x, Mathf.Lerp(m_AimGoal.y, LookPos.y, Time.deltaTime), LookPos.z);
                        }
                        else if (EmeraldComponent.AnimationComponent.IsTurningRight)
                        {
                            Vector3 LookPos = m_AimSourceParent.position + (transform.forward * EmeraldComponent.CombatComponent.DistanceFromTarget) - m_AimSourceParent.right * 5 + transform.up * LookHeightOffset;
                            m_AimGoal = new Vector3(LookPos.x, Mathf.Lerp(m_AimGoal.y, LookPos.y, Time.deltaTime), LookPos.z);
                        }
                    }
                    else
                    {
                        Vector3 LookPos = m_AimSourceParent.position + (transform.forward * 3) + transform.up * LookHeightOffset;
                        m_AimGoal = new Vector3(LookPos.x, Mathf.Lerp(m_AimGoal.y, LookPos.y, Time.deltaTime), LookPos.z);
                    }
                }
            }
            else
            {
                if (!EmeraldComponent.CombatComponent.DeathDelayActive || EmeraldComponent.CombatTarget == null && !EmeraldComponent.CombatComponent.DeathDelayActive || EmeraldComponent.DetectionComponent.TargetObstructed)
                {
                    //Sets the aim goal to be in front of the AI (the default looking position).
                    m_AimGoal = m_AimSourceParent.position + (transform.forward * 3) + transform.up * LookHeightOffset;
                    EmeraldComponent.BehaviorsComponent.IsAiming = false;
                }
            }

            if (!EmeraldComponent.AnimationComponent.IsTurning && !EmeraldComponent.AnimationComponent.IsDodging) m_AimSource.position = Vector3.Slerp(m_AimSource.position, m_AimGoal, Time.deltaTime * LookSpeed);
            else m_AimSource.position = Vector3.Slerp(m_AimSource.position, m_AimGoal, Time.deltaTime * 1);
        }
        

        void Debugging()
        {
            for (int i = 0; i < UpperBodyRigsList.Count; i++)
            {
                var ChildMultiAimConstraints = UpperBodyRigsList[i].GetComponentsInChildren<MultiAimConstraint>();

                if (ChildMultiAimConstraints.Length > 0)
                {
                    for (int j = 0; j < ChildMultiAimConstraints.Length; j++)
                    {
                        Vector3 AimDir = (m_AimSource.position - ChildMultiAimConstraints[j].data.constrainedObject.position);
                        //Debug.DrawLine(ChildMultiAimConstraints[j].data.constrainedObject.position, AimDir);
                        Debug.DrawRay(ChildMultiAimConstraints[j].data.constrainedObject.position, AimDir);
                    }
                }
            }
        }

        /// <summary>
        /// Pass the rig name to fade out the rig's weight (String Parameter == Rig Name) and (Float Parameter == Fade Speed).
        /// </summary>
        public void FadeOutIK (AnimationEvent animationEvent)
        {
            if (!this.enabled) return;
            string[] RigNames = animationEvent.stringParameter.Split(',');
            if (RigNames.Length == 0)
            {
                //Find the rig using the passed name
                Rig m_Rig = UpperBodyRigsList.Find(x => x.gameObject.name == animationEvent.stringParameter);
                if (FadeRigCoroutine != null) StopCoroutine(FadeRigCoroutine);
                if (m_Rig != null) FadeRigCoroutine = StartCoroutine(FadeOutRigInternal(new Rig[] {m_Rig}, animationEvent.floatParameter));
                else Debug.LogError("The Rig " + animationEvent.stringParameter + " could not be found on the " + gameObject.name + " AI. Please check to ensure everything was properly named.");
            }
            else
            {
                List<Rig> m_Rigs = new List<Rig>();

                for (int i = 0; i < RigNames.Length; i++)
                {
                    //Remove the first character from the rig name if it's a space
                    if (RigNames[i][0] == ' ')
                        RigNames[i] = RigNames[i].Substring(1);

                    //Find the rig using the passed name and add it to the list of rigs
                    Rig m_Rig = UpperBodyRigsList.Find(x => x.gameObject.name == RigNames[i]);
                    if (m_Rig != null) m_Rigs.Add(m_Rig);
                    else Debug.LogError("The Rig " + RigNames[i] + " could not be found on the " + gameObject.name + " AI. Please check to ensure everything was properly named.");
                }

                //Start the coroutine to fade out each rig
                if (FadeRigCoroutine != null) StopCoroutine(FadeRigCoroutine);
                FadeRigCoroutine = StartCoroutine(FadeOutRigInternal(m_Rigs.ToArray(), animationEvent.floatParameter));
            }
        }

        IEnumerator FadeOutRigInternal(Rig[] rigs, float Speed)
        {
            if (EmeraldComponent.CombatComponent.CombatState) FadeInProgress = true;

            for (int i = 0; i < rigs.Length; i++)
            {
                float t = 0;
                float StartingWeight = rigs[i].weight;

                while (t < 1)
                {
                    t += Time.deltaTime * Speed;
                    EmeraldComponent.BehaviorsComponent.IsAiming = true;
                    rigs[i].weight = Mathf.Lerp(StartingWeight, 0, t);
                    yield return null;
                }
            }
        }

        /// <summary>
        /// Pass the rig name to fade in the rig's weight (String Parameter == Rig Name) and (Float Parameter == Fade Speed).
        /// </summary>
        public void FadeInIK(AnimationEvent animationEvent)
        {
             if (!this.enabled) return;
            string[] RigNames = animationEvent.stringParameter.Split(',');
            if (RigNames.Length == 0)
            {
                //Find the rig using the passed name
                Rig m_Rig = UpperBodyRigsList.Find(x => x.gameObject.name == animationEvent.stringParameter);
                if (FadeRigCoroutine != null) StopCoroutine(FadeRigCoroutine);
                if (m_Rig != null) FadeRigCoroutine = StartCoroutine(FadeInRigInternal(new Rig[] {m_Rig}, animationEvent.floatParameter));
                else Debug.LogError("The Rig " + animationEvent.stringParameter + " could not be found on the " + gameObject.name + " AI. Please check to ensure everything was properly named.");
            }
            else
            {
                List<Rig> m_Rigs = new List<Rig>();

                for (int i = 0; i < RigNames.Length; i++)
                {
                    //Remove the first character from the rig name if it's a space
                    if (RigNames[i][0] == ' ')
                        RigNames[i] = RigNames[i].Substring(1);

                    //Find the rig using the passed name and add it to the list of rigs
                    Rig m_Rig = UpperBodyRigsList.Find(x => x.gameObject.name == RigNames[i]);
                    if (m_Rig != null) m_Rigs.Add(m_Rig);
                    else Debug.LogError("The Rig " + RigNames[i] + " could not be found on the " + gameObject.name + " AI. Please check to ensure everything was properly named.");
                }

                //Start the coroutine to fade in each rig
                if (FadeRigCoroutine != null) StopCoroutine(FadeRigCoroutine);
                FadeRigCoroutine = StartCoroutine(FadeInRigInternal(m_Rigs.ToArray(), animationEvent.floatParameter));
            }

            FadeInProgress = false;
        }

        IEnumerator FadeInRigInternal(Rig[] rigs, float Speed)
        {
            for (int i = 0; i < rigs.Length; i++)
            {
                float t = 0;
                float StartingWeight = rigs[i].weight;

                while (t < 1)
                {
                    t += Time.deltaTime * Speed;
                    EmeraldComponent.BehaviorsComponent.IsAiming = true;
                    rigs[i].weight = Mathf.Lerp(StartingWeight, 1, t);
                    yield return null;
                }
            }

            FadeRigCoroutine = null;
        }

        /// <summary>
        /// Enables the Inverse Kinematics and RigBuilder.
        /// </summary>
        public void EnableInverseKinematics()
        {
            for (int i = 0; i < UpperBodyRigsList.Count; i++)
            {
                UpperBodyRigsList[i].weight = 1;
            }

            m_RigBuilder.enabled = true;
            this.enabled = true;
        }

        /// <summary>
        /// Enables the Inverse Kinematics and RigBuilder.
        /// </summary>
        public void DisableInverseKinematics()
        {
            for (int i = 0; i < UpperBodyRigsList.Count; i++)
            {
                UpperBodyRigsList[i].weight = 0;
            }

            m_RigBuilder.enabled = false;
            this.enabled = false;
        }
    }
}