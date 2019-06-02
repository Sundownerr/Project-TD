using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Enums;
using Game.Utility;
using Game.Systems.Spirit;
using Game.Systems.Enemy;
using Game.Consts;

namespace Game.Systems
{
    public class RangeSystem : ExtendedMonoBehaviour
    {

        public event Action<IVulnerable> EntityEntered;
        public event Action<IVulnerable> EntityExit;
        public event Action Destroyed;

        public List<IVulnerable> EntitySystems { get; private set; } = new List<IVulnerable>();
        public List<GameObject> Entities { get; private set; } = new List<GameObject>();
        public IPrefabComponent Owner { get; set; }
        public CollideWith CollideType { get; set; }

        bool show;
        public bool Show { set => rend.enabled = value; }

        MeshRenderer rend;
        Color transparent, notTransparent;
        bool isRangeShowed;

        protected override void Awake()
        {
            base.Awake();

            rend = GetComponent<MeshRenderer>();
            transform.position += new Vector3(0, -5, 0);
            Show = false;

            transparent = new Color(0f, 0f, 0f, 0f);
            notTransparent = new Color(0, 0.5f, 0, 0.2f);
        }

        void OnDestroy()
        {
            EntityEntered = null;
            EntityExit = null;
            Destroyed?.Invoke();
            Destroyed = null;
        }

        void OnTriggerEnter(Collider other)
        {
            var isTagOk = other.gameObject.CompareTag(CollideType == CollideWith.Enemies ? StringConsts.EnemyTag : StringConsts.SpiritTag);

            if (!isTagOk)
                return;

            var newEntity = GetEntityFromList();

            EntitySystems.Add(newEntity);
            Entities.Add(newEntity.Prefab);
            EntityEntered?.Invoke(newEntity);

            IVulnerable GetEntityFromList()
            {
                var player = (Owner as IEntitySystem).GetOwnerOfType<PlayerSystem>();
                IVulnerable findedEntity;

                if (CollideType == CollideWith.Enemies)
                {
                    findedEntity = player.EnemyControlSystem.AllEnemies.Find(enemy =>
                        other.gameObject == enemy.Prefab ||
                        other.gameObject == enemy.Prefab.transform.GetChild(0).gameObject) as IVulnerable;
                }
                else
                {
                    findedEntity = player.SpiritControlSystem.AllSpirits.Find(spirit =>
                        other.gameObject == spirit.Prefab ||
                        other.gameObject == spirit.Prefab.transform.GetChild(0).gameObject) as IVulnerable;
                }

                if (findedEntity != null)
                {
                    return findedEntity;
                }
                else
                {
                    Debug.LogError("cant find entered entity");
                    return null;
                }
            }
        }

        void OnTriggerExit(Collider other)
        {
            for (int i = 0; i < EntitySystems.Count; i++)
            {
                if (other.gameObject == EntitySystems[i].Prefab)
                {
                    RemoveEntity(i);
                }
            }
        }

        void OnTriggerStay(Collider other)
        {
            for (int i = 0; i < EntitySystems.Count; i++)
            {
                if (EntitySystems[i] == null || EntitySystems[i].Prefab == null)
                {
                    RemoveEntity(i);
                }
            }
        }

        void RemoveEntity(int index)
        {
            EntityExit?.Invoke(EntitySystems[index]);
            EntitySystems.RemoveAt(index);
            Entities.RemoveAt(index);
        }
    }
}