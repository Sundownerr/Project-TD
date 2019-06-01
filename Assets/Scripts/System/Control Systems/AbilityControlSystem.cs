using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Enums;
using Game.Systems.Abilities;
using Game.Systems.Enemy;
using Game.Systems.Spirit;
using Game.Systems.Spirit.Internal;

namespace Game.Systems
{
    public class AbilityControlSystem
    {
        IAbilitiySystem owner;
        List<AbilitySystem> abilityStacks = new List<AbilitySystem>();
        bool isAllEffectsEnded, isOwnedByPlayer;

        public AbilityControlSystem(IAbilitiySystem owner, bool isOwnedByPlayer)
        {
            this.owner = owner;
            this.isOwnedByPlayer = isOwnedByPlayer;
        }

        public void Set()
        {
            if (owner is SpiritSystem spirit)
            {
                spirit.ShootSystem.Shooting += OnSpiritShooting;
            }

            void OnSpiritShooting(BulletSystem obj)
            {
                owner.AbilitySystems.ForEach(abilitySystem =>
                {
                    if (abilitySystem.Ability.IsUsedWhenShoot)
                    {
                        abilitySystem.SetTarget(owner.Targets[0] as ICanReceiveEffects);
                        abilitySystem.Init();
                    }
                });
            }
        }

        public void UseAbility(int abilityIndex)
        {
            if (isOwnedByPlayer)
            {
                return;
            }

            owner.AbilitySystems[abilityIndex].Init();
        }

        public void UpdateSystem()
        {
            var abilitySystems = owner.AbilitySystems;

            if (owner is EnemySystem)
            {
                InitEnemyAbilities();
            }
            else
            {
                if (!isOwnedByPlayer)
                {
                    return;
                }

                InitSpiritAbilities();
            }

            void InitEnemyAbilities()
            {
                abilitySystems.ForEach(system =>
                {
                    if (owner.CheckHaveMana(system.Ability.ManaCost))
                    {
                        system.Init();
                    }
                });
            }

            void InitSpiritAbilities()
            {
                abilitySystems.ForEach(abilitySystem =>
                {
                    if (owner.CheckHaveMana(abilitySystem.Ability.ManaCost))
                    {
                        if (!abilitySystem.Ability.IsUsedWhenShoot)
                        {
                            if (owner.Targets.Count > 0)
                            {
                                abilitySystem.SetTarget(owner.Targets[0] as ICanReceiveEffects);
                                Init(abilitySystem, CheckTargetInRange(abilitySystem.Target));
                            }
                            else
                            {
                                abilitySystem.SetTarget(null);
                                Init(abilitySystem, !abilitySystem.CheckAllEffectsEnded());
                            }

                            if (abilitySystem.IsNeedStack)
                            {
                                abilityStacks.Add(CreateStack(abilitySystem));
                                abilitySystem.IsNeedStack = false;
                            }
                        }
                    }
                });

                InitStacks();

                void InitStacks()
                {
                    for (int i = 0; i < abilityStacks.Count; i++)
                    {
                        Init(abilityStacks[i], !abilityStacks[i].CheckAllEffectsEnded());
                    }
                }
            }

            AbilitySystem CreateStack(AbilitySystem baseAbilitySystem)
            {
                var stack = new AbilitySystem(baseAbilitySystem.Ability, owner);

                stack.SetTarget(baseAbilitySystem.Target);
                stack.StackReset(baseAbilitySystem);

                return stack;
            }

            bool CheckTargetInRange(ICanReceiveEffects target) =>
                owner.Targets.Find(targetInRange => target == targetInRange) != null;

            void Init(AbilitySystem abilitySystem, bool condition)
            {
                if (abilitySystem.Target != null && condition)
                {
                    abilitySystem.Init();
                }
                else if (abilitySystem.IsStacked && abilitySystem.CheckAllEffectsEnded())
                {
                    abilityStacks.Remove(abilitySystem);
                }
            }
        }
    }
}