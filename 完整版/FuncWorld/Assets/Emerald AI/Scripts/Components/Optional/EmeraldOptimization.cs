using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EmeraldAI.Utility;
using UnityEditor;

namespace EmeraldAI
{
    [HelpURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-optional/optimization-component")]
    public class EmeraldOptimization : MonoBehaviour
    {
        public bool HideSettingsFoldout;
        public bool OptimizationFoldout;
        public enum OptimizedStates { Active = 0, Inactive = 1 };
        public OptimizedStates OptimizedState = OptimizedStates.Inactive;
        public enum TotalLODsEnum { One = 1, Two = 2, Three = 3, Four = 4 };
        public TotalLODsEnum TotalLODsRef = TotalLODsEnum.Three;
        public YesOrNo OptimizeAI = YesOrNo.Yes;
        public YesOrNo UseDeactivateDelay = YesOrNo.No;
        public enum MeshTypes { SingleMesh, LODGroup};
        public MeshTypes MeshType = MeshTypes.SingleMesh;
        public Renderer AIRenderer;
        public Renderer Renderer1;
        public Renderer Renderer2;
        public Renderer Renderer3;
        public Renderer Renderer4;
        public VisibilityCheck m_VisibilityCheck;
        public int DeactivateDelay = 5;
        public bool Initialized;

        EmeraldSystem EmeraldComponent;
        

        void Start()
        {
            InitializeOptimizationSettings();
            StartCoroutine(Initialize());
        }

        IEnumerator Initialize ()
        {
            while (Time.time < 0.5f)
            {
                yield return null;
            }

            Initialized = true;
        }

        /// <summary>
        /// Initializes the optimization settings.
        /// </summary>
        public void InitializeOptimizationSettings()
        {
            if (OptimizeAI == YesOrNo.Yes)
            {
                EmeraldComponent = GetComponent<EmeraldSystem>();

                if (OptimizeAI == YesOrNo.Yes && MeshType == MeshTypes.SingleMesh)
                {
                    if (AIRenderer != null && UseDeactivateDelay == YesOrNo.No)
                    {
                        DeactivateDelay = 0;
                        m_VisibilityCheck = AIRenderer.gameObject.AddComponent<VisibilityCheck>();
                        m_VisibilityCheck.EmeraldComponent = EmeraldComponent;
                        m_VisibilityCheck.EmeraldOptimization = this;
                    }
                    else if (AIRenderer != null && UseDeactivateDelay == YesOrNo.Yes)
                    {
                        m_VisibilityCheck = AIRenderer.gameObject.AddComponent<VisibilityCheck>();
                        m_VisibilityCheck.EmeraldComponent = EmeraldComponent;
                        m_VisibilityCheck.EmeraldOptimization = this;
                    }
                    else if (MeshType == MeshTypes.SingleMesh && AIRenderer == null)
                    {
                        OptimizeAI = YesOrNo.No;
                    }
                }

                if (MeshType == MeshTypes.LODGroup)
                {
                    GetLODs();

                    if (TotalLODsRef == TotalLODsEnum.One)
                    {
                        if (Renderer1 == null)
                        {
                            OptimizeAI = YesOrNo.No;
                            MeshType = MeshTypes.SingleMesh;
                        }
                        else
                        {
                            m_VisibilityCheck = Renderer1.gameObject.AddComponent<VisibilityCheck>();
                            m_VisibilityCheck.EmeraldComponent = EmeraldComponent;
                            m_VisibilityCheck.EmeraldOptimization = this;
                        }
                    }
                    else if (TotalLODsRef == TotalLODsEnum.Two)
                    {
                        if (Renderer1 == null || Renderer2 == null)
                        {
                            OptimizeAI = YesOrNo.No;
                            MeshType = MeshTypes.SingleMesh;
                        }
                        else
                        {
                            m_VisibilityCheck = Renderer2.gameObject.AddComponent<VisibilityCheck>();
                            m_VisibilityCheck.EmeraldComponent = EmeraldComponent;
                            m_VisibilityCheck.EmeraldOptimization = this;
                        }
                    }
                    else if (TotalLODsRef == TotalLODsEnum.Three)
                    {
                        if (Renderer1 == null || Renderer2 == null || Renderer3 == null)
                        {
                            OptimizeAI = YesOrNo.No;
                            MeshType = MeshTypes.SingleMesh;
                        }
                        else
                        {
                            m_VisibilityCheck = Renderer3.gameObject.AddComponent<VisibilityCheck>();
                            m_VisibilityCheck.EmeraldComponent = EmeraldComponent;
                            m_VisibilityCheck.EmeraldOptimization = this;
                        }
                    }
                    else if (TotalLODsRef == TotalLODsEnum.Four)
                    {
                        if (Renderer1 == null || Renderer2 == null ||
                            Renderer3 == null || Renderer4 == null)
                        {
                            OptimizeAI = YesOrNo.No;
                            MeshType = MeshTypes.SingleMesh;
                        }
                        else
                        {
                            m_VisibilityCheck = Renderer4.gameObject.AddComponent<VisibilityCheck>();
                            m_VisibilityCheck.EmeraldComponent = EmeraldComponent;
                            m_VisibilityCheck.EmeraldOptimization = this;
                        }
                    }
                }
            }
            else if (OptimizeAI == YesOrNo.No)
            {
                OptimizedState = OptimizedStates.Inactive;
            }
        }

        /// <summary>
        /// Gets all groups within an AI's LODGroup component.
        /// </summary>
        void GetLODs ()
        {
            LODGroup _LODGroup = GetComponentInChildren<LODGroup>();

            if (_LODGroup == null)
            {
                Debug.LogError("No LOD Group could be found. Please ensure that your AI has an LOD group that has at least 1 levels. The LODGroup Feature has been disabled.");
                MeshType = MeshTypes.SingleMesh;
            }
            else if (_LODGroup != null)
            {
                LOD[] AllLODs = _LODGroup.GetLODs();

                if (_LODGroup.lodCount <= 4)
                {
                    TotalLODsRef = (TotalLODsEnum)(_LODGroup.lodCount);
                }

                if (_LODGroup.lodCount >= 1)
                {
                    for (int i = 0; i < _LODGroup.lodCount; i++)
                    {
                        if (i == 0)
                        {
                            Renderer1 = AllLODs[i].renderers[0];
                        }
                        if (i == 1)
                        {
                            Renderer2 = AllLODs[i].renderers[0];
                        }
                        if (i == 2)
                        {
                            Renderer3 = AllLODs[i].renderers[0];
                        }
                        if (i == 3)
                        {
                            Renderer4 = AllLODs[i].renderers[0];
                        }
                    }
                }
            }
        }

        void Update ()
        {
            //Check all of an AI's LOD renderers, when using the Optimization feature.
            if (OptimizeAI == YesOrNo.Yes && MeshType == MeshTypes.LODGroup && Initialized)
            {
                m_VisibilityCheck.CheckAIRenderers();
            }
        }
    }
}