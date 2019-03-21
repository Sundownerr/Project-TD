using System.Collections;
using System.Collections.Generic;
using Game.Enemy;
using Game.Data;
using Game.Spirit;
using UnityEngine;

namespace Game.Systems
{
    public class AuraSystem : EffectSystem
    {
        protected RangeSystem range;
        protected GameObject rangePrefab;

        public AuraSystem(Effect effect) : base(effect)
        {
            this.effect = effect;
        }

        public override void Apply()
        {
            isSet = true;
            isEnded = false;
            var owner = OwnerSystem.GetOwnerOfType<IEntitySystem>();

            range = StaticMethods.CreateRange(owner as IPrefabComponent, 1, CollideWith.EnemiesAndSpirits);
        }

        public override void Continue()
        {
            if (OwnerSystem == null)
                End();
        }

        public override void End()
        {
            Object.Destroy(rangePrefab);
        }
    }
}