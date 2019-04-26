using System.Collections.Generic;
using UnityEngine;
using System;
using Game.Enemy.Data;
using UnityEditor;
using Game.Enums;

namespace Game.Data
{
    [CreateAssetMenu(fileName = "EnemyDB", menuName = "Data/Data Base/Enemy DataBase")]
    [Serializable]
    public class EnemyDataBase : ScriptableObject
    {
        [SerializeField]
        public Race[] Data;

#if UNITY_EDITOR

        void Awake()
        {
            if (Data == null)
            {
                var races = Enum.GetValues(typeof(RaceType));

                Data = new Race[races.Length];

                for (int i = 0; i < races.Length; i++)
                    Data[i] = new Race();
            }         
        }

#endif
    }
}

