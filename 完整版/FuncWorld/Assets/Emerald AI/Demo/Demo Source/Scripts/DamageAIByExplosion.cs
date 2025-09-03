using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EmeraldAI;
using EmeraldAI.Utility;

[RequireComponent(typeof(AudioSource))]
public class DamageAIByExplosion : MonoBehaviour
{
    public LayerMask EmeraldAILayer;
    public int DamageAmount = 200;
    public int ExplosionRadius = 4;
    public int ExplosionForce = 400;
    public GameObject ExplosionEffect;
    public AudioClip ExplosionSound;
    GameObject ExplosionSoundObject;

    private void Start()
    {
        ExplosionSoundObject = Resources.Load("Emerald Sound") as GameObject;
        Invoke(nameof(Explode), 1.75f);
    }

    /// <summary>
    /// Call this function when you want to damage surrounding AI based on the set public variables within this script. 
    /// </summary>
    public void Explode ()
    {
        EmeraldObjectPool.SpawnEffect(ExplosionEffect, transform.position, Quaternion.identity, 4);
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, ExplosionRadius, EmeraldAILayer);

        foreach (var hitCollider in hitColliders)
        {
            int DamageMitigation = Mathf.RoundToInt((1f - Vector3.Distance(hitCollider.transform.position, transform.position) / ExplosionRadius) * DamageAmount);
            int ForceMitigation = Mathf.RoundToInt((1f - Vector3.Distance(hitCollider.transform.position, transform.position) / ExplosionRadius) * ExplosionForce);
            
            if (hitCollider.GetComponent<IDamageable>() != null)
            {
                hitCollider.GetComponent<IDamageable>().Damage(DamageMitigation, transform, ForceMitigation);
            }
        }
        SpawnExplosionSound();
        gameObject.SetActive(false);
    }

    void SpawnExplosionSound ()
    {
        GameObject SpawnedExplosionSound = EmeraldObjectPool.SpawnEffect(ExplosionSoundObject, transform.position, Quaternion.identity, 3);
        SpawnedExplosionSound.transform.SetParent(EmeraldSystem.ObjectPool.transform);
        AudioSource ExplosionAudioSource = SpawnedExplosionSound.GetComponent<AudioSource>();
        if (ExplosionSound != null) ExplosionAudioSource.PlayOneShot(ExplosionSound);
    }
}
