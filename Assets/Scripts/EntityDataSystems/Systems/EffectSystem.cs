using System.Collections;
using System.Collections.Generic;
using Game.Data;
using UnityEngine;
using System;

namespace Game.Systems
{
    public class EffectSystem : IEntitySystem
    {
        public bool IsEnded { get; protected set; } = true;
        public bool IsMaxStackReached => Target.CountOf(effect) > effect.MaxStackCount;
        public ICanReceiveEffects Target { get; protected set; }
        public IEntitySystem Owner { get; set; }
        public ID ID { get; set; }

        protected Effect effect;

        public EffectSystem(Effect effect)
        {
            this.effect = effect;
        }

        public void SetSystem(AbilitySystem ownerAbility)
        {
            Owner = ownerAbility;
            ID = new ID(ownerAbility.ID);
            ID.Add(ownerAbility.EffectSystems.IndexOf(this));
        }

        public virtual void Apply()
        {
            Target.AddEffect(effect);
            IsEnded = false;
        }

        public virtual void End()
        {
            Target?.RemoveEffect(effect);
            IsEnded = true;
        }

        public virtual void ApplyRestart()
        {
            if (effect.MaxStackCount > 1) End();
            else
            if (IsEnded) End();
        }
    
        public virtual void SetTarget(ICanReceiveEffects newTarget)
        {
            if (Target == null || Target.Prefab == null)
                Target = newTarget;
        }
    }
}
