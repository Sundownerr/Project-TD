using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Enums;
using Game.Utility;
using Game.Systems.Spirit;
using Game.Systems.Enemy;

namespace Game.Systems
{
    public class RangeSystem : ExtendedMonoBehaviour
    {
        public List<IVulnerable> EntitySystems { get; set; } = new List<IVulnerable>();
        public List<GameObject> Entities { get; set; } = new List<GameObject>();
        public IPrefabComponent Owner { get; set; }
        public CollideWith CollideType { get; set; }
        public event Action<IVulnerable> EntityEntered;
        public event Action<IVulnerable> EntityExit;
        public event Action Destroyed;

        Renderer rend;
        Color transparent, notTransparent;
        bool isRangeShowed;

        protected override void Awake()
        {
            base.Awake();

            rend = GetComponent<Renderer>();
            transform.position += new Vector3(0, -5, 0);

            transparent = new Color(0f, 0f, 0f, 0f);
            notTransparent = new Color(0, 0.5f, 0, 0.2f);
        }

        void OnDestroy()
        {
            Destroyed?.Invoke();
        }

        void OnTriggerEnter(Collider other)
        {
            AddToList();

            void AddToList()
            {
                if (CollideType == CollideWith.Enemies)
                {
                    AddEntity<EnemySystem>();
                    return;
                }

                if (CollideType == CollideWith.Spirits)
                {
                    AddEntity<SpiritSystem>();
                    return;
                }

                if (CollideType == CollideWith.EnemiesAndSpirits)
                {
                    AddEntity<EnemySystem>();
                    AddEntity<SpiritSystem>();
                }

                void AddEntity<T>() where T : IVulnerable
                {
                    var owner = (Owner as IEntitySystem).GetOwnerOfType<PlayerSystem>();

                    if (typeof(T) == typeof(EnemySystem))
                    {
                        for (int i = 0; i < owner.EnemyControlSystem.AllEnemies.Count; i++)
                        {
                            if (CheckFound(owner.EnemyControlSystem.AllEnemies[i]))
                            {
                                return;
                            }
                        }
                    }

                    if (typeof(T) == typeof(SpiritSystem))
                    {
                        for (int i = 0; i < owner.SpiritControlSystem.AllSpirits.Count; i++)
                        {
                            if (CheckFound(owner.SpiritControlSystem.AllSpirits[i]))
                            {
                                return;
                            }
                        }
                    }
                }

                bool CheckFound(IVulnerable entitySystem)
                {
                    if (other.gameObject == entitySystem.Prefab || other.gameObject == entitySystem.Prefab.transform.GetChild(0).gameObject)
                    {
                        EntitySystems.Add(entitySystem);
                        Entities.Add(entitySystem.Prefab);
                        EntityEntered?.Invoke(entitySystem);

                        return true;
                    }

                    return false;
                }
            }
        }

        void OnTriggerExit(Collider other)
        {
            for (int i = 0; i < EntitySystems.Count; i++)
            {
                if (other.gameObject == EntitySystems[i].Prefab)
                {
                    EntityExit?.Invoke(EntitySystems[i]);
                    EntitySystems.Remove(EntitySystems[i]);
                    Entities.Remove(other.gameObject);
                }
            }
        }

        void OnTriggerStay(Collider other)
        {
            for (int i = 0; i < EntitySystems.Count; i++)
            {
                if (EntitySystems[i] == null || EntitySystems[i].Prefab == null)
                {
                    EntityExit?.Invoke(EntitySystems[i]);
                    Entities.RemoveAt(i);
                    EntitySystems.RemoveAt(i);
                }
            }
        }

        void Show(bool show)
        {
            isRangeShowed = show;
            rend.material.color = show ? notTransparent : transparent;
        }

        public void SetShow()
        {
            var isChoosedSpirit =
                (Owner as IEntitySystem).GetOwnerOfType<PlayerSystem>().PlayerInputSystem.ChoosedSpirit == Owner as SpiritSystem;

            if (isChoosedSpirit)
            {
                if (!isRangeShowed)
                    Show(true);
            }
            else if (isRangeShowed)
                Show(false);
        }

        public void SetShow(bool show) => Show(show);
    }
}