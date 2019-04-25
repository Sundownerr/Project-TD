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
        public bool IsMaxStackReached => Target.CountOf(this) > Effect.MaxStackCount;
        public ICanReceiveEffects Target { get; protected set; }
        public IEntitySystem Owner { get; set; }
        public ID ID { get; set; }
        public Effect Effect { get; protected set; }

        public EffectSystem(Effect effect)
        {
            Effect = effect;
        }

        public void SetSystem(AbilitySystem ownerAbility)
        {
            Owner = ownerAbility;
            ID = new ID(ownerAbility.ID);
            ID.Add(ownerAbility.EffectSystems.IndexOf(this));

        }

        public virtual void Apply()
        {
            Target.AddEffect(this);
            IsEnded = false;
        }

        public virtual void End()
        {
            Target.RemoveEffect(this);
            IsEnded = true;
        }

        public virtual void SetTarget(ICanReceiveEffects newTarget, bool forceSet)
        {
            if (forceSet)
            {
                Target = newTarget;
                return;
            }

            if (Target == null || Target.Prefab == null)
                Target = newTarget;
        }
    }
}
