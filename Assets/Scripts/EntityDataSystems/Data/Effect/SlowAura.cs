using Game.Systems.Effects;
using UnityEngine;

namespace Game.Data.Effects
{
    [CreateAssetMenu(fileName = "SlowAura", menuName = "Data/Effect/Slow Aura")]

    public class SlowAura : Effect
    {
        public float Size, SlowPercent;
        public GameObject EffectPrefab;

        public override EffectSystem EffectSystem { get => new SlowAuraSystem(this); }
    }
}
