using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    [Serializable]
    public class NetworkWaveData
    {
        [SerializeField] public List<int> EnemyIndexes;
        [SerializeField] public List<int> AbilityIndexes;
        [SerializeField] public List<int> TraitIndexes;
        [SerializeField] public int ArmorIndex;
    }
}