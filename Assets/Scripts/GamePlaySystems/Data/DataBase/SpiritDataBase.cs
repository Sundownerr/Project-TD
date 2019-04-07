using System.Collections.Generic;
using UnityEngine;
using System;
using Game.Spirit.Data.Stats;
using UnityEditor;
using Game.Enums;

namespace Game.Data
{
    [CreateAssetMenu(fileName = "SpiritDB", menuName = "Data/Data Base/Spirit DataBase")]
    [Serializable]
    public class SpiritDataBase : ScriptableObject
    {
        [SerializeField]
        public ElementList Spirits;

#if UNITY_EDITOR

        void Awake()
        {
            if (Spirits == null)
            {
                var elementNames = Enum.GetNames(typeof(ElementType));

                Spirits = new ElementList { Elements = new Element[elementNames.Length] };
                
                for (int i = 0; i < elementNames.Length; i++)
                    Spirits.Elements[i] = new Element(elementNames[i]);
            }

            EditorUtility.SetDirty(this);
        }

#endif

    }
}