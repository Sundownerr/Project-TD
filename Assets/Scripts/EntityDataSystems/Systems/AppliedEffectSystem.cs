using System.Collections.Generic;
using Game.Data.Effects;

namespace Game.Systems
{
    public class AppliedEffectSystem
    {
        public List<Effect> AppliedEffects { get; private set; } = new List<Effect>();

        public void AddEffect(Effect effect)
        {
            AppliedEffects.Add(effect);
        }

        public void RemoveEffect(Effect effect)
        {
            for (int i = 0; i < AppliedEffects.Count; i++)
                if (effect.Index == AppliedEffects[i].Index)
                {
                    AppliedEffects.RemoveAt(i);
                    return;
                }
        }

        public int CountOf(Effect effect)
        {
            var count = 0;
            var appliedEffects = AppliedEffects;

            for (int i = 0; i < appliedEffects.Count; i++)
                if (effect.Index == AppliedEffects[i].Index)
                    count++;
            return count;
        }
    }
}