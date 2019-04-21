using System.Collections;
using System.Collections.Generic;
using Game.Enemy;
using Game.Data;
using Game.Spirit;
using UnityEngine;
using Game.Enums;
using System;

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
            var owner = Owner.GetOwnerOfType<IEntitySystem>();
            range = StaticMethods.CreateRange(owner as IPrefabComponent, 1, CollideWith.EnemiesAndSpirits);

            IsEnded = false;
        }

        public override void End()
        {
            UnityEngine.Object.Destroy(rangePrefab);
        }
    }
}