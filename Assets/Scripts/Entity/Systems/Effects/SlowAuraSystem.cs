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
        private new SlowAura effect;
        private Dictionary<SpiritSystem, int> removedAttackSpeedMods;

        public SlowAuraSystem(SlowAura effect) : base(effect)
        {
            this.effect = effect;

            removedAttackSpeedMods = new Dictionary<SpiritSystem, int>();
        }

        private void OnSpiritEnteredRange(object _, IVulnerable e)
        {
            var spirit = e as SpiritSystem;

            var removedAttackSpeedMod =
                    (int)spirit.Data.Get(Enums.Spirit.AttackSpeedModifier).Value.GetPercent(effect.SlowPercent);

            if (spirit.CountOf(effect) <= 0)          
                spirit.Data.Get(Enums.Spirit.AttackSpeedModifier).Value -= removedAttackSpeedMod;       
            else
                removedAttackSpeedMod =
                    (int)(spirit.Data.Get(Enums.Spirit.AttackSpeedModifier).Value + effect.SlowPercent).GetPercent(effect.SlowPercent);

            spirit.AddEffect(effect);
            removedAttackSpeedMods.Add(spirit, removedAttackSpeedMod);

        }

        private void OnSpiritExitRange(object _, IVulnerable e) => RemoveEffect(e as ICanApplyEffects);

        private void RemoveEffect(ICanApplyEffects entity)
        {
            var spirit = entity as SpiritSystem;

            if (spirit.CountOf(effect) <= 1)
                if (removedAttackSpeedMods.TryGetValue(spirit, out int attackSpeedMod))
                    spirit.Data.Get(Enums.Spirit.AttackSpeedModifier).Value += attackSpeedMod;

            removedAttackSpeedMods.Remove(spirit);
            spirit.RemoveEffect(effect);
        }

        public override void Apply()
        {
            base.Apply();

            range.EntityEntered += OnSpiritEnteredRange;
            range.EntityExit += OnSpiritExitRange;
            range.Destroyed += OnRangeDestroyed;

            range.CollideType = CollideWith.Spirits;
            range.transform.localScale = new Vector3(effect.Size, 0.001f, effect.Size);
            range.transform.position += new Vector3(0, 15, 0);
            range.SetShow(true);
        }

        private void OnRangeDestroyed(object _, System.EventArgs e) => End();

        public override void Continue()
        {
            base.Continue();
            if (Owner == null)
                End();
        }

        public override void End()
        {
            for (int i = 0; i < range.EntitySystems.Count; i++)
                RemoveEffect(range.EntitySystems[i] as ICanApplyEffects);

            removedAttackSpeedMods.Clear();

            range.EntityEntered -= OnSpiritEnteredRange;
            range.EntityExit -= OnSpiritExitRange;
            range.Destroyed -= OnRangeDestroyed;
            base.End();
        }
    }
}