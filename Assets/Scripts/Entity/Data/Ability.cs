using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Systems;
using System;
using Game.Enemy;
using Game.Spirit;

namespace Game.Data
{
    [CreateAssetMenu(fileName = "Neww Ability", menuName = "Data/Ability")]


    [Serializable]
    public class Ability : Entity
    {
        public float Cooldown, TriggerChance;
        public int ManaCost;

        [Expandable]
        public List<Effect> Effects;
    }

    public class AbilityList : List<Ability> {}
}