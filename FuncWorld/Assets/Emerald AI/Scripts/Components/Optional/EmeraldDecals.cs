using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

namespace EmeraldAI
{
    /// <summary>
    /// A component for spawning and positioning decals.
    /// </summary>
    [HelpURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-optional/decal-component")]
    [RequireComponent(typeof(EmeraldEvents))]
    public class EmeraldDecals : MonoBehaviour
    {
        #region Decals Variables
        public List<GameObject> BloodEffects = new List<GameObject>();
        [Range(0, 3f)]
        public float BloodSpawnHeight = 0.33f;
        [Range(0, 3f)]
        public float BloodSpawnDelay = 0;
        [Range(0f, 3f)]
        public float BloodSpawnRadius = 0.6f;
        [Range(3f, 60f)]
        public int BloodDespawnTime = 16;
        [Range(1, 100)]
        public int OddsForBlood = 100;
        EmeraldEvents EmeraldEventsComponent;
        EmeraldSystem EmeraldComponent;
        #endregion

        #region Editor Variables
        public bool HideSettingsFoldout;
        public bool DecalsFoldout;
        public bool MessageDismissed;
        #endregion

        void Start()
        {
            Initialize();
        }

        /// <summary>
        /// Initialize the Events Component.
        /// </summary>
        void Initialize ()
        {
            EmeraldComponent = GetComponent<EmeraldSystem>();
            EmeraldEventsComponent = GetComponent<EmeraldEvents>();
            EmeraldEventsComponent.OnTakeDamageEvent.AddListener(() => { CreateBloodSplatter(); });
        }

        public void CreateBloodSplatter()
        {
            Invoke("DelayCreateBloodSplatter", BloodSpawnDelay);
        }

        void DelayCreateBloodSplatter()
        {
            var Odds = Random.Range(0, 101);

            if (Odds <= OddsForBlood && EmeraldComponent != null && !EmeraldComponent.AnimationComponent.IsBlocking)
            {
                GameObject BloodEffect = EmeraldAI.Utility.EmeraldObjectPool.SpawnEffect(BloodEffects[Random.Range(0, BloodEffects.Count)], transform.position + Random.insideUnitSphere * BloodSpawnRadius, Quaternion.identity, BloodDespawnTime);
                BloodEffect.transform.position = new Vector3(BloodEffect.transform.position.x, transform.position.y, BloodEffect.transform.position.z);
                BloodEffect.transform.rotation = Quaternion.AngleAxis(Random.Range(55, 125), Vector3.right) * Quaternion.AngleAxis(Random.Range(10, 350), Vector3.forward);
                BloodEffect.transform.localScale = Vector3.one * Random.Range(0.8f, 1f);
            }
        }
    }
}