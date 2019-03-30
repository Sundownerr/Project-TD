using Game.Spirit;
using System.Collections;
using System.Collections.Generic;
using Game.Enemy;
using Game.Data;
using Game.Data.Effects;
using UnityEngine;

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

        private void OnSpiritEnteredRange(object _, IHealthComponent e)
        {
            var spirit = e as SpiritSystem;

            var removedAttackSpeedMod =
                    (int)spirit.Data.GetValue(Numeral.AttackSpeedModifier).GetPercent(effect.SlowPercent);

            if (spirit.CountOf(effect) <= 0)
                spirit.Data.Get(Numeral.AttackSpeedModifier, From.Base).Value -= removedAttackSpeedMod;
            else
                removedAttackSpeedMod =
                    (int)(spirit.Data.GetValue(Numeral.AttackSpeedModifier) + effect.SlowPercent).GetPercent(effect.SlowPercent);

            removedAttackSpeedMods.Add(spirit, removedAttackSpeedMod);
            spirit.AddEffect(effect);
        }

        private void OnSpiritExitRange(object _, IHealthComponent e) => RemoveEffect(e);

        private void RemoveEffect(IHealthComponent entity)
        {
            var spirit = entity as SpiritSystem;

            if (spirit.CountOf(effect) <= 1)
                if (removedAttackSpeedMods.TryGetValue(spirit, out int attackSpeedMod))
                    spirit.Data.Get(Numeral.AttackSpeedModifier, From.Base).Value += attackSpeedMod;

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
                RemoveEffect(range.EntitySystems[i]);

            removedAttackSpeedMods.Clear();

            range.EntityEntered -= OnSpiritEnteredRange;
            range.EntityExit -= OnSpiritExitRange;
            range.Destroyed -= OnRangeDestroyed;
            base.End();
        }
    }
}