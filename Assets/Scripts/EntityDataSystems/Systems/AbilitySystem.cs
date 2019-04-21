using System;
using System.Collections;
using System.Collections.Generic;
using Game.Enemy;
using Game.Data;
using Game.Spirit;
using UnityEngine;
namespace Game.Systems
{
    public class AbilitySystem : IEntitySystem
    {
        public event EventHandler<AbilitySystem> Used;
        public ID ID { get; private set; }
        public IEntitySystem Owner { get; private set; }
        public bool IsStacked { get; private set; }
        public bool IsNeedStack { get; set; }
        public bool IsCooldowned { get; private set; } = true;
        public List<EffectSystem> EffectSystems { get; set; } = new List<EffectSystem>();
        public Ability Ability { get; private set; }
        public IHealthComponent Target { get; private set; }

        int effectCount;
        WaitForSeconds cooldownDelay;
        List<WaitForSeconds> nextEffectDelays = new List<WaitForSeconds>();
        List<Coroutine> effectCoroutines = new List<Coroutine>();

        public AbilitySystem(Ability ability, IAbilitiySystem owner)
        {
            Ability = ability;
            cooldownDelay = new WaitForSeconds(Ability.Cooldown);

            Ability.Effects.ForEach(effect =>
            {
                EffectSystems.Add(effect.EffectSystem);
                nextEffectDelays.Add(new WaitForSeconds(effect.NextInterval));
            });

            SetSystem(owner);
        }

        void SetSystem(IAbilitiySystem owner)
        {
            Owner = owner;
            ID = new ID(owner.ID);
            ID.Add(owner.AbilitySystems.IndexOf(this));

            EffectSystems.ForEach(effectSystem =>
            {
                effectSystem.SetSystem(this);

                if (effectSystem is IDamageDealerChild child)
                    child.OwnerDamageDealer = this.GetOwnerOfType<IDamageDealer>();
            });

            Ability.Effects[Ability.Effects.Count - 1].NextInterval = 0.01f;
        }

        public void Init()
        {
            if (!IsStacked)
                if (IsCooldowned)
                {
                    IsCooldowned = false;
                    Used?.Invoke(null, this);
                    GameLoop.Instance.StartCoroutine(Cooldown());
                    effectCoroutines.Add(GameLoop.Instance.StartCoroutine(InitEffect()));
                }

            #region  Helper functions

            IEnumerator Cooldown()
            {
                yield return cooldownDelay;

                CooldownReset();
            }

            IEnumerator InitEffect()
            {
                for (int i = 0; i <= effectCount; i++)
                    EffectSystems[i].Apply();

                yield return nextEffectDelays[effectCount];

                if (effectCount < Ability.Effects.Count - 1)
                {
                    effectCount++;
                    effectCoroutines.Add(GameLoop.Instance.StartCoroutine(InitEffect()));
                }
            }

            #endregion
        }

        public void SetTarget(IHealthComponent target)
        {
            Target = target;
            EffectSystems.ForEach(effectSystem => effectSystem.SetTarget(target as ICanReceiveEffects));
        }

        public void StackReset(IAbilitiySystem owner)
        {
            IsStacked = true;
            SetSystem(owner);
        }

        public void CooldownReset()
        {
            effectCount = 0;
            IsCooldowned = true;

            IsNeedStack = CheckNeedStack();
            EffectSystems.ForEach(effectSystem => effectSystem.ApplyRestart());
            effectCoroutines.ForEach(coroutine => GameLoop.Instance.StopCoroutine(coroutine));
            effectCoroutines.Clear();

            #region helper functions

            bool CheckNeedStack()
            {
                for (int i = 0; i < EffectSystems.Count; i++)
                    if (Ability.Effects[i].MaxStackCount > 1)
                    {
                        if (!EffectSystems[i].IsEnded)
                            return true;
                    }
                    else
                    if (EffectSystems[i].Target != Target)
                        return true;
                return false;
            }

            #endregion
        }

        public bool CheckAllEffectsEnded()
        {
            foreach (var effectSystem in EffectSystems)
                if (!effectSystem.IsEnded)
                    return false;
            return true;
        }
    }
}
