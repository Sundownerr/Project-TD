using System.Collections.Generic;
using UnityEngine;
using System;
using Game.Data.Enemy;

namespace Game.Data
{
    [CreateAssetMenu(fileName = "Wave", menuName = "Data/Wave")]
    [Serializable]
    public class Wave : ScriptableObject
    {       
        [SerializeField]
        public List<EnemyData> EnemyTypes;
    }
}