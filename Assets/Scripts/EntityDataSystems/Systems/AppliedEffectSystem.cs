using System.Collections;
using System.Collections.Generic;
using Game.Data;
using Game.Systems;
using UnityEngine;

public class AppliedEffectSystem 
{
    public List<EffectSystem> AppliedEffects { get; private set; } = new List<EffectSystem>();

    public void AddEffect(EffectSystem effect)
    {
        AppliedEffects.Add(effect);
    }

    public void RemoveEffect(EffectSystem effect)
    {
        for (int i = 0; i < AppliedEffects.Count; i++)
            if (effect.ID.Compare(AppliedEffects[i].ID))
            {
                AppliedEffects.RemoveAt(i);
                return;
            }
    }

    public int CountOf(EffectSystem effect)
    {
        var count = 0;
        var appliedEffects = AppliedEffects;

        for (int i = 0; i < appliedEffects.Count; i++)
            if (effect.ID.Compare(appliedEffects[i].ID))
                count++;
        return count;
    }
}
