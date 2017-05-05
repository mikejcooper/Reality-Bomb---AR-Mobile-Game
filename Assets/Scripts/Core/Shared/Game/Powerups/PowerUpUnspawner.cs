using UnityEngine;
using UnityEngine.Networking;

namespace Powerups
{
    class PowerUpUnspawner : NetworkBehaviour
    {
        //private SpawnPool _powerUpPool;
        private float _startTime;
        private float maxTime = 10;
        private bool timeUp = false;

        private ParticleSystem _ps;

        private void Start()
        {
            GetComponentInChildren<EndSpinOut>().OnEndSpinOutEvent += Recycle;
            _ps = GetComponentInChildren<ParticleSystem>();
        }

        private void OnEnable()
        {
            _startTime = Time.time;
            timeUp = false;
            _ps.Play();
        }

        private void OnDestroy()
        {
            //Not sure this really matters when we are destroying the same gameobject
            GetComponentInChildren<EndSpinOut>().OnEndSpinOutEvent -= Recycle;
        }

        public void Update()
        {
            float elapsed = Time.time - _startTime;
            
            if (elapsed > maxTime-1 && _ps.isEmitting)
                _ps.Stop();
            if (elapsed > maxTime && !timeUp)
            {
                Animation anim = GetComponentInChildren<Animation>();
                
                anim.Play("SpinOut");
                timeUp = true;                
            }                      
        }
        
        public void Recycle()
        {
            
            if (!isServer && NetworkServer.active)
                return;

            FindObjectOfType<BasePowerUpManager>().PowerUpPool.UnSpawnObject(gameObject);
            if (NetworkServer.active)
                NetworkServer.UnSpawn(gameObject);
                
        }
    
    }
}
