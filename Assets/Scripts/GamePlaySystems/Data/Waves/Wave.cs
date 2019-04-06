using System.Collections.Generic;
using Game.Enemy.Data;
using UnityEngine;
using System;
using Game.Enemy;

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