using System.Collections.Generic;
using UnityEngine;
using System;
using Game.Enemy.Data;
using UnityEditor;

namespace Game.Data
{
    [CreateAssetMenu(fileName = "EnemyDB", menuName = "Data/Data Base/Enemy DataBase")]
    [Serializable]
    public class EnemyDataBase : ScriptableObject
    {
        [SerializeField]
        public Race[] Races;

#if UNITY_EDITOR

        private void Awake()
        {
            if (Races == null)
            {
                var races = Enum.GetValues(typeof(RaceType));

                Races = new Race[races.Length];

                for (int i = 0; i < races.Length; i++)
                    Races[i] = new Race();
            }

            EditorUtility.SetDirty(this);
        }

#endif
    }
}

