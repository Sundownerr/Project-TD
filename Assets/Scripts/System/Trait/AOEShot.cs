﻿using UnityEngine;
using Game.Systems.Spirit;
using Game.Data.Traits;

namespace Game.Systems.Traits
{
    public class AOEShot : ITraitSystem
    {
        public ITraitComponent Owner { get; set; }

        Data.Traits.AOEShot trait;

        public AOEShot(Data.Traits.AOEShot trait, ITraitComponent owner)
        {
            this.trait = trait;
            Owner = owner;
        }

        public void IncreaseStatsPerLevel()
        {
            // Debug.Log("increase stats per level");
        }

        public void Apply(IPrefabComponent bullet)
        {
            var enemyLayer = 1 << 12;
            var colliders = new Collider[40];
            var spirit = Owner as SpiritSystem;
            var player = spirit.GetOwnerOfType<PlayerSystem>();

            var hitTargetCount = Physics.OverlapSphereNonAlloc(
                bullet.Prefab.transform.position,
                trait.Range,
                colliders,
                enemyLayer);

            for (int i = 0; i < hitTargetCount; i++)
                for (int j = 0; j < player.EnemyControlSystem.AllEnemies.Count; j++)
                    if (colliders[i].transform.gameObject == player.EnemyControlSystem.AllEnemies[j].Prefab)
                        spirit.DealDamage(player.EnemyControlSystem.AllEnemies[i], spirit.Data.Get(Enums.Spirit.Damage).Sum);
        }

        public void Set()
        {

        }
    }
}