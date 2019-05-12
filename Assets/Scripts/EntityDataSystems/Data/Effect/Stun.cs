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

        public override EffectSystem EffectSystem => new StunSystem(this); 
        public GameObject EffectPrefab { get => effectPrefab; set => effectPrefab = value; }
    }
}