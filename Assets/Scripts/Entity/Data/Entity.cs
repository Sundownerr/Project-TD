using UnityEngine;
using System.Collections.Generic;
using Game.Systems;
using System.Text;
using NaughtyAttributes;
using System;

namespace Game
{
    [Serializable]
    public abstract class Entity : ScriptableObject
    {
        [SerializeField] public ID ID = new ID();

        [SerializeField] public string Name;
        [SerializeField] public string Description;

        [SerializeField, ShowAssetPreview(125, 125)]
        public GameObject Prefab;

        [SerializeField, ShowAssetPreview(125, 125)]
        public Sprite Image;
    }
}