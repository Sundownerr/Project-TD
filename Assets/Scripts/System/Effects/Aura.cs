using Game.Data;
using UnityEngine;
using Game.Enums;
using Game.Utility;
using Game.Data.Effects;
using Game.Utility.Creator;

namespace Game.Systems.Effects
{
    public abstract class Aura : Effect
    {
        protected RangeSystem range;
        protected GameObject rangePrefab;

        public Aura(Data.Effect effect) : base(effect)
        {
            EffectData = effect;
        }

        public override void Apply()
        {
            IsEnded = false;
        }

        protected abstract void OnEntityEnteredRange(IVulnerable entity);
        protected abstract void OnEntityExitRange(IVulnerable entity);
        
        public override void End()
        {
            UnityEngine.Object.Destroy(rangePrefab);
        }
    }
}