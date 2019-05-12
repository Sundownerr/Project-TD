using System.Collections;
using System.Collections.Generic;
using Game.Systems;
using Game.Systems.Effects;
using UnityEngine;

namespace Game.Data.Effects
{
    [CreateAssetMenu(fileName = "DoT", menuName = "Data/Effect/DoT")]

    public class DoT : Effect
    {
        [SerializeField] int damagePerTick;
        [SerializeField] GameObject effectPrefab;

        public override EffectSystem EffectSystem => new DoTSystem(this);

        public int DamagePerTick { get => damagePerTick; private set => damagePerTick = value; }
        public GameObject EffectPrefab { get => effectPrefab; private set => effectPrefab = value; }
    }
}