using System;
using System.Collections.Generic;
using Game.Enemy;
using Game.Data;
using Game.Systems;
using Game.Spirit;
using UnityEngine;

namespace Game.Systems
{
    public class TraitControlSystem
    {
        public bool IsHaveChainTargets { get; set; }

        private ITraitSystem owner;

        public TraitControlSystem(ITraitSystem owner)
        {
            this.owner = owner;
        }

        public void Set()
        {
            if (owner is SpiritSystem spirit)
            {
                spirit.ShootSystem.BulletHit += OnBulletHit;
                spirit.ShootSystem.PrepareToShoot += OnPrepareToShoot;
                spirit.ShootSystem.Shooting += OnShooting;

                void OnBulletHit(object _, BulletSystem bullet)
                {
                    var bulletTraitCount = 0;
                    var isHaveChainShot = false;

                    for (int i = 0; i < spirit.TraitSystems.Count; i++)
                    {
                        bulletTraitCount++;
                        spirit.TraitSystems[i].Apply(bullet);

                        if (spirit.TraitSystems[i] is ChainshotSystem)
                            isHaveChainShot = true;
                    }

                    if (bulletTraitCount == 0)
                    {
                        if (bullet.Target != null)
                            spirit.DealDamage(bullet.Target, spirit.Data.Get(Enums.Spirit.Damage).Sum);

                        spirit.ShootSystem.SetTargetReached(bullet);
                    }
                    else
                    {
                        if (!isHaveChainShot)
                            spirit.ShootSystem.SetTargetReached(bullet);
                    }
                }

                void OnPrepareToShoot(object _, EventArgs e)
                {
                    spirit.ShootSystem.ShotCount = 1;

                    for (int i = 0; i < spirit.Data.Traits.Count; i++)
                        if (spirit.Data.Traits[i] is Multishot multishot)
                        {
                            var enemies = spirit.Targets;
                            var requiredShotCount = 1 + multishot.Count;

                            spirit.ShootSystem.ShotCount =
                                enemies.Count >= requiredShotCount ? requiredShotCount : enemies.Count;
                        }
                }

                void OnShooting(object _, BulletSystem bullet)
                {
                    for (int i = 0; i < spirit.Data.Traits.Count; i++)
                        if (spirit.Data.Traits[i] is Chainshot chainshot)
                            bullet.RemainingBounceCount = chainshot.BounceCount;
                }
            }
        }

        public void IncreaseStatsPerLevel()
        {
            for (int i = 0; i < owner.TraitSystems.Count; i++)
                owner.TraitSystems[i].IncreaseStatsPerLevel();
        }
    }
}