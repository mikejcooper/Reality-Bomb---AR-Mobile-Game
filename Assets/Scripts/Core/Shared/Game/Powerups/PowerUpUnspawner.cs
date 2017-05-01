using UnityEngine;
using UnityEngine.Networking;

namespace Powerups
{
    class PowerUpUnspawner : MonoBehaviour
    {
        //private SpawnPool _powerUpPool;
        private float _startTime;
        private float maxTime = 20;

        public delegate void OnTimeOut(GameObject go);
        public static event OnTimeOut OnTimeOutEvent;

        private void OnEnable()
        {
            _startTime = Time.time;
        }

        public void Update()
        {
            float elapsed = Time.time - _startTime;
            
            if (elapsed > maxTime)
            {
                OnTimeOutEvent(gameObject);
            }                      
        }
    }
}
