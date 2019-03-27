using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class ID : List<int>
{
    public bool Compare(ID other)
    {      
        if (this == null || other == null)
            return false;

        if (this.Count != other.Count)
            return false;

        for (int i = 0; i < this.Count; i++)
            if (this[i] != other[i])
                return false;

        return true;
    }

    public ID() {}
    public ID(ID other) => this.AddRange(other);
    
 }

[Serializable]
public class ListID : List<ID> { }

[Serializable]
public class WaveEnemyID
{
    [SerializeField] public ListID IDs;
    [SerializeField] public ListID AbilityIDs;
    [SerializeField] public ListID TraitIDs;
}
