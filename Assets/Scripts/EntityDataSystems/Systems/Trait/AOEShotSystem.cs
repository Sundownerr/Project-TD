using System.Collections;
using System.Collections.Generic;
using Game.Data;
using Game.Systems;
using Game.Spirit;
using UnityEngine;

namespace Game.Systems
{
    public class AOEShotSystem : ITraitHandler
    {
        public ITraitSystem Owner { get; set; }

        private AOEShot trait;

        public AOEShotSystem(AOEShot trait, ITraitSystem owner)
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
                for (int j = 0; j < player.Enemies.Count; j++)
                    if (colliders[i].transform.gameObject == player.Enemies[j].Prefab)
                        spirit.DealDamage(player.Enemies[i], spirit.Data.Get(Enums.Spirit.Damage).Sum);
        }

        public void Set()
        {
            
        }
    }
}