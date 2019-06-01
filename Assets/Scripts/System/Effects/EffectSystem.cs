using Game.Data.Effects;
using Game.Systems.Abilities;

namespace Game.Systems
{
    public class Effect : IEntitySystem
    {
        public bool IsEnded { get; protected set; } = true;
        public bool IsMaxStackReached => Target.CountOf(Data) > Data.MaxStackCount;
        public IAppliedEffectsComponent Target { get; protected set; }
        public IEntitySystem Owner { get; protected set; }
        public Data.Effect Data { get; protected set; }

        public Effect(Data.Effect effect)
        {
            Data = effect;
        }

        public void SetSystem(AbilitySystem ownerAbility)
        {
            Owner = ownerAbility;
        }

        public virtual void Apply()
        {
            Target.AddEffect(Data);
            IsEnded = false;
        }

        public virtual void End()
        {
            if (Target != null)
            {
                Target.RemoveEffect(Data);
            }

            IsEnded = true;
        }

        public virtual void SetTarget(IAppliedEffectsComponent newTarget, bool forceSet)
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
