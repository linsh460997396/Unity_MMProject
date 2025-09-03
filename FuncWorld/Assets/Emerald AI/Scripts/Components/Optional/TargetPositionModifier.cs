using UnityEngine;

namespace EmeraldAI.Utility
{
    [ExecuteInEditMode]
    [HelpURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-optional/target-position-modifier-component")]
    public class TargetPositionModifier : MonoBehaviour
    {
        public bool HideSettingsFoldout;
        public bool TPMSettingsFoldout = false;
        public Transform TransformSource;
        public float PositionModifier = 0;
        public float GizmoRadius = 0.15f;
        public Color GizmoColor = new Color(1f, 0, 0, 0.8f);

        void OnEnable()
        {
            if (TransformSource == null && Application.isPlaying)
            {
                Debug.LogError("<b>Target Position Modifier:</b> " + "No Transform Source has been assigned on " + gameObject.name + ". The Transform Source will be set as this object instead (which may be undesirable). To resolve this, add a proper Transform Source through the Target Position Modifier editor.");
                TransformSource = transform;
            }
        }


        private void OnDrawGizmosSelected()
        {
            if (TransformSource == null || !TPMSettingsFoldout || HideSettingsFoldout)
                return;

            Gizmos.color = GizmoColor;
            Gizmos.DrawSphere(TransformSource.position + (Vector3.up * PositionModifier), GizmoRadius);
        }
    }
}