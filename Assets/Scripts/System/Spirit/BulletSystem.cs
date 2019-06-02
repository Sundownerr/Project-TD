using UnityEngine;
using Game.Utility;

namespace Game.Systems.Spirit.Internal
{
    public class BulletSystem : IPrefabComponent
    {
        public IHealthComponent Target { get; set; }
        public int RemainingBounceCount { get => remainingBounceCount; set => remainingBounceCount = value >= 0 ? value : 0; }

        public float Lifetime { get => lifetime; set => lifetime = value >= 0 ? value : 0; }
        public float Speed { get => speed; set => speed = value >= 0 ? value : 0; }

        public GameObject Prefab { get; set; }
        public float DistanceToTarget => Prefab.GetDistanceTo(Target.Prefab);

        bool isTargetReached;
        public bool IsTargetReached
        {
            get => isTargetReached;
            set
            {
                isTargetReached = value;
                Speed = defaultSpeed;

                Show(!value);

                void Show(bool enabled)
                {
                    for (int i = 0; i < particleSystems.Length; i++)
                    {
                        var emissionModule = particleSystems[i].emission;
                        emissionModule.enabled = enabled;

                        if (enabled)
                            particleSystems[i].Play();
                        else
                            particleSystems[i].Stop();
                    }
                }
            }

        }

        public IPrefabComponent Owner { get; set; }

        ParticleSystem[] particleSystems;

        int remainingBounceCount;
        float lifetime;
        float speed;
        float defaultSpeed;

        public BulletSystem(GameObject prefab, Vector3 startPosition)
        {
            Prefab = prefab;
            particleSystems = prefab.GetComponentsInChildren<ParticleSystem>(true);
            Prefab.layer = 13;

            Speed = 1f;

            Prefab.transform.position = startPosition;
          

            defaultSpeed = Speed;

            SetActive(true);

            Lifetime = particleSystems[0].main.startLifetime.constant;
        }

        public void Update()
        {
            Prefab.transform.LookAt(Target.Prefab.transform);
            Prefab.transform.Translate(Prefab.transform.forward * Speed, Space.World);

            if (!IsTargetReached)
            {
                Speed += 0.5f;
            }
        }

        public void SetActive(bool active)
        {
            IsTargetReached = !active;
            Prefab.SetActive(active);
        }
    }
}