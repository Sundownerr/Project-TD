using UnityEngine;
using System;
using Game.Data.Spirit.Internal;
using Game.Enums;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Game.Data.Databases
{
    [CreateAssetMenu(fileName = "SpiritDataBase", menuName = "Data/Data Base/Spirit DataBase")]
    [Serializable]
    public class SpiritDataBase : ScriptableObject
    {
        [SerializeField]
        public ElementList Spirits;

        public static string Path { get; protected set; }

#if UNITY_EDITOR

        void Awake()
        {
            Path = AssetDatabase.GetAssetPath(this);
            
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