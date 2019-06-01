using System.Collections;
using System.Collections.Generic;
using Game.Systems;
using Game.Systems.Effects;
using UnityEngine;

namespace Game.Data.Effects
{
    [CreateAssetMenu(fileName = "Stun", menuName = "Data/Effect/Stun")]

    public class Stun : Effect
    {
        [SerializeField] GameObject effectPrefab;

        public override Systems.Effect EffectSystem => new Systems.Effects.Stun((Stun)this); 
        public GameObject EffectPrefab { get => effectPrefab; set => effectPrefab = value; }
    }
}