using System.Collections;
using UnityEngine;
using Game.Systems;

namespace Game.Data.Effects
{

    public class StunSystem : EffectSystem
    {
        GameObject effectPrefab;
        new Stun effect;

        public StunSystem(Stun effect) : base(effect)
        {
            this.effect = effect;
        }

        public override void Apply()
        {
            base.Apply();

            if (isMaxStackCount || target == null || target.Prefab == null)
                End();
            else
            {
                effectPrefab = Object.Instantiate(
                    effect.EffectPrefab,
                    target.Prefab.transform.position,
                    Quaternion.identity,
                    target.Prefab.transform);

                target.AddEffect(effect);
            }
        }

        public override void Continue()
        {
            base.Continue();
            target.IsOn = false;
        }

        public override void End()
        {
            if (target != null)
                target.IsOn = true;

            Object.Destroy(effectPrefab);

            base.End();
        }
    }
}
