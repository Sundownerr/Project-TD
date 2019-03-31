using System;
using System.Collections.Generic;
using Game.Enemy;
using Game.Systems;
using Game.Spirit;
using UnityEngine;

namespace Game.Systems
{
    public class RangeSystem : ExtendedMonoBehaviour
    {
        public List<IVulnerable> EntitySystems { get; set; } = new List<IVulnerable>();
        public List<GameObject> Entities { get; set; } = new List<GameObject>();
        public IPrefabComponent Owner { get; set; }
        public CollideWith CollideType { get; set; }
        public event EventHandler<IVulnerable> EntityEntered = delegate{};
        public event EventHandler<IVulnerable> EntityExit = delegate{};
        public event EventHandler Destroyed = delegate { };

        private Renderer rend;
        private Color transparent, notTransparent;
        private bool isRangeShowed;

        protected override void Awake()
        {
            base.Awake();

            rend = GetComponent<Renderer>();
            transform.position += new Vector3(0, -5, 0);

            transparent = new Color(0f, 0f, 0f, 0f);
            notTransparent = new Color(0, 0.5f, 0, 0.2f);
        }

        private void OnDestroy()
        {
            Destroyed?.Invoke(null, null);
        }

        private void OnTriggerEnter(Collider other)
        {
            AddToList();

            #region  Helper functions

            void AddToList()
            {
                if (CollideType == CollideWith.Enemies)
                    AddEntity<EnemySystem>();
                    
                if (CollideType == CollideWith.Spirits)
                    AddEntity<SpiritSystem>();

                if (CollideType == CollideWith.EnemiesAndSpirits)
                {
                    AddEntity<EnemySystem>();
                    AddEntity<SpiritSystem>();
                }
                    
                #region  Helper functions

                void AddEntity<T>() where T: IVulnerable
                {
                    var owner = (Owner as IEntitySystem).GetOwnerOfType<PlayerSystem>();

                    if (typeof(T) == typeof(EnemySystem))
                        for (int i = 0; i < owner.Enemies.Count; i++)
                            if (CheckFound(owner.Enemies[i]))
                                return;

                    if (typeof(T) == typeof(SpiritSystem))
                        for (int i = 0; i < owner.Spirits.Count; i++)
                            if (CheckFound(owner.Spirits[i]))
                                return;
                }
                
                bool CheckFound(IVulnerable entitySystem)
                {
                    if (other.gameObject == entitySystem.Prefab)
                    {
                        EntitySystems.Add(entitySystem);
                        Entities.Add(entitySystem.Prefab);      
                        EntityEntered?.Invoke(null, entitySystem);
                        return true;                  
                    }       
                    return false;       
                }

                #endregion
            }

            #endregion
        }

        private void OnTriggerExit(Collider other)
        {          
            for (int i = 0; i < EntitySystems.Count; i++)
                if (other.gameObject == EntitySystems[i].Prefab)
                {
                    EntityExit?.Invoke(null, EntitySystems[i]);
                    EntitySystems.Remove(EntitySystems[i]);
                    Entities.Remove(other.gameObject);                   
                }            
        }

        private void OnTriggerStay(Collider other)
        {
            for (int i = 0; i < EntitySystems.Count; i++)
                if (EntitySystems[i] == null || EntitySystems[i].Prefab == null )
                {
                    EntityExit?.Invoke(null, EntitySystems[i]);
                    Entities.RemoveAt(i);
                    EntitySystems.RemoveAt(i);                   
                }
        }

        private void Show(bool show)
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