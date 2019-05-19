
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Game.Systems
{
    public static class DataControlSystem
    {
#if UNITY_EDITOR
        public static void Save<T>(T data) where T : ScriptableObject
        {
            AssetDatabase.Refresh();
            EditorUtility.SetDirty(data);
            AssetDatabase.SaveAssets();

        }
#endif

#if UNITY_EDITOR
        public static ScriptableObject LoadDatabase<T>() where T : ScriptableObject
        {
            var path = $"Assets/DataBase/{typeof(T).Name}.asset";
            var database = AssetDatabase.LoadAssetAtPath<T>(path);

            if (database != null)
                return database;
            else
            {
                Debug.LogError($"Wrong database type or can't find database at path {path}");
                return null;
            }
        }
#endif
    }
}
