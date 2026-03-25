using UnityEngine;
using System;
using System.Collections.Generic;

namespace EmeraldAI
{
    [RequireComponent(typeof(BoxCollider))]
    [HelpURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-optional/weapon-collisions-component")]
    public class EmeraldWeaponCollision : MonoBehaviour
    {
        public bool HideSettingsFoldout;
        public bool WeaponCollisionFoldout;
        public BoxCollider WeaponCollider;
        public Color CollisionBoxColor = new Color(1, 0.85f, 0, 0.25f);

        public List<Transform> HitTargets = new List<Transform>();

        public bool OnCollision;
        EmeraldSystem EmeraldComponent;
        Rigidbody m_Rigidbody;

        private void Start()
        {
            EmeraldComponent = GetComponentInParent<EmeraldSystem>();
            EmeraldComponent.CombatComponent.WeaponColliders.Add(this);
            EmeraldComponent.AnimationComponent.OnGetHit += DisableWeaponCollider; //Subscribe to the OnGetHit event for canceling weapon colliders during hits.
            EmeraldComponent.AnimationComponent.OnRecoil += DisableWeaponCollider; //Subscribe to the OnRecoil event for canceling weapon colliders during hits.
            WeaponCollider = GetComponent<BoxCollider>();
            WeaponCollider.enabled = false;
            WeaponCollider.isTrigger = true;
            if (m_Rigidbody == null) m_Rigidbody = gameObject.AddComponent<Rigidbody>();
            m_Rigidbody.isKinematic = true;
        }

        public void EnableWeaponCollider(string Name)
        {
            if (gameObject.name == Name)
            {
                if (gameObject.GetComponent<Collider>() == null)
                    return;

                WeaponCollider.enabled = true;
                EmeraldComponent.CombatComponent.CurrentWeaponCollision = this;
            }
        }

        public void DisableWeaponCollider(string Name)
        {
            if (gameObject.name == Name)
            {
                if (gameObject.GetComponent<Collider>() == null)
                    return;

                WeaponCollider.enabled = false;
                EmeraldComponent.CombatComponent.CurrentWeaponCollision = null;
                HitTargets.Clear();
            }
        }

        void DisableWeaponCollider ()
        {
            if (WeaponCollider.enabled)
            {
                WeaponCollider.enabled = false;
                HitTargets.Clear();
            }
        }

        private void OnTriggerEnter(Collider collision)
        {
            if (collision.gameObject != EmeraldComponent.gameObject)
            {
                if (collision.gameObject.GetComponent<LocationBasedDamageArea>() != null || collision.gameObject.GetComponent<IDamageable>() != null)
                {
                    if (EmeraldComponent.LBDComponent != null && !EmeraldComponent.LBDComponent.ColliderList.Exists(x => x.ColliderObject == collision))
                    {
                        DamageTarget(collision.gameObject);
                    }
                    else if (EmeraldComponent.LBDComponent == null)
                    {
                        DamageTarget(collision.gameObject);
                    }
                }
            }
        }

        /// <summary>
        /// Damages the target that collided with the weapon, given that it has a IDamageable.
        /// </summary>
        void DamageTarget(GameObject Target)
        {
            MeleeAbility m_MeleeAbility = EmeraldComponent.CombatComponent.CurrentEmeraldAIAbility as MeleeAbility;

            if (m_MeleeAbility != null)
            {
                Transform TargetRoot = m_MeleeAbility.GetTargetRoot(Target);

                if (TargetRoot != null && !HitTargets.Contains(TargetRoot))
                {
                    m_MeleeAbility.MeleeDamage(EmeraldComponent.gameObject, Target, TargetRoot);
                    HitTargets.Add(TargetRoot);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (WeaponCollider == null)
                return;

            if (WeaponCollider.enabled)
            {
                Gizmos.color = CollisionBoxColor;
                Gizmos.matrix = Matrix4x4.TRS(transform.TransformPoint(WeaponCollider.center), transform.rotation, transform.lossyScale);
                Gizmos.DrawCube(Vector3.zero, WeaponCollider.size);
            }
        }
    }
}