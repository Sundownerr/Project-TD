using System.Collections;
using UnityEngine;
using Game.Systems;

namespace Game.Data.Effects
{

    public class StunSystem : EffectSystem
    {
        GameObject effectPrefab;
        Coroutine effectCoroutine;

        new Stun effect;
        WaitForSeconds stunDuration;

        public StunSystem(Stun effect) : base(effect)
        {
            this.effect = effect;
            stunDuration = new WaitForSeconds(effect.Duration);
        }

        public override void Apply()
        {
            base.Apply();

            if (target.Prefab == null)
            {
                target = (Owner as AbilitySystem).Target as ICanReceiveEffects;
                End();
                return;
            }

            if (isMaxStackCount || target == null)
                End();
            else
            {
                effectPrefab = Object.Instantiate(
                    effect.EffectPrefab,
                    target.Prefab.transform.position,
                    Quaternion.identity,
                    target.Prefab.transform);

                target.AddEffect(effect);
                effectCoroutine = GameLoop.Instance.StartCoroutine(Stun());
            }

            IEnumerator Stun()
            {
                target.IsOn = false;
                yield return stunDuration;
                End();
            }
        }

        public override void End()
        {
            if (effectCoroutine != null)
                GameLoop.Instance.StopCoroutine(effectCoroutine);

            if (target != null)
                target.IsOn = true;

            Object.Destroy(effectPrefab);

            base.End();
        }
    }
}
