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
        public Transform RangeTransform { get;  set; }
        public Transform MovingPart { get;  set; }
        public Transform StaticPart { get;  set; }
        public Transform ShootPoint { get;  set; }
        public GameObject UsedCell { get; set; }
        public GameObject Bullet { get;  set; }
        public GameObject Range { get;  set; }
        public RangeSystem RangeSystem { get;  set; }
        public ShootSystem ShootSystem { get;  set; }
        public SpiritDataSystem DataSystem { get;  set; }
        public SpiritData Data { get => DataSystem.CurrentData; set => DataSystem.CurrentData = value; }
        public Renderer[] Renderers { get; private set; }
        public AbilityControlSystem AbilityControlSystem { get; set; }
        public TraitControlSystem TraitControlSystem { get; set; }             
        public HealthSystem HealthSystem { get ; set ; }
        public GameObject Prefab { get; set ; }
        public bool IsOn { get; set; }
        public IEntitySystem OwnerSystem { get ; set ; }
        public List<int> InstanceId { get; set; }
        public List<IHealthComponent> Targets { get; set; }
        public List<ITraitHandler> TraitSystems { get; set; }
        public List<AbilitySystem> AbilitySystems { get; set; }

        public SpiritSystem(GameObject ownerPrefab)
        {
            Prefab = ownerPrefab;
            MovingPart = ownerPrefab.transform.GetChild(0);
            StaticPart = ownerPrefab.transform.GetChild(1);
            ShootPoint = MovingPart.GetChild(0).GetChild(0);
            Bullet = ownerPrefab.transform.GetChild(2).gameObject;

            DataSystem = new SpiritDataSystem(this);
            HealthSystem = new HealthSystem(this);
            TraitControlSystem = new TraitControlSystem(this);
            ShootSystem = new ShootSystem(this);
            AbilityControlSystem = new AbilityControlSystem(this);
            AbilitySystems = new List<AbilitySystem>();
            TraitSystems = new List<ITraitHandler>();
            Targets = new List<IHealthComponent>();

            Bullet.SetActive(false);
            HealthSystem.IsVulnerable = false;
        }

        public void SetSystem(PlayerSystem player)
        {
            OwnerSystem = player;
            this.SetId();

            if (!Data.IsGradeSpirit)
                DataSystem.Set();

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

            ShootSystem.Set();
            AbilityControlSystem.Set();
            TraitControlSystem.Set();

            RangeSystem = StaticMethods.CreateRange(this, Data.Get(Numeral.Range, From.Base).Value, CollideWith.Enemies);
            RangeSystem.EntityEntered += OnEntityEnteredRange;
            RangeSystem.EntityExit += OnEntityExitRange;
            Renderers = Prefab.GetComponentsInChildren<Renderer>();
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