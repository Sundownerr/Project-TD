using System;
using Game.Systems.Traits;
using Game.Systems.Spirit;
using Game.Systems.Spirit.Internal;
using Game.Data.Traits;

namespace Game.Systems
{
    public class TraitControlSystem
    {
        public bool IsHaveChainTargets { get; set; }

        ITraitSystem owner;

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

                void OnBulletHit(BulletSystem bullet)
                {
                    var bulletTraitCount = 0;
                    var isHaveChainShot = false;

                    for (int i = 0; i < spirit.TraitSystems.Count; i++)
                    {
                        bulletTraitCount++;
                        spirit.TraitSystems[i].Apply(bullet);

                        if (spirit.TraitSystems[i] is ChainshotSystem)
                        {
                            isHaveChainShot = true;
                        }
                    }

                    if (bulletTraitCount == 0)
                    {
                        if (bullet.Target != null)
                        {
                            spirit.DealDamage(bullet.Target, spirit.Data.Get(Enums.Spirit.Damage).Sum);
                        }

                        spirit.ShootSystem.SetTargetReached(bullet);
                    }
                    else
                    {
                        if (!isHaveChainShot)
                        {
                            spirit.ShootSystem.SetTargetReached(bullet);
                        }
                    }
                }

                void OnPrepareToShoot()
                {
                    spirit.ShootSystem.ShotCount = 1;

                    var multishotTrait = spirit.Data.Traits.Find(trait => trait is Multishot) as Multishot;

                    if (multishotTrait != null)
                    {
                        var requiredShotCount = 1 + multishotTrait.Count;

                        spirit.ShootSystem.ShotCount = spirit.Targets.Count >= requiredShotCount ?
                            requiredShotCount :
                            spirit.Targets.Count;
                    }
                }

                void OnShooting(BulletSystem bullet)
                {
                    var chainshotTrait = spirit.Data.Traits.Find(trait => trait is Chainshot) as Chainshot;

                    if (chainshotTrait != null)
                    {
                        bullet.RemainingBounceCount = chainshotTrait.BounceCount;
                    }
                }
            }
        }

        public void IncreaseStatsPerLevel() => owner.TraitSystems.ForEach(traitSystem => traitSystem.IncreaseStatsPerLevel());
    }
}