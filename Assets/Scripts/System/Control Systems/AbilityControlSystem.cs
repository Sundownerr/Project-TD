using System.Collections.Generic;
using Game.Systems.Abilities;
using Game.Systems.Enemy;

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

        public void Set() { }

        public void UseAbility(int abilityIndex)
        {
            if (isOwnedByPlayer) return;

            owner.AbilitySystems[abilityIndex].Init();
        }

        public void UpdateSystem()
        {
            var abilitySystems = owner.AbilitySystems;

            if (owner is EnemySystem)          
                for (int i = 0; i < abilitySystems.Count; i++)
                    abilitySystems[i].Init();

            if (!isOwnedByPlayer) return;

            for (int i = 0; i < abilitySystems.Count; i++)
            {
               
                if (owner.Targets.Count > 0)
                {              
                    abilitySystems[i].SetTarget(owner.Targets[0] as ICanReceiveEffects);
                    Init(abilitySystems[i], CheckTargetInRange(abilitySystems[i].Target));       
                }
                else
                {
                    abilitySystems[i].SetTarget(null);      
                    Init(abilitySystems[i], !abilitySystems[i].CheckAllEffectsEnded());
                }

                if (abilitySystems[i].IsNeedStack)
                    CreateStack(i);
            }

            for (int i = 0; i < abilityStacks.Count; i++)
                Init(abilityStacks[i], !abilityStacks[i].CheckAllEffectsEnded());

            #region  Helper functions

            void CreateStack(int index)
            {
                var stack = new AbilitySystem(abilitySystems[index].Ability, owner);

                stack.SetTarget(abilitySystems[index].Target);
                stack.StackReset(abilitySystems[index]);

                abilityStacks.Add(stack);
                abilitySystems[index].IsNeedStack = false;
            }

            bool CheckTargetInRange(ICanReceiveEffects target) =>
                owner.Targets.Find(targetInRange => target == targetInRange) != null;

            void Init(AbilitySystem abilitySystem, bool condition)
            {
                if (abilitySystem.Target != null && condition)
                    abilitySystem.Init();
                else
                if (abilitySystem.IsStacked && abilitySystem.CheckAllEffectsEnded())
                    abilityStacks.Remove(abilitySystem);
            }

            #endregion
        }
    }
}