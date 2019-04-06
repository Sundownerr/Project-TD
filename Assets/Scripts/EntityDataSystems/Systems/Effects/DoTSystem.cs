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

        private float tickTimer;
        private new DoT effect;
        private GameObject effectPrefab;
        private ParticleSystem[] psList;

        public DoTSystem(DoT effect) : base(effect)
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
                    target.Prefab.transform.position + Vector3.up * 20,
                    Quaternion.identity,
                    target.Prefab.transform);

                psList = effectPrefab.GetComponentsInChildren<ParticleSystem>();
                Show(true);

                target.AddEffect(effect);
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

            #endregion
        }

        public override void Continue()
        {
            base.Continue();

            tickTimer += Time.deltaTime * 0.5f;
            if (tickTimer >= 1)
                if (target is EnemySystem enemy)
                {
                    tickTimer = 0;
                    this.DealDamage(enemy, effect.DamagePerTick);
                }
                else
                    End();
        }

        public override void End()
        {
            Object.Destroy(effectPrefab);
            tickTimer = 0;
            base.End();
        }
    }
}
