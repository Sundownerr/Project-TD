﻿using UnityEngine;
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
        public IEntitySystem Owner { get; set; }
        public ID ID { get; set; }
        public List<IHealthComponent> Targets { get; set; } = new List<IHealthComponent>();
        public List<ITraitHandler> TraitSystems { get; set; } = new List<ITraitHandler>();
        public List<AbilitySystem> AbilitySystems { get; set; } = new List<AbilitySystem>();
        private Vector3[] waypoints;

        public EnemySystem(GameObject ownerPrefab, Vector3[] waypoints)
        {
            AbilityControlSystem = new AbilityControlSystem(this);
            TraitControlSystem = new TraitControlSystem(this);
            this.waypoints = waypoints;
            Prefab = ownerPrefab;
        }

        public void SetSystem(PlayerSystem player)
        {
            if (player == null)
            {
                Debug.LogError($"{this} owner player is null");
                return;
            }


            Owner = player;
            ID = new ID() { player.EnemyControlSystem.Enemies.Count };

            HealthSystem = new HealthSystem(this) { IsVulnerable = true };

            SetAbilitySystems();
            SetTraitSystems();

            #region Helper functions

            void SetTraitSystems()
            {
                for (int i = 0; i < Data.Traits.Count; i++)
                {
                    TraitSystems.Add(Data.Traits[i].GetSystem(this));
                    TraitSystems[TraitSystems.Count - 1].Set();
                }
                TraitControlSystem.Set();
            }

            void SetAbilitySystems()
            {
                for (int i = 0; i < Data.Abilities.Count; i++)
                    AbilitySystems.Add(new AbilitySystem(Data.Abilities[i], this));

                AbilityControlSystem.Set();
            }

            #endregion
        }

        public void UpdateSystem()
        {
            HealthSystem?.UpdateSystem();
            AbilityControlSystem.UpdateSystem();

            if (IsOn)
            {
                var waypointReached = Prefab.transform.position.GetDistanceTo(waypoints[WaypointIndex]) < 30;

                if (WaypointIndex < waypoints.Length - 1)
                    if (!waypointReached)
                        MoveAndRotateEnemy();
                    else
                        WaypointIndex++;
                else
                    LastWaypointReached?.Invoke(null, this);
            }
        }

        #region  Helper functions

        void MoveAndRotateEnemy()
        {
            var lookRotation =
                Quaternion.LookRotation(waypoints[WaypointIndex] - Prefab.transform.position);
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
