using System.Collections.Generic;
using UnityEngine;
using System;
using Game.Data.EnemyEntity;

namespace Game.Data
{
    [CreateAssetMenu(fileName = "Wave", menuName = "Data/Wave")]
    [Serializable]
    public class Wave : ScriptableObject
    {       
        [SerializeField]
        public List<EnemyEntity.Enemy> EnemyTypes;
    }
}