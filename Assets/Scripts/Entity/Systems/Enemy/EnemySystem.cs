using UnityEngine;
using Game.Systems;
using Game.Spirit;
using System.Collections;
using System.Collections.Generic;
using Game.Data;
using Game.Enemy.Data;
using Game.Spirit.System;
using System;

namespace Game.Enemy
{
    public class EnemySystem : IAbilitiySystem, ITraitSystem, IHealthComponent
    {
        public event EventHandler<EnemySystem> LastWaypointReached = delegate { };

        public EnemyData Data { get; set; }
        public int WaypointIndex { get; set; }
        public IDamageDealer LastDamageDealer { get; set; }
        public AbilityControlSystem AbilityControlSystem { get; set; }
        public TraitControlSystem TraitControlSystem { get; set; }
        public HealthSystem HealthSystem { get; set; }
        public GameObject Prefab { get; set; }
        public bool IsOn { get; set; }
        public IEntitySystem OwnerSystem { get; set; }
        public List<int> InstanceId { get; set; }
        public List<IHealthComponent> Targets { get; set; }
        public List<ITraitHandler> TraitSystems { get; set; }
        public List<AbilitySystem> AbilitySystems { get; set; }

        private GameObject[] groundWaypoints;
        private GameObject[] flyingWaypoints;

        public EnemySystem(GameObject ownerPrefab)
        {
            AbilitySystems = new List<AbilitySystem>();
            TraitSystems = new List<ITraitHandler>();
            Targets = new List<IHealthComponent>();
            AbilityControlSystem = new AbilityControlSystem(this);
            TraitControlSystem = new TraitControlSystem(this);

            Prefab = ownerPrefab;
        }

        public void SetSystem(PlayerSystem player)
        {
            OwnerSystem = player;
            this.SetId();

            groundWaypoints = player.Map.GroundWaypoints;
            flyingWaypoints = player.Map.FlyingWaypoints;

            HealthSystem = new HealthSystem(this) { IsVulnerable = true };

            for (int i = 0; i < Data.Abilities.Count; i++)
            {
                AbilitySystems.Add(new AbilitySystem(Data.Abilities[i], this));
                AbilitySystems[AbilitySystems.Count - 1].Set(this);
            }

            for (int i = 0; i < Data.Traits.Count; i++)
            {
                TraitSystems.Add(Data.Traits[i].GetSystem(this));
                TraitSystems[TraitSystems.Count - 1].Set();
            }

            AbilityControlSystem.Set();
            TraitControlSystem.Set();
        }

        public void UpdateSystem()
        {
            var waypoints = Data.Type == EnemyType.Flying ?
                flyingWaypoints : 
                groundWaypoints;

            HealthSystem?.UpdateSystem();
            AbilityControlSystem.UpdateSystem();

            if (IsOn)
            {
                var waypointTransform = waypoints[WaypointIndex].transform;
                var enemyTransform = Prefab.transform;
                var waypointReached = enemyTransform.position.GetDistanceTo(waypointTransform.position) < 30;

                if (WaypointIndex < waypoints.Length - 1)
                    if (!waypointReached)
                        MoveAndRotateEnemy();
                    else
                        WaypointIndex++;
                else
                    LastWaypointReached?.Invoke(this, this);
            }

            #region  Helper functions

            void MoveAndRotateEnemy()
            {
                var lookRotation =
                    Quaternion.LookRotation(waypoints[WaypointIndex].transform.position - Prefab.transform.position);
                var rotation =
                    Quaternion.Lerp(Prefab.transform.rotation, lookRotation, Time.deltaTime * 10f);

                rotation.z = 0;
                rotation.x = 0;

                Prefab.transform.Translate(Vector3.forward * (float)(Time.deltaTime * Data.GetValue(Numeral.MoveSpeed)), Space.Self);
                Prefab.transform.localRotation = rotation;
            }

            #endregion
        }
    }
}
