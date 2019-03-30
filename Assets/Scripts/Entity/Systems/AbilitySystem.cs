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
        public event EventHandler<AbilitySystem> Used = delegate { };
        public ID ID { get; private set; }
        public IEntitySystem Owner { get; private set; }
        public bool IsStacked { get; private set; }
        public bool IsNeedStack { get; set; }
        public List<EffectSystem> EffectSystems { get; set; } = new List<EffectSystem>();
        public Ability Ability { get; private set; }
        public IHealthComponent Target { get; private set; }

        private int effectCount;
        private float cooldownTimer, nextEffectTimer;
        private bool used;

        public AbilitySystem(Ability ability, IAbilitiySystem owner)
        {
            Ability = ability;

            for (int i = 0; i < Ability.Effects.Count; i++)
                EffectSystems.Add(Ability.Effects[i].EffectSystem);

            SetSystem(owner);
        }

        public void Init()
        {
            if (!IsStacked)
                if (cooldownTimer < Ability.Cooldown)
                {
                    if (!used)
                    {
                        used = true;
                        Used?.Invoke(null, this);
                    }
                    cooldownTimer += Time.deltaTime;
                }
                else
                {
                    used = false;
                    cooldownTimer = 0;
                    IsNeedStack = CheckNeedStack();
                    CooldownReset();
                }

            nextEffectTimer = nextEffectTimer > Ability.Effects[effectCount].NextInterval ? 0 : nextEffectTimer + Time.deltaTime;

            for (int i = 0; i <= effectCount; i++)
                EffectSystems[i].Init();

            if (!(effectCount >= Ability.Effects.Count - 1))
                if (nextEffectTimer > Ability.Effects[effectCount].NextInterval)
                    effectCount++;

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

            #endregion
        }

        public void SetTarget(IHealthComponent target)
        {
            Target = target;

            for (int i = 0; i < EffectSystems.Count; i++)
                EffectSystems[i].SetTarget(target);
        }

        public void StackReset(IAbilitiySystem owner)
        {
            IsStacked = true;
            SetSystem(owner);
        }

        private void SetSystem(IAbilitiySystem owner)
        {
            Owner = owner;
            ID = new ID(owner.ID);
            ID.Add(owner.AbilitySystems.IndexOf(this));

            for (int i = 0; i < EffectSystems.Count; i++)
            {
                EffectSystems[i].SetSystem(this);

                if (EffectSystems[i] is IDamageDealerChild child)
                    child.OwnerDamageDealer = this.GetOwnerOfType<IDamageDealer>();
            }

            Ability.Effects[Ability.Effects.Count - 1].NextInterval = 0.01f;
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
