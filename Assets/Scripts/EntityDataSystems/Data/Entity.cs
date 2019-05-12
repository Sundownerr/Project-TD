using UnityEngine;
using System;

#if UNITY_EDITOR
using NaughtyAttributes;
using UnityEditor;
#endif

namespace Game
{
    [Serializable]
    public abstract class Entity : ScriptableObject, IIDComponent
    {
        [SerializeField] protected ID id;
        public ID ID { get => id; set => id = value; }
        [SerializeField] public string Name;
        [SerializeField, NaughtyAttributes.ResizableTextArea] public string Description;

        [SerializeField] protected int index = -1;
        public int Index { get => index; protected set => index = value; } 

#if UNITY_EDITOR
        [ShowAssetPreview(125, 125)]
#endif
        [SerializeField]
        public Sprite Image;

        protected virtual void Awake()
        {
            ID.Add(Index);
        }

        public bool Compare(Entity other)
        {
            return ID.Compare(other.ID);
        }

#if UNITY_EDITOR
        [Button]
        public void ClearID()
        {
            ID = new ID();
            EditorUtility.SetDirty(this);
        }
#endif
    }
}