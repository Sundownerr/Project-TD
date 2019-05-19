using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Managers;
using Game.Data.Abilities;
using Game.Systems.Effects;

namespace Game.Systems.Abilities
{
    public class AbilitySystem : IEntitySystem, IIndexComponent
    {
        public event Action<AbilitySystem> Used;

        public IEntitySystem Owner { get; private set; }
        public Ability Ability { get; private set; }
        public ICanReceiveEffects Target { get; private set; }
        public List<EffectSystem> EffectSystems { get; set; } = new List<EffectSystem>();
        public int CurrentEffectIndex { get; private set; }
        public bool IsStacked { get; private set; }
        public bool IsNeedStack { get; set; }
        public bool IsCooldowned { get; private set; } = true;
        public int Index { get; set; }

        WaitForSeconds cooldownDelay;
        List<WaitForSeconds> nextEffectDelays = new List<WaitForSeconds>();

        public AbilitySystem(Ability ability, IAbilitiySystem owner)
        {
            Ability = ability;
            cooldownDelay = new WaitForSeconds(Ability.Cooldown);

            Ability.Effects.ForEach(effect =>
            {
                effect = UnityEngine.Object.Instantiate(effect);
                EffectSystems.Add(effect.EffectSystem);
                nextEffectDelays.Add(new WaitForSeconds(effect.NextInterval));
            });
            
            SetSystem(owner);
        }

        void SetSystem(IAbilitiySystem owner)
        {
            Owner = owner;
            Index = owner.AbilitySystems.Count > 0 ? owner.AbilitySystems.IndexOf(this) : 0;

            SetEffects();
        }

        void SetEffects()
        {
            EffectSystems.ForEach(effectSystem =>
            {
                effectSystem.SetSystem(this);

                if (effectSystem is IDamageDealerChild child)
                {
                    child.OwnerDamageDealer = this.GetOwnerOfType<IDamageDealer>();
                }
            });

            Ability.Effects[Ability.Effects.Count - 1].NextInterval = 0.01f;
        }

        void StartAbility()
        {
            Used?.Invoke(this);
            GameLoop.Instance.StartCoroutine(Cooldown());
            GameLoop.Instance.StartCoroutine(InitEffect());

            IEnumerator Cooldown()
            {
                yield return cooldownDelay;

                if (!IsStacked)
                {
                    CooldownReset();
                }

                void CooldownReset()
                {
                    CurrentEffectIndex = 0;
                    IsCooldowned = true;
                    IsNeedStack = CheckNeedStack();

                    bool CheckNeedStack()
                    {
                        if (Target == null)
                        {
                            return false;
                        }

                        foreach (var effectSystem in EffectSystems)
                        {
                            if (effectSystem.Effect.MaxStackCount > 1)
                            {
                                if (!effectSystem.IsEnded && !effectSystem.IsMaxStackReached)
                                {
                                    return true;
                                }
                            }
                            else if (effectSystem.Target != Target && Target.CountOf(effectSystem.Effect) == 0)
                            {
                                return true;
                            }
                        }

                        return false;
                    }
                }
            }

            IEnumerator InitEffect()
            {
                EffectSystems[CurrentEffectIndex].Apply();

                yield return nextEffectDelays[CurrentEffectIndex];

                if (CurrentEffectIndex < Ability.Effects.Count - 1)
                {
                    CurrentEffectIndex++;
                    GameLoop.Instance.StartCoroutine(InitEffect());
                }
            }
        }

        public void Init()
        {
            if (IsCooldowned)
            {
                IsCooldowned = false;
                StartAbility();
            }
        }

        public void SetTarget(ICanReceiveEffects target, bool forceSet = false)
        {
            Target = target;
            EffectSystems.ForEach(effectSystem => effectSystem.SetTarget(target, forceSet));
        }

        public void StackReset(AbilitySystem baseAbility)
        {
            SetStackedSystem();

            IsStacked = true;
            CurrentEffectIndex = baseAbility.CurrentEffectIndex;

            for (int i = 0; i < CurrentEffectIndex; i++)
            {
                EffectSystems[i].End();
            }

            StartAbility();

            void SetStackedSystem()
            {
                Index = baseAbility.Index;
                SetEffects();
            }
        }

        public bool CheckAllEffectsEnded()
        {
            foreach (var effectSystem in EffectSystems)
            {
                if (!effectSystem.IsEnded)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
