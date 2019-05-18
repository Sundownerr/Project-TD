using Game.Data;
using UnityEngine;
using Game.Enums;
using Game.Utility;
using Game.Data.Effects;
using Game.Utility.Creator;

namespace Game.Systems.Effects
{
    public class AuraSystem : EffectSystem
    {
        protected RangeSystem range;
        protected GameObject rangePrefab;

        public AuraSystem(Effect effect) : base(effect)
        {
            Effect = effect;
        }

        public override void Apply()
        {
            var owner = Owner.GetOwnerOfType<IEntitySystem>();
            range = Create.Range(owner as IPrefabComponent, 1, CollideWith.EnemiesAndSpirits);

            IsEnded = false;
        }

        public override void End()
        {
            UnityEngine.Object.Destroy(rangePrefab);
        }
    }
}