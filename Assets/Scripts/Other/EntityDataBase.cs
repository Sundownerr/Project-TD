using System.Collections;
using System.Collections.Generic;
using Game;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;

namespace Game.Data
{
    public class EntityDataBase<T> : ScriptableObject where T : Entity
    {
        public List<T> Data;


#if UNITY_EDITOR

        protected void Awake()
        {
            if (Data == null)
                CreateDefaultData();
            else
                UpdateElementId();
        }


        protected virtual void CreateDefaultData()
        {
            Data = new List<T>();
        }

        protected void OnValidate() => UpdateElementId();

        protected void UpdateElementId()
        {
            for (int i = 0; i < Data.Count; i++)
                if (Data[i].ID.Count == 0)
                    Data[i].ID = new ID() { i };
        }

        [Button]
        public void DisplayData()
        {
            Debug.Log(AssetDatabase.GetAssetPath(this));
            Debug.Log(name);
            Data.ForEach(element => Debug.Log($"{element.Name}, ID: {element.ID.ToString()}"));
        }

#endif

    }
}