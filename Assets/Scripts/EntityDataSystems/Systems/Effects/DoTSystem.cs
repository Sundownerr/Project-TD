using System.Collections;
using UnityEngine;
using Game.Systems;
using Game.Enemy;
using Game.Spirit;
using Game.Data.Effects;
using Game.Data;

namespace Game.Systems.Effects
{
    public class DoTSystem : EffectSystem, IDamageDealerChild
    {
        public IDamageDealer OwnerDamageDealer { get; set; }

        new DoT effect;
        GameObject effectPrefab;
        ParticleSystem[] psList;
        WaitForSeconds damageInterval = new WaitForSeconds(0.5f);
        Coroutine effectCoroutine;


        public DoTSystem(DoT effect) : base(effect)
        {
            this.effect = effect;
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
                    target.Prefab.transform.position + Vector3.up * 20,
                    Quaternion.identity,
                    target.Prefab.transform);

                psList = effectPrefab.GetComponentsInChildren<ParticleSystem>();
                Show(true);

                target.AddEffect(effect);
                effectCoroutine = GameLoop.Instance.StartCoroutine(DealDamageOverTime());
            }

            #region Helper functions

            void Show(bool enabled)
            {
                for (int i = 0; i < psList.Length; i++)
                {
                    var emissionModule = psList[i].emission;
                    emissionModule.enabled = enabled;

                    if (enabled)
                        psList[i].Play();
                    else
                        psList[i].Stop();
                }
            }

            IEnumerator DealDamageOverTime()
            {
                var tickTimer = 0f;

                while (tickTimer < effect.Duration)
                {
                    tickTimer += 0.5f;

                    if (target is EnemySystem enemy)
                        this.DealDamage(enemy, effect.DamagePerTick);

                    yield return damageInterval;
                }

                End();
            }

            #endregion
        }

        public override void End()
        {
            if (effectCoroutine != null)
                GameLoop.Instance.StopCoroutine(effectCoroutine);

            Object.Destroy(effectPrefab);
            base.End();
        }
    }
}
