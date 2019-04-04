using UnityEngine;
using System.Collections.Generic;
using Game.Systems;
using System.Text;
using NaughtyAttributes;
using System;

namespace Game
{
    [Serializable]
    public abstract class Entity : ScriptableObject, IIDComponent
    {
        [SerializeField] public ID ID { get; set; } = new ID();
        [SerializeField] public string Name;
        [SerializeField, NaughtyAttributes.ResizableTextArea] public string Description;

        [SerializeField, ShowAssetPreview(125, 125)]
        public Sprite Image;
    }
}