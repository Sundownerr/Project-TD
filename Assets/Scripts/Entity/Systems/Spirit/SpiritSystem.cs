using System;
using System.Collections.Generic;
using Game.Enemy;
using Game.Enemy.Data;
using Game.Data;
using Game.Systems;
using Game.Spirit.Data;
using Game.Spirit.System;
using UnityEngine;
using U = UnityEngine.Object;

namespace Game.Spirit
{
    public class SpiritSystem : IAbilitiySystem, ITraitSystem, IHealthComponent, IDamageDealer
    {
        public Transform RangeTransform { get; private set; }
        public Transform MovingPart { get; private set; }
        public Transform StaticPart { get; private set; }
        public Transform ShootPoint { get; private set; }
        public GameObject UsedCell { get; set; }
        public RangeSystem RangeSystem { get; private set; }
        public ShootSystem ShootSystem { get; private set; }
        public SpiritDataSystem DataSystem { get; private set; }
        public SpiritData Data { get => DataSystem.CurrentData; set => DataSystem.CurrentData = value; }
        public Renderer[] Renderers { get; private set; }
        public AbilityControlSystem AbilityControlSystem { get; private set; }
        public TraitControlSystem TraitControlSystem { get; private set; }
        public HealthSystem HealthSystem { get; private set; }
        public GameObject Prefab { get; private set; }
        public bool IsOn { get; set; }
        public IEntitySystem Owner { get; private set; }
        public ID ID { get; private set; }
        public List<IHealthComponent> Targets { get; private set; } = new List<IHealthComponent>();
        public List<ITraitHandler> TraitSystems { get; private set; } = new List<ITraitHandler>();
        public List<AbilitySystem> AbilitySystems { get; private set; } = new List<AbilitySystem>();

        public SpiritSystem(GameObject ownerPrefab)
        {
            Prefab = ownerPrefab;
            MovingPart = ownerPrefab.transform.GetChild(0);
            StaticPart = ownerPrefab.transform.GetChild(1);
            ShootPoint = MovingPart.GetChild(0).GetChild(0);

            DataSystem = new SpiritDataSystem(this);
            HealthSystem = new HealthSystem(this);
            TraitControlSystem = new TraitControlSystem(this);
            ShootSystem = new ShootSystem(this);
            AbilityControlSystem = new AbilityControlSystem(this);

            HealthSystem.IsVulnerable = false;
        }

        public void SetSystem(PlayerSystem player)
        {
            if (player == null)
            {
                Debug.LogError($"{this} owner player is null");
                return;
            }

            Owner = player;
            ID = new ID() { player.SpiritControlSystem.Spirits.Count };

            if (!Data.IsGradeSpirit)
                DataSystem.Set();

            SetTraitSystems();
            SetAbilitySystems();
            SetShootSystem();
            SetRangeSystem();

            #region Helper functions

            void SetRangeSystem()
            {
                RangeSystem = StaticMethods.CreateRange(this, Data.Get(Numeral.Range, From.Base).Value, CollideWith.Enemies);
                RangeSystem.EntityEntered += OnEntityEnteredRange;
                RangeSystem.EntityExit += OnEntityExitRange;
                Renderers = Prefab.GetComponentsInChildren<Renderer>();
            }

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

            void SetShootSystem()
            {
                var bullet = Prefab.transform.GetChild(2).gameObject;
                bullet.SetActive(false);
                ShootSystem.Set(bullet);
            }

            #endregion
        }

        public void UpdateSystem()
        {
            RangeSystem.SetShow();

            if (IsOn)
            {
                AbilityControlSystem.UpdateSystem();

                if (Targets.Count == 0)
                    ShootSystem.MoveBullet();
                else
                {
                    ShootSystem.UpdateSystem();

                    if (Targets[0].Prefab != null)
                        RotateAtEnemy();

                    for (int j = 0; j < Targets.Count; j++)
                        if (Targets[j] == null || Targets[j].Prefab == null)
                            Targets.RemoveAt(j);

                    #region  Helper functions

                    void RotateAtEnemy()
                    {
                        var offset = Targets[0].Prefab.transform.position - Prefab.transform.position;
                        offset.y = 0;
                        MovingPart.rotation = Quaternion.Lerp(MovingPart.rotation, Quaternion.LookRotation(offset), Time.deltaTime * 9f);
                    }

                    #endregion
                }
            }
        }

        private void OnEntityEnteredRange(object _, IHealthComponent e)
        {
            if (e is EnemySystem enemy)
                if (enemy.Data.Type == EnemyType.Flying && !Data.CanAttackFlying)
                    return;
                else
                    Targets.Add(e);
        }

        private void OnEntityExitRange(object _, IHealthComponent e) => Targets.Remove(e);

        public void AddExp(int amount) => DataSystem.AddExp(amount);
    }
}