using System.Collections.Generic;
using Game.Enemy;
using Game.Data;
using Game.Systems;
using UnityEngine;

namespace Game.Systems
{
    public class AbilityControlSystem
    {
        private IAbilitiySystem owner;
        private List<AbilitySystem> abilityStacks = new List<AbilitySystem>();
        private bool isAllEffectsEnded, isInContinueState;

        public AbilityControlSystem(IAbilitiySystem owner)
        {
            this.owner = owner;
        }

        public void Set() { }

        public void UpdateSystem()
        {
            var abilitySystems = owner.AbilitySystems;

            if (owner is EnemySystem)
                for (int i = 0; i < abilitySystems.Count; i++)
                    abilitySystems[i].Init();
            else
            if (owner.Targets.Count > 0)
            {
                isInContinueState = false;

                for (int i = 0; i < abilitySystems.Count; i++)
                {
                    if (abilitySystems[i].IsNeedStack)
                        CreateStack(i);
                    Init(abilitySystems[i], CheckTargetInRange(abilitySystems[i].Target));
                }

                for (int i = 0; i < abilityStacks.Count; i++)
                    Init(abilityStacks[i], !abilityStacks[i].CheckAllEffectsEnded());
            }
            else
            {
                isAllEffectsEnded = true;

                for (int i = 0; i < abilitySystems.Count; i++)
                    CheckContinueEffects(abilitySystems[i]);

                for (int i = 0; i < abilityStacks.Count; i++)
                    CheckContinueEffects(abilityStacks[i]);

                if (!isAllEffectsEnded)
                    ContinueEffects();
            }

            #region  Helper functions

            void CheckContinueEffects(AbilitySystem abilitySystem)
            {
                if (!abilitySystem.CheckAllEffectsEnded())
                    isAllEffectsEnded = false;
            }

            void CreateStack(int index)
            {
                var stack = new AbilitySystem(abilitySystems[index].Ability, owner);

                stack.StackReset(owner);
                stack.Target = abilitySystems[index].Target;

                abilityStacks.Add(stack);
                abilitySystems[index].IsNeedStack = false;
            }

            void ContinueEffects()
            {
                isInContinueState = true;

                for (int i = 0; i < abilitySystems.Count; i++)
                    Init(abilitySystems[i], !abilitySystems[i].CheckAllEffectsEnded());

                for (int i = 0; i < abilityStacks.Count; i++)
                    Init(abilityStacks[i], !abilityStacks[i].CheckAllEffectsEnded());
            }

            bool CheckTargetInRange(IHealthComponent target)
            {
                for (int i = 0; i < owner.Targets.Count; i++)
                    if (target == owner.Targets[i])
                        return true;
                return false;
            }

            void Init(AbilitySystem abilitySystem, bool condition)
            {
                if (abilitySystem.Target != null && condition)
                {
                    isAllEffectsEnded = false;
                    abilitySystem.Init();
                }
                else
                {
                    if (!abilitySystem.IsStacked)
                        if (!isInContinueState)
                            abilitySystem.Target = owner.Targets[0];
                        else
                        {
                            abilitySystem.CooldownReset();
                            abilitySystem.Target = null;
                        }
                    else
                    {
                        for (int i = 0; i < abilitySystem.EffectSystems.Count; i++)
                            abilitySystem.EffectSystems.Remove(abilitySystem.EffectSystems[i]);

                        abilitySystem.EffectSystems.Clear();
                        abilityStacks.Remove(abilitySystem);
                    }

                    isAllEffectsEnded = true;
                }
            }

            #endregion
        }
    }
}