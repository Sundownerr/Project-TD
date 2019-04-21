using Game.Spirit;
using System.Collections;
using System.Collections.Generic;
using Game.Enemy;
using Game.Data;
using Game.Data.Effects;
using UnityEngine;
using Game.Enums;

namespace Game.Systems
{
    public class SlowAuraSystem : AuraSystem
    {
        new SlowAura effect;
        Dictionary<SpiritSystem, int> removedAttackSpeedMods;

        public SlowAuraSystem(SlowAura effect) : base(effect)
        {
            this.effect = effect;

            removedAttackSpeedMods = new Dictionary<SpiritSystem, int>();
        }

        void OnSpiritEnteredRange(object _, IVulnerable e)
        {
            var spirit = e as SpiritSystem;
            var spiritAttackSpeed = spirit.Data.Get(Enums.Spirit.AttackSpeed);
            var removedAttackSpeedMod = (int)spiritAttackSpeed.Sum.GetPercent(effect.SlowPercent);

            if (spirit.CountOf(effect) <= 0)
                spiritAttackSpeed.AppliedValue -= removedAttackSpeedMod;
            else
                removedAttackSpeedMod = (int)(spiritAttackSpeed.Sum - effect.SlowPercent).GetPercent(effect.SlowPercent);

            spirit.AddEffect(effect);
            removedAttackSpeedMods.Add(spirit, removedAttackSpeedMod);
        }

        void OnSpiritExitRange(object _, IVulnerable e) => RemoveEffect(e as ICanReceiveEffects);

        void RemoveEffect(ICanReceiveEffects entity)
        {
            var spirit = entity as SpiritSystem;
            var spiritAttackSpeed = spirit.Data.Get(Enums.Spirit.AttackSpeed);

            if (spirit.CountOf(effect) <= 1)
                spiritAttackSpeed.AppliedValue += removedAttackSpeedMods[spirit];

            removedAttackSpeedMods.Remove(spirit);
            spirit.RemoveEffect(effect);
        }

        public override void Apply()
        {
            if (!IsEnded) return;

            base.Apply();

            range.EntityEntered += OnSpiritEnteredRange;
            range.EntityExit += OnSpiritExitRange;
            range.Destroyed += OnRangeDestroyed;

            range.CollideType = CollideWith.Spirits;
            range.transform.localScale = new Vector3(effect.Size, 0.001f, effect.Size);
            range.transform.position += new Vector3(0, 15, 0);
            range.SetShow(true);
        }

        void OnRangeDestroyed(object _, System.EventArgs e) => End();

        public override void End()
        {
            range.EntitySystems.ForEach(entitySystem => RemoveEffect(entitySystem as ICanReceiveEffects));
            removedAttackSpeedMods.Clear();

            range.EntityEntered -= OnSpiritEnteredRange;
            range.EntityExit -= OnSpiritExitRange;
            range.Destroyed -= OnRangeDestroyed;
            base.End();
        }
    }
}