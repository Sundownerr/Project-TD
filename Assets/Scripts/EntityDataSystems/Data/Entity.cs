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
        [SerializeField] protected new string name;
        [SerializeField, NaughtyAttributes.ResizableTextArea] protected string description;
        [SerializeField] protected int index = -1;

#if UNITY_EDITOR
        [ShowAssetPreview(125, 125)]
#endif
        [SerializeField] Sprite image;

        public int Index { get => index; set => index = value; }
        public string Name { get => name; protected set => name = value; }
        public string Description { get => description; protected set => description = value; }
        public Sprite Image { get => image; protected set => image = value; }
    }
}