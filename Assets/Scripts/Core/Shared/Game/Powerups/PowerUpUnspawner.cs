using UnityEngine;
using UnityEngine.Networking;

namespace Powerups
{
    class PowerUpUnspawner : NetworkBehaviour
    {
        //private SpawnPool _powerUpPool;
        private float _startTime;
        private float maxTime = 20;

        private void OnEnable()
        {
            _startTime = Time.time;
        }

        public void Update()
        {
            float elapsed = Time.time - _startTime;
            
            if (elapsed > maxTime && (isServer || !NetworkServer.active))
            {
                FindObjectOfType<BasePowerUpManager>().PowerUpPool.UnSpawnObject(gameObject);
                if (NetworkServer.active)
                    NetworkServer.UnSpawn(gameObject);
            }                      
        }
    }
}
