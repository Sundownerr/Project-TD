using System.Collections;
using UnityEngine;
using Game.Systems;
using Game.Data.Effects;
using Game.Managers;
using Game.Systems.Enemy;
using Game.Systems.Abilities;

namespace Game.Systems.Effects
{
    public class DamageOverTime : Effect, IDamageDealerChild
    {
        public IDamageDealer OwnerDamageDealer { get; set; }

        Data.Effects.DamageOverTime effect;
        GameObject effectPrefab;
        ParticleSystem[] psList;
        WaitForSeconds damageInterval = new WaitForSeconds(1);
        Coroutine effectCoroutine;


        public DamageOverTime(Data.Effects.DamageOverTime effect) : base(effect)
        {
            EffectData = effect;
            this.effect = effect;
        }

        public override void Apply()
        {
            if (!IsEnded) return;

            if (Target == null || IsMaxStackReached)
            {
                End();
                return;
            }

            if (Target.Prefab == null)
            {
                Target = (Owner as AbilitySystem).Target as IAppliedEffectsComponent;
                End();
                return;
            }

            base.Apply();

            effectPrefab = Object.Instantiate(
                effect.EffectPrefab,
                Target.Prefab.transform.position + Vector3.up * 20,
                Quaternion.identity,
                Target.Prefab.transform);

            psList = effectPrefab.GetComponentsInChildren<ParticleSystem>();
            Show(true);

            effectCoroutine = GameLoop.Instance.StartCoroutine(DealDamageOverTime());


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
                    tickTimer += 1;

                    if (Target is EnemySystem enemy)
                    {
                        this.DealDamage(enemy, effect.DamagePerTick);
                    }

                    yield return damageInterval;
                }

                End();
            }

            #endregion
        }

        public override void End()
        {
            base.End();

            if (effectCoroutine != null)
            {
                GameLoop.Instance.StopCoroutine(effectCoroutine);
            }

            Object.Destroy(effectPrefab);
        }
    }
}
