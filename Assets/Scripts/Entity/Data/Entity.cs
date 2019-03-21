using UnityEngine;
using System.Collections.Generic;
using Game.Systems;
using System.Text;
using NaughtyAttributes;

namespace Game
{
    public abstract class Entity : ScriptableObject
    {
        public List<int> Id;

        public string Name;
        public string Description;

        [ShowAssetPreview(125, 125)]
        public GameObject Prefab;

        [ShowAssetPreview(125, 125)]
        public Sprite Image;
    }
}