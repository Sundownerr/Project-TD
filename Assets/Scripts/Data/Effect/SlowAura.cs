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

        public override Systems.Effect EffectSystem => new Systems.Effects.SlowAura(this);

        public float SlowPercent { get => slowPercent; private set => slowPercent = value; }
        public float Size { get => size; private set => size = value; }
        public GameObject EffectPrefab { get => effectPrefab; private set => effectPrefab = value; }
    }
}
