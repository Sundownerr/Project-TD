using UnityEngine;
using System.Collections.Generic;
using System;
using Game.Data.Effects;
using Game.Systems.Abilities;
using Game.Data.Enemy;

namespace Game.Systems.Enemy
{
    public class EnemySystem : IAbilitiySystem, ITraitSystem, IHealthComponent, ICanReceiveEffects
    {
        public event Action<EnemySystem> LastWaypointReached;
        public event Action<Effect> EffectApplied, EffectRemoved;
        public event Action<IHealthComponent> Died;

        public EnemyData Data { get; set; }
        public int WaypointIndex { get; set; }
        public IDamageDealer LastDamageDealer { get; set; }
        public AbilityControlSystem AbilityControlSystem { get; private set; }
        public TraitControlSystem TraitControlSystem { get; private set; }
        public HealthSystem HealthSystem { get; private set; }
        public GameObject Prefab { get; set; }
        public bool IsOn { get; set; }
        public IEntitySystem Owner { get; set; }
        public List<IHealthComponent> Targets { get; private set; } = new List<IHealthComponent>();
        public List<ITraitHandler> TraitSystems { get; private set; } = new List<ITraitHandler>();
        public List<AbilitySystem> AbilitySystems { get; private set; } = new List<AbilitySystem>();
        public AppliedEffectSystem AppliedEffectSystem { get; private set; }
        public bool IsOwnedByLocalPlayer { get; private set; }

        Vector3[] waypoints;

        public EnemySystem(GameObject ownerPrefab, Vector3[] waypoints, bool isOwnedByPlayer = true)
        {
            AbilityControlSystem = new AbilityControlSystem(this, isOwnedByPlayer);
            TraitControlSystem = new TraitControlSystem(this);
            AppliedEffectSystem = new AppliedEffectSystem();
            this.waypoints = waypoints;
            Prefab = ownerPrefab;
            Prefab.layer = 12;
            IsOwnedByLocalPlayer = isOwnedByPlayer;
        }

        public void SetSystem(PlayerSystem player)
        {
            if (player == null)
            {
                Debug.LogError($"{this} owner player is null");
                return;
            }


            Owner = player;

            HealthSystem = new HealthSystem(this) { IsVulnerable = true };

            SetAbilitySystems();
            SetTraitSystems();

            #region Helper functions

            void SetTraitSystems()
            {
                Data.Traits?.ForEach(trait =>
                {
                    TraitSystems.Add(trait.GetSystem(this));
                    TraitSystems[TraitSystems.Count - 1].Set();
                });

                TraitControlSystem.Set();
            }

            void SetAbilitySystems()
            {
                Data.Abilities?.ForEach(ability => AbilitySystems.Add(new AbilitySystem(ability, this)));

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
                    LastWaypointReached?.Invoke(this);
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

                Prefab.transform.Translate(Vector3.forward * (float)(Time.deltaTime * Data.Get(Enums.Enemy.MoveSpeed).Sum), Space.Self);
                Prefab.transform.localRotation = rotation;

            }

            #endregion
        }

        public void AddEffect(Effect effect)
        {
            AppliedEffectSystem.AddEffect(effect);

            EffectApplied?.Invoke(effect);
        }

        public void RemoveEffect(Effect effect)
        {
            AppliedEffectSystem.RemoveEffect(effect);

            EffectRemoved?.Invoke(effect);
        }

        public int CountOf(Effect effect) => AppliedEffectSystem.CountOf(effect);
        public void ChangeHealth(IDamageDealer changer, double damage) => HealthSystem.ChangeHealth(changer, damage);
        public void OnZeroHealth(IHealthComponent entity) => Died?.Invoke(entity);
    }
}
