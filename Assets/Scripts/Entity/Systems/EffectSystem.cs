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
        public IHealthComponent Target { get => target; set => target = value; }       
        public IEntitySystem OwnerSystem { get; set; }
        public ID ID { get; set; }

        protected bool isSet, isEnded, isMaxStackCount;
        protected float effectTimer;
        protected IHealthComponent target;
        protected Effect effect;

        public EffectSystem(Effect effect)
        {
            this.effect = effect;

            if (!effect.IsStackable)
                effect.MaxStackCount = 1;
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
            else if (IsEnded)
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
