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
        public List<EffectSystem> EffectSystems { get; set; } = new List<EffectSystem>();
        public Ability Ability { get; private set; }
        public IHealthComponent Target { get; private set; }

        int effectCount;
        float cooldownTimer, nextEffectTimer;
        bool isUsed;
        WaitForSeconds cooldownDelay;
        List<WaitForSeconds> nextEffectDelays = new List<WaitForSeconds>();
        List<Coroutine> nextEffectCoroutines = new List<Coroutine>();

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

            EffectSystems.ForEach(effect =>
            {
                effect.SetSystem(this);

                if (effect is IDamageDealerChild child)
                    child.OwnerDamageDealer = this.GetOwnerOfType<IDamageDealer>();
            });

            Ability.Effects[Ability.Effects.Count - 1].NextInterval = 0.01f;
        }

        public void Init()
        {
            if (!IsStacked)
                if (!isUsed)
                {
                    isUsed = true;
                    Used?.Invoke(null, this);
                    GameLoop.Instance.StartCoroutine(Cooldown());
                    nextEffectCoroutines.Add(GameLoop.Instance.StartCoroutine(NextEffectDelay()));
                }

            for (int i = 0; i <= effectCount; i++)
                EffectSystems[i].Init();

            #region  Helper functions

            bool CheckNeedStack()
            {
                for (int i = 0; i < EffectSystems.Count; i++)
                    if (Ability.Effects[i].IsStackable)
                    {
                        if (!EffectSystems[i].IsEnded)
                            return true;
                    }
                    else
                    if (EffectSystems[i].Target != Target)
                        return true;
                return false;
            }

            IEnumerator Cooldown()
            {
                yield return cooldownDelay;

                isUsed = false;
                IsNeedStack = CheckNeedStack();
                nextEffectCoroutines.ForEach(coroutine => GameLoop.Instance.StopCoroutine(coroutine));
                nextEffectCoroutines.Clear();
                CooldownReset();
            }

            IEnumerator NextEffectDelay()
            {
                yield return nextEffectDelays[effectCount];

                if (!(effectCount >= Ability.Effects.Count - 1))
                {
                    effectCount++;
                    nextEffectCoroutines.Add(GameLoop.Instance.StartCoroutine(NextEffectDelay()));
                    yield return nextEffectCoroutines[nextEffectCoroutines.Count - 1];
                }
            }

            #endregion
        }

        public void SetTarget(IHealthComponent target)
        {
            Target = target;

            for (int i = 0; i < EffectSystems.Count; i++)
                EffectSystems[i].SetTarget(target as ICanReceiveEffects);
        }

        public void StackReset(IAbilitiySystem owner)
        {
            IsStacked = true;
            SetSystem(owner);
        }


        public void CooldownReset()
        {
            effectCount = 0;
            nextEffectTimer = 0;

            for (int i = 0; i < EffectSystems.Count; i++)
                EffectSystems[i].ApplyRestart();
        }

        public bool CheckAllEffectsEnded()
        {
            for (int i = 0; i < EffectSystems.Count; i++)
                if (!EffectSystems[i].IsEnded)
                    return false;
            return true;
        }
    }
}
