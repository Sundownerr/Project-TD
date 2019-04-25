using System.Collections;
using UnityEngine;
using Game.Systems;

namespace Game.Data.Effects
{

    public class StunSystem : EffectSystem
    {
        GameObject effectPrefab;
        Coroutine effectCoroutine;

        Stun effect;
        WaitForSeconds stunDuration;

        public StunSystem(Stun effect) : base(effect)
        {
            Effect = effect;
            this.effect = effect;
            stunDuration = new WaitForSeconds(effect.Duration);
        }

        public override void Apply()
        {
            if (!IsEnded) return;
            if (IsMaxStackReached) { End(); return; }

            if (Target.Prefab == null)
            {
                Target = (Owner as AbilitySystem).Target as ICanReceiveEffects;
                End();
                return;
            }

            if (Target == null)
                End();
            else
            {
                base.Apply();

                effectPrefab = Object.Instantiate(
                    effect.EffectPrefab,
                    Target.Prefab.transform.position,
                    Quaternion.identity,
                    Target.Prefab.transform);

                effectCoroutine = GameLoop.Instance.StartCoroutine(Stun());
            }

            IEnumerator Stun()
            {
                Target.IsOn = false;
                yield return stunDuration;
                End();
            }
        }

        public override void End()
        {
            base.End();

            if (effectCoroutine != null)
                GameLoop.Instance.StopCoroutine(effectCoroutine);

            if (Target != null)
                if (Target.CountOf(this) == 0)
                    Target.IsOn = true;

            Object.Destroy(effectPrefab);
        }
    }
}
