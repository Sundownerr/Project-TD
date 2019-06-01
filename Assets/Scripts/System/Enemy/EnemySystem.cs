using UnityEngine;
using System.Collections.Generic;
using System;
using Game.Data.Effects;
using Game.Systems.Abilities;
using Game.Data.EnemyEntity;

namespace Game.Systems.Enemy
{
    public class EnemySystem : IAbilitiyComponent, ITraitComponent, IHealthComponent, IAppliedEffectsComponent
    {
        public event Action<EnemySystem> LastWaypointReached;
        public event Action<Data.Effect> EffectApplied;
        public event Action<Data.Effect> EffectRemoved;
        public event Action<IHealthComponent> Died;

        public Data.EnemyEntity.Enemy Data { get; set; }
        public int WaypointIndex { get; set; }
        public IDamageDealer LastDamageDealer { get; set; }
        public Abilities.ControlSystem AbilityControlSystem { get; private set; }
        public Traits.ControlSystem TraitControlSystem { get; private set; }
        public HealthSystem HealthSystem { get; private set; }
        public GameObject Prefab { get; set; }
        public bool IsOn { get; set; }
        public IEntitySystem Owner { get; set; }
        public List<IHealthComponent> Targets { get; private set; } = new List<IHealthComponent>();
        public AppliedEffectSystem AppliedEffectSystem { get; private set; }
        public bool IsOwnedByLocalPlayer { get; private set; }

        Vector3[] waypoints;

        public EnemySystem(GameObject ownerPrefab, Vector3[] waypoints, bool isOwnedByPlayer = true)
        {
            AbilityControlSystem = new Abilities.ControlSystem(this, isOwnedByPlayer);
            TraitControlSystem = new Traits.ControlSystem(this);
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

            AbilityControlSystem.Set(Data.Abilities);
            TraitControlSystem.Set(Data.Traits);
        }

        public bool CheckHaveMana(double requiredAmount) => Data.Get(Enums.Enemy.Mana).Sum >= requiredAmount;

        public void UpdateSystem()
        {
            HealthSystem?.UpdateSystem();
            AbilityControlSystem.UpdateSystem();

            if (IsOn)
            {
                var waypointReached = Prefab.GetDistanceTo(waypoints[WaypointIndex]) < 30;

                if (WaypointIndex < waypoints.Length - 1)
                {
                    if (!waypointReached)
                    {
                        MoveAndRotateEnemy();
                    }
                    else
                    {
                        WaypointIndex++;
                    }
                }
                else
                {
                    LastWaypointReached?.Invoke(this);
                }
            }

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
        }

        public void AddEffect(Data.Effect effect)
        {
            AppliedEffectSystem.AddEffect(effect);

            EffectApplied?.Invoke(effect);
        }

        public void RemoveEffect(Data.Effect effect)
        {
            AppliedEffectSystem.RemoveEffect(effect);

            EffectRemoved?.Invoke(effect);
        }

        public int CountOf(Data.Effect effect) => AppliedEffectSystem.CountOf(effect);
        public void ChangeHealth(IDamageDealer changer, double damage) => HealthSystem.ChangeHealth(changer, damage);
        public void OnZeroHealth(IHealthComponent entity) => Died?.Invoke(entity);
    }
}
