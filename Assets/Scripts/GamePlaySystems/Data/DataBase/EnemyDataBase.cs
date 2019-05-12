using UnityEngine;
using System;
using Game.Data.Enemy.Internal;
using Game.Enums;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Game.Data.Databases
{
    [CreateAssetMenu(fileName = "EnemyDataBase", menuName = "Data/Data Base/Enemy DataBase")]
    [Serializable]
    public class EnemyDataBase : ScriptableObject
    {
        [SerializeField]
        public Race[] Data;

        public static string Path { get; protected set; }

#if UNITY_EDITOR

        void Awake()
        {
            Path = AssetDatabase.GetAssetPath(this);

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

