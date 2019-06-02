using System.Collections.Generic;
using UnityEngine;
using Game.Enums;
using Game.Systems.Spirit;
using Game.Data.Effects;
using Game.Utility.Creator;

namespace Game.Systems.Effects
{
    public class SlowAura : Aura
    {
        Data.Effects.SlowAura effect;
        Dictionary<SpiritSystem, int> removedAttackSpeedMods;

        public SlowAura(Data.Effects.SlowAura effect) : base(effect)
        {
            EffectData = effect;
            this.effect = effect;
            removedAttackSpeedMods = new Dictionary<SpiritSystem, int>();
        }

        protected override void OnEntityEnteredRange(IVulnerable e)
        {
            var spirit = e as SpiritSystem;

            var spiritAttackSpeed = spirit.Data.Get(Enums.Spirit.AttackSpeed);
            var removedAttackSpeedMod = (int)spiritAttackSpeed.Sum.GetPercent(effect.SlowPercent);

            if (spirit.CountOf(EffectData) <= 0)
            {
                spiritAttackSpeed.AppliedValue -= removedAttackSpeedMod;
            }
            else
            {
                removedAttackSpeedMod = (int)(spiritAttackSpeed.Sum - effect.SlowPercent).GetPercent(effect.SlowPercent);
            }

            spirit.AddEffect(EffectData);
            removedAttackSpeedMods.Add(spirit, removedAttackSpeedMod);
        }

        protected override void OnEntityExitRange(IVulnerable e) => RemoveEffect(e as IAppliedEffectsComponent);

        void RemoveEffect(IAppliedEffectsComponent entity)
        {
            var spirit = entity as SpiritSystem;
            var spiritAttackSpeed = spirit.Data.Get(Enums.Spirit.AttackSpeed);

            if (spirit.CountOf(EffectData) <= 1)
            {
                if (removedAttackSpeedMods.TryGetValue(spirit, out int removedAttackSpeed))
                {
                    spiritAttackSpeed.AppliedValue += removedAttackSpeed;
                }
            }

            removedAttackSpeedMods.Remove(spirit);
            spirit.RemoveEffect(EffectData);
        }

        public override void Apply()
        {
            if (!IsEnded) return;

            base.Apply();

            var owner = Owner.GetOwnerOfType<IEntitySystem>();
            range = Create.Range(owner as IPrefabComponent, 1, CollideWith.EnemiesAndSpirits, OnEntityEnteredRange, OnEntityExitRange);

            range.Destroyed += OnRangeDestroyed;

            range.CollideType = CollideWith.Spirits;
            range.transform.localScale = new Vector3(effect.Size, 0.001f, effect.Size);
            range.transform.position += new Vector3(0, 15, 0);
            range.Show = true;
        }

        void OnRangeDestroyed() => End();

        public override void End()
        {
            range.EntitySystems.ForEach(entitySystem => RemoveEffect(entitySystem as IAppliedEffectsComponent));
            removedAttackSpeedMods.Clear();

            range.Destroyed -= OnRangeDestroyed;
            base.End();
        }
    }
}