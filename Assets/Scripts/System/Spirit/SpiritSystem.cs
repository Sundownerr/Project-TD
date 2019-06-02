using System;
using System.Collections.Generic;
using UnityEngine;
using U = UnityEngine.Object;
using Game.Enums;
using Game.Systems.Spirit.Internal;
using Game.Systems.Enemy;
using Game.Data.SpiritEntity;
using Game.Data.Effects;
using Game.Systems.Abilities;
using Game.Utility.Creator;

namespace Game.Systems.Spirit
{
    public class SpiritSystem : IAbilitiyComponent, ITraitComponent, IDamageDealer, IAppliedEffectsComponent, IDisposable
    {
        public event Action<Data.Effect> EffectApplied;
        public event Action<Data.Effect> EffectRemoved;
        public event Action<SpiritSystem> LeveledUp;
        public event Action StatsChanged;

        public Transform RangeTransform { get; private set; }
        public Transform MovingPart { get; private set; }
        public Transform StaticPart { get; private set; }
        public Transform ShootPoint { get; private set; }
        public GameObject UsedCell { get; set; }
        public RangeSystem RangeSystem { get; private set; }
        public ShootSystem ShootSystem { get; private set; }
        public SpiritData Data { get => dataSystem.CurrentData; set => dataSystem.CurrentData = value; }
        public Abilities.ControlSystem AbilityControlSystem { get; private set; }
        public Traits.ControlSystem TraitControlSystem { get; private set; }

        public GameObject Prefab { get; private set; }
        public bool IsOn { get; set; }
        public IEntitySystem Owner { get; private set; }
        public List<IHealthComponent> Targets { get; private set; } = new List<IHealthComponent>();
        public AppliedEffectSystem AppliedEffectSystem { get; private set; }
        public bool IsOwnedByLocalPlayer { get; private set; } = true;

        SpiritDataSystem dataSystem;

        public SpiritSystem(GameObject ownerPrefab, bool isOwnedByPlayer = true)
        {
            Prefab = ownerPrefab;
            MovingPart = ownerPrefab.transform.GetChild(0);
            StaticPart = ownerPrefab.transform.GetChild(1);
            ShootPoint = MovingPart.GetChild(0).GetChild(0);

            dataSystem = new SpiritDataSystem(this);
            TraitControlSystem = new Traits.ControlSystem(this);
            ShootSystem = new ShootSystem(this, ShootPoint.position);
            AbilityControlSystem = new Abilities.ControlSystem(this, isOwnedByPlayer);
            AppliedEffectSystem = new AppliedEffectSystem();
            Prefab.layer = 14;
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

            if (!Data.Get(Enums.SpiritFlag.IsGradeSpirit).Value)
            {
                dataSystem.Set();
                dataSystem.LeveledUp += OnLeveledUp;
                dataSystem.StatsChanged += OnStatsChanged;
            }

            TraitControlSystem.Set(Data.Traits);
            AbilityControlSystem.Set(Data.Abilities);

            ShootSystem.Set(Prefab.transform.GetChild(2).gameObject);
            SetRangeSystem();

            void OnLeveledUp(SpiritSystem obj) => LeveledUp?.Invoke(this);

            void OnStatsChanged() => StatsChanged?.Invoke();

            void SetRangeSystem()
            {
                RangeSystem = Create.Range(this, Data.Get(Enums.Spirit.Range).Value, CollideWith.Enemies, OnEntityEnteredRange, OnEntityExitRange);

                void OnEntityEnteredRange(IVulnerable e)
                {
                    if (e is EnemySystem enemy)
                    {
                        if (enemy.Data.Type == EnemyType.Flying && !Data.Get(Enums.SpiritFlag.CanAttackFlying).Value)
                        {
                            return;
                        }
                        else
                        {
                            Targets.Add(e as IHealthComponent);
                        }
                    }
                }

                void OnEntityExitRange(IVulnerable e) => Targets.Remove(e as IHealthComponent);
            }
        }

        public bool CheckHaveMana(double requiredAmount) => Data.Get(Enums.Spirit.Mana).Sum >= requiredAmount;

        public void ShowRange(bool show) => RangeSystem.Show = show;

        public void UpdateSystem()
        {
            if (IsOn)
            {
                AbilityControlSystem.UpdateSystem();

                if (Targets.Count == 0)
                {
                    ShootSystem.UpdateBullets();
                }
                else
                {
                    ShootSystem.UpdateSystem(Data.Get(Enums.Spirit.AttackSpeed).Sum, Data.Get(Enums.Spirit.AttackDelay).Sum);
                    RotateAtEnemy();
                    ClearNullTargets();
                }
            }
      
            void RotateAtEnemy()
            {
                if (Targets[0].Prefab != null)
                {
                    var offset = Targets[0].Prefab.transform.position - Prefab.transform.position;
                    offset.y = 0;
                    MovingPart.rotation = Quaternion.Lerp(MovingPart.rotation, Quaternion.LookRotation(offset), Time.deltaTime * 9f);
                }
            }

            void ClearNullTargets()
            {
                for (int i = 0; i < Targets.Count; i++)
                {
                    if (Targets[i] == null || Targets[i].Prefab == null)
                    {
                        Targets.RemoveAt(i);
                    }
                }
            }
        }

        public void Upgrade(SpiritSystem previousSpirit, SpiritData newData)
        {
            UsedCell = previousSpirit.UsedCell;
            dataSystem.Upgrade(previousSpirit, newData);
        }

        public void AddExp(int amount) => dataSystem.AddExp(amount);

        public void AddEffect(Data.Effect effect)
        {
            AppliedEffectSystem.AddEffect(effect);
            EffectApplied?.Invoke(effect);
        }

        public void RemoveEffect(Data.Effect effect)
        {
            EffectRemoved?.Invoke(effect);
            AppliedEffectSystem.RemoveEffect(effect);
        }

        public int CountOf(Data.Effect effect) => AppliedEffectSystem.CountOf(effect);

        #region IDisposable Support
        bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    EffectApplied = null;
                    EffectRemoved = null;
                    LeveledUp = null;
                    StatsChanged = null;
                    U.Destroy(Data);
                    U.Destroy(Prefab);
                }
                disposedValue = true;
            }
        }

        ~SpiritSystem()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}