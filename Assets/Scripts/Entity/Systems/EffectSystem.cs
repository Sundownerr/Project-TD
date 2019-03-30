using System.Collections;
using System.Collections.Generic;
using Game.Data;
using UnityEngine;

namespace Game.Systems
{
    public class EffectSystem : IEntitySystem
    {
        public bool IsSet { get => isSet; set => isSet = value; }
        public bool IsEnded { get => isEnded; set => isEnded = value; }
        public bool IsMaxStackCount { get => isMaxStackCount; set => isMaxStackCount = value; }
        public IEntitySystem Owner { get; set; }
        public ID ID { get; set; }

        protected bool isSet, isEnded, isMaxStackCount;
        protected float effectTimer;
        protected Effect effect;
        protected IHealthComponent target;

        public IHealthComponent Target { get => target; private set => target = value; }

        public EffectSystem(Effect effect)
        {
            this.effect = effect;

            if (!effect.IsStackable)
                effect.MaxStackCount = 1;
        }

        public void SetSystem(AbilitySystem ownerAbility)
        {
            Owner = ownerAbility;
            ID = new ID(ownerAbility.ID);
            ID.Add(ownerAbility.EffectSystems.IndexOf(this));
        }


        public virtual void Init()
        {
            if (!IsSet)
                Apply();

            Continue();
        }

        public virtual void Apply()
        {
            if (effect.IsStackable)
                if (Target.CountOf(effect) >= effect.MaxStackCount)
                {
                    IsMaxStackCount = true;
                    return;
                }

            IsSet = true;
            IsEnded = false;
        }

        public virtual void Continue()
        {
            if (!IsEnded)
            {
                if (Target == null)
                    End();

                effectTimer = effectTimer > effect.Duration ? -1 : effectTimer += Time.deltaTime;

                if (effectTimer == -1)
                    End();
            }
        }

        public virtual void End()
        {
            if (!IsMaxStackCount)
                Target?.RemoveEffect(effect);

            IsEnded = true;
        }

        public virtual void ApplyRestart()
        {
            if (effect.IsStackable)
                RestartState();
            else
            if (IsEnded)
                RestartState();
        }

        public virtual void RestartState()
        {
            End();
            IsMaxStackCount = false;
            IsEnded = false;
            IsSet = false;
        }

        public virtual void SetTarget(IHealthComponent newTarget)
        {
            Target = Target ?? newTarget;
        }
    }
}
