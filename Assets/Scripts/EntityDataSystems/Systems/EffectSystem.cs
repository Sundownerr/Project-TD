using Game.Data.Effects;
using Game.Systems.Abilities;

namespace Game.Systems.Effects
{
    public class EffectSystem : IEntitySystem
    {
        public bool IsEnded { get; protected set; } = true;
        public bool IsMaxStackReached => Target.CountOf(Effect) > Effect.MaxStackCount;
        public ICanReceiveEffects Target { get; protected set; }
        public IEntitySystem Owner { get; protected set; }
        public ID ID { get; protected set; }
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
            Target.AddEffect(Effect);
            IsEnded = false;
        }

        public virtual void End()
        {
            if (Target != null)
            {
                Target.RemoveEffect(Effect);
            }

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
            {
                Target = newTarget;
            }
        }
    }
}
