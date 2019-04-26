using System.Collections;
using System.Collections.Generic;
using Game;
using UnityEngine;

public class EntityDataBase<T> : ScriptableObject where T : Entity
{
    public List<T> Data;

#if UNITY_EDITOR

    void Awake()
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

    void OnValidate() => UpdateElementId();

    void UpdateElementId()
    {
        for (int i = 0; i < Data.Count; i++)
            if (Data[i].ID.Count == 0)
                Data[i].ID = new ID() { i };
    }

#endif

}
