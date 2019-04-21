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
            if (!IsEnded) return;
            if (IsMaxStackReached) return;

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
                effectPrefab = Object.Instantiate(
                    effect.EffectPrefab,
                    Target.Prefab.transform.position + Vector3.up * 20,
                    Quaternion.identity,
                    Target.Prefab.transform);

                psList = effectPrefab.GetComponentsInChildren<ParticleSystem>();
                Show(true);

                base.Apply();

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

                    if (Target is EnemySystem enemy)
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
