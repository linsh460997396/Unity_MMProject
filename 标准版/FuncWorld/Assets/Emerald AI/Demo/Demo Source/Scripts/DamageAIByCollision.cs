using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EmeraldAI.Example
{
    /// <summary>
    /// A script that damages AI based on collisions. Can be used for dynamic damaging objects such as rocks, logs, 
    /// and other falling objects or collision based weapons.
    /// </summary>
    public class DamageAIByCollision : MonoBehaviour
    {
        public bool IsTrigger = false;
        public int DamageAmount = 10;
        public int RagdollForceAmount = 50;

        private void OnTriggerEnter(Collider collision)
        {
            if (!IsTrigger) return;

            //Damages an AI to the collided object
            if (collision.gameObject.GetComponent<IDamageable>() != null)
            {
                collision.gameObject.GetComponent<IDamageable>().Damage(DamageAmount, transform, RagdollForceAmount);
            }
            //Damages an AI's location based damage component
            else if (collision.gameObject.GetComponent<LocationBasedDamageArea>() != null)
            {
                LocationBasedDamageArea LBDArea = collision.gameObject.GetComponent<LocationBasedDamageArea>();
                LBDArea.DamageArea(DamageAmount, transform, RagdollForceAmount);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (IsTrigger) return;

            //Damages an AI to the collided object
            if (collision.gameObject.GetComponent<IDamageable>() != null)
            {
                collision.gameObject.GetComponent<IDamageable>().Damage(DamageAmount, transform, RagdollForceAmount);
            }
            //Damages an AI's location based damage component
            else if (collision.gameObject.GetComponent<LocationBasedDamageArea>() != null)
            {
                LocationBasedDamageArea LBDArea = collision.gameObject.GetComponent<LocationBasedDamageArea>();
                LBDArea.DamageArea(DamageAmount, transform, RagdollForceAmount);
            }
        }
    }
}
