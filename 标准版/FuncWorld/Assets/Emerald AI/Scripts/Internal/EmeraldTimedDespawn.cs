using UnityEngine;

namespace EmeraldAI.Utility
{
    public class EmeraldTimedDespawn : MonoBehaviour
    {
        public float SecondsToDespawn = 3;
        float Timer;

        void Update()
        {
            Timer += Time.deltaTime;
            if (Timer >= SecondsToDespawn)
            {
                EmeraldObjectPool.Despawn(gameObject);
            }
        }

        void OnDisable()
        {
            Timer = 0;
        }
    }
}