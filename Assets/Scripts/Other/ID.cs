using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
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

        public override string ToString()
        {
            var stringID = string.Empty;
            this.ForEach(number => stringID += number.ToString());

            return stringID;
        }

        public ID() { }
        public ID(ID other) => this.AddRange(other);
        public ID(int[] other)
        {
            for (int i = 0; i < other.Length; i++)
            {
                this.Add(other[i]);
            }
        }

    }

    [Serializable]
    public class ListID : List<ID> { }

    [Serializable]
    public class WaveEnemyID
    {
        [SerializeField] public ListID IDs;
        [SerializeField] public ListID AbilityIDs;
        [SerializeField] public ListID TraitIDs;
        [SerializeField] public int ArmorID;
    }
}