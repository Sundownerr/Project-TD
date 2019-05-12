using System.Collections.Generic;
using UnityEngine;
using Game.Enums;
using Game.Systems.Spirit;
using Game.Data.Effects;

namespace Game.Systems.Effects
{
    public class SlowAuraSystem : AuraSystem
    {
        SlowAura effect;
        Dictionary<SpiritSystem, int> removedAttackSpeedMods;

        public SlowAuraSystem(SlowAura effect) : base(effect)
        {
            Effect = effect;
            this.effect = effect;
            removedAttackSpeedMods = new Dictionary<SpiritSystem, int>();
        }

        void OnSpiritEnteredRange(object _, IVulnerable e)
        {
            var spirit = e as SpiritSystem;

            var spiritAttackSpeed = spirit.Data.Get(Enums.Spirit.AttackSpeed);
            var removedAttackSpeedMod = (int)spiritAttackSpeed.Sum.GetPercent(effect.SlowPercent);

            if (spirit.CountOf(Effect) <= 0)
                spiritAttackSpeed.AppliedValue -= removedAttackSpeedMod;
            else
                removedAttackSpeedMod = (int)(spiritAttackSpeed.Sum - effect.SlowPercent).GetPercent(effect.SlowPercent);

            spirit.AddEffect(Effect);
            removedAttackSpeedMods.Add(spirit, removedAttackSpeedMod);
        }

        void OnSpiritExitRange(object _, IVulnerable e) => RemoveEffect(e as ICanReceiveEffects);

        void RemoveEffect(ICanReceiveEffects entity)
        {
            var spirit = entity as SpiritSystem;
            var spiritAttackSpeed = spirit.Data.Get(Enums.Spirit.AttackSpeed);

            if (spirit.CountOf(Effect) <= 1)
                if (removedAttackSpeedMods.TryGetValue(spirit, out int removedAttackSpeed))
                    spiritAttackSpeed.AppliedValue += removedAttackSpeed;

            removedAttackSpeedMods.Remove(spirit);
            spirit.RemoveEffect(Effect);
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