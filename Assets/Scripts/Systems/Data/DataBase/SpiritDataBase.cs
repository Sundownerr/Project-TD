using System.Collections.Generic;
using UnityEngine;
using System;
using Game.Spirit.Data.Stats;
using UnityEditor;

namespace Game.Data
{
    [CreateAssetMenu(fileName = "SpiritDB", menuName = "Data/Data Base/Spirit DataBase")]
    [Serializable]
    public class SpiritDataBase : ScriptableObject
    {
        [SerializeField]
        public ElementList Spirits;

#if UNITY_EDITOR

        private void Awake()
        {
            if (Spirits == null)
            {
                Spirits = new ElementList { Elements = new List<Element>() };

                var elementNames = Enum.GetNames(typeof(ElementType));

                for (int i = 0; i < elementNames.Length; i++)
                    Spirits.Elements.Add(new Element(elementNames[i]));
            }

            EditorUtility.SetDirty(this);
        }

#endif

    }
}