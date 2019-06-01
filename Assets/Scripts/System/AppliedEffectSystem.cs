using System.Collections.Generic;
using Game.Data.Effects;

namespace Game.Systems
{
    public class AppliedEffectSystem
    {
        public List<Data.Effect> AppliedEffects { get; private set; } = new List<Data.Effect>();

        public void AddEffect(Data.Effect effect)
        {
            AppliedEffects.Add(effect);
        }

        public void RemoveEffect(Data.Effect effect)
        {
            for (int i = 0; i < AppliedEffects.Count; i++)
            {
                if (effect.Index == AppliedEffects[i].Index)
                {
                    AppliedEffects.RemoveAt(i);
                    return;
                }
            }
        }

        public int CountOf(Data.Effect effect)
        {
            var count = 0;
            var appliedEffects = AppliedEffects;

            for (int i = 0; i < appliedEffects.Count; i++)
            {
                if (effect.Index == AppliedEffects[i].Index)
                {
                    count++;
                }
            }
            return count;
        }
    }
}