using UnityEngine;
using System;

#if UNITY_EDITOR
using NaughtyAttributes;
using UnityEditor;
#endif

namespace Game
{
    [Serializable]
    public abstract class Entity : ScriptableObject, IIndexComponent
    {
        [SerializeField] public string Name;
        [SerializeField, NaughtyAttributes.ResizableTextArea] public string Description;

        [SerializeField] protected int index = -1;
        public int Index { get => index; set => index = value; }

#if UNITY_EDITOR
        [ShowAssetPreview(125, 125)]
#endif
        [SerializeField]
        public Sprite Image;
    }
}