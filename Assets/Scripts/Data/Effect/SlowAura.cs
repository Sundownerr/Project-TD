using Game.Systems.Effects;
using UnityEngine;

namespace Game.Data.Effects
{
    [CreateAssetMenu(fileName = "SlowAura", menuName = "Data/Effect/Slow Aura")]

    public class SlowAura : Effect
    {
        [SerializeField] float size;
        [SerializeField] float slowPercent;
        [SerializeField] GameObject effectPrefab;

        public override EffectSystem EffectSystem => new SlowAuraSystem(this);

        public float SlowPercent { get => slowPercent; private set => slowPercent = value; }
        public float Size { get => size; private set => size = value; }
        public GameObject EffectPrefab { get => effectPrefab; private set => effectPrefab = value; }
    }
}
