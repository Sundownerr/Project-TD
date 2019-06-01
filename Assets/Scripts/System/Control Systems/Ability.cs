using System.Collections.Generic;
using Game.Systems.Abilities;
using Game.Systems.Enemy;
using Game.Systems.Spirit;
using Game.Systems.Spirit.Internal;
using Game.Data.Abilities;
using System;

namespace Game.Systems.Abilities
{
    public class ControlSystem
    {
        public event Action<AbilitySystem> AbilityUsed;

        List<AbilitySystem> abilityStacks = new List<AbilitySystem>();
        List<AbilitySystem> abilitySystems = new List<AbilitySystem>();
        IAbilitiyComponent owner;
        bool isAllEffectsEnded;
        bool isOwnedByPlayer;

        public ControlSystem(IAbilitiyComponent owner, bool isOwnedByPlayer)
        {
            this.isOwnedByPlayer = isOwnedByPlayer;
            this.owner = owner;
        }

        public void Set(List<Ability> abilities)
        {
            abilities?.ForEach(ability => abilitySystems.Add(new AbilitySystem(ability, owner)));

            if (owner is SpiritSystem spirit)
            {
                spirit.ShootSystem.Shooting += OnSpiritShooting;
            }

            void OnSpiritShooting(BulletSystem obj)
            {
                abilitySystems.ForEach(abilitySystem =>
                {
                    if (abilitySystem.Ability.IsUsedWhenShoot)
                    {
                        abilitySystem.SetTarget(owner.Targets[0] as IAppliedEffectsComponent);
                        abilitySystem.Init();
                        AbilityUsed?.Invoke(abilitySystem);
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

            abilitySystems[abilityIndex].Init();
        }

        public void UpdateSystem()
        {
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
                                abilitySystem.SetTarget(owner.Targets[0] as IAppliedEffectsComponent);
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

            bool CheckTargetInRange(IAppliedEffectsComponent target) =>
                owner.Targets.Find(targetInRange => target == targetInRange) != null;

            void Init(AbilitySystem abilitySystem, bool condition)
            {
                if (abilitySystem.Target != null && condition)
                {
                    abilitySystem.Init();
                    AbilityUsed?.Invoke(abilitySystem);
                }
                else if (abilitySystem.IsStacked && abilitySystem.CheckAllEffectsEnded())
                {
                    abilityStacks.Remove(abilitySystem);
                }
            }
        }
    }
}