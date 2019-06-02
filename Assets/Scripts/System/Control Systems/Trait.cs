using System;
using Game.Systems.Traits;
using Game.Systems.Spirit;
using Game.Systems.Spirit.Internal;
using Game.Data.Traits;
using System.Collections.Generic;

namespace Game.Systems.Traits
{
    public class ControlSystem
    {
        public bool IsHaveChainTargets { get; set; }
        List<ITraitSystem> traitSystems = new List<ITraitSystem>();

        ITraitComponent owner;

        public ControlSystem(ITraitComponent owner)
        {
            this.owner = owner;
        }

        public void Set(List<Trait> traits)
        {
            traits?.ForEach(trait =>
            {
                traitSystems.Add(trait.GetSystem(owner));
                traitSystems[traitSystems.Count - 1].Set();
            });

            if (owner is SpiritSystem spirit)
            {
                spirit.ShootSystem.BulletHit += OnBulletHit;
                spirit.ShootSystem.PrepareToShoot += OnPrepareToShoot;
                spirit.ShootSystem.Shooting += OnShooting;

                void OnBulletHit(BulletSystem bullet)
                {
                    var bulletTraitCount = 0;
                    var isHaveChainShot = false;

                    for (int i = 0; i < traitSystems.Count; i++)
                    {
                        bulletTraitCount++;
                        traitSystems[i].Apply(bullet);

                        if (traitSystems[i] is Traits.Chainshot)
                        {
                            isHaveChainShot = true;
                        }
                    }

                    if (bulletTraitCount == 0)
                    {
                        if (bullet.Target != null)
                        {
                            spirit.DealDamage(bullet.Target, spirit.DamageInstance);
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

                    var multishotTrait = spirit.Data.Traits.Find(trait => trait is Data.Traits.Multishot);

                    if (multishotTrait != null)
                    {
                        var requiredShotCount = 1 + (multishotTrait as Data.Traits.Multishot).Count;

                        spirit.ShootSystem.ShotCount = spirit.Targets.Count >= requiredShotCount ?
                            requiredShotCount :
                            spirit.Targets.Count;
                    }
                }

                void OnShooting(BulletSystem bullet)
                {
                    var chainshotTrait = spirit.Data.Traits.Find(trait => trait is Data.Traits.Chainshot) as Data.Traits.Chainshot;

                    if (chainshotTrait != null)
                    {
                        bullet.RemainingBounceCount = chainshotTrait.BounceCount;
                    }
                }
            }
        }

        public void LevelUp() => traitSystems.ForEach(traitSystem => traitSystem.LevelUp());
    }
}