using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Game.Data;
using Game.Spirit.Data.Stats;

#if UNITY_EDITOR	
using UnityEditor;
#endif
using UnityEngine;
using U = UnityEngine.Object;

namespace Game.Systems
{
    public static class DataControlSystem
    {
        public static void Save<Data>(Data data) where Data : ScriptableObject
        {
#if UNITY_EDITOR
            AssetDatabase.Refresh();
            EditorUtility.SetDirty(data);
            AssetDatabase.SaveAssets();
#endif
        }

        public static ScriptableObject Load<Data>() where Data : ScriptableObject
        {
            return
#if UNITY_EDITOR
                typeof(Data) == typeof(SpiritDataBase) ? LoadSpiritDB() :
                typeof(Data) == typeof(EnemyDataBase) ? LoadEnemyDB() :
                typeof(Data) == typeof(MageDataBase) ? LoadMageDB() :
#endif
                null as ScriptableObject;

            #region  Helper functions


            #endregion
        }

#if UNITY_EDITOR
        static SpiritDataBase LoadSpiritDB() => AssetDatabase.LoadAssetAtPath<SpiritDataBase>("Assets/DataBase/SpiritDB.asset");
        static EnemyDataBase LoadEnemyDB() => AssetDatabase.LoadAssetAtPath<EnemyDataBase>("Assets/DataBase/EnemyDB.asset");
        static MageDataBase LoadMageDB() => AssetDatabase.LoadAssetAtPath<MageDataBase>("Assets/DataBase/MageDB.asset");
#endif
    }
}
