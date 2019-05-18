using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Systems;
using Game.Enums;

namespace Game.Data.Player
{
    public class Resources
    {
        public double MagicCrystals;
        public double Resource;
        public double CurrentSpiritLimit;
        public double MaxSpiritLimit;
        public double StartSpiritRerollCount;
    }

    [Serializable]
    public class Player
    {
        [SerializeField] Inventory inventory;
        [SerializeField] Resources resources;
        [SerializeField] List<int> elementLevels;

        public PlayerSystem System { get; set; }
        public Inventory Inventory { get => inventory; set => inventory = value; }
        public Resources Resources { get => resources; set => resources = value; }
        public List<int> ElementLevels { get => elementLevels; set => elementLevels = value; }

        public Player()
        {
            var elementAmount = Enum.GetValues(typeof(ElementType)).Length;
            ElementLevels = new List<int>();

            Inventory = new Inventory(30);
            Resources = new Resources
            {
                Resource = 100,
                MagicCrystals = 100,
                MaxSpiritLimit = 500,
                StartSpiritRerollCount = 3
            };

            for (int i = 0; i < elementAmount; i++)
            {
                ElementLevels.Add(0);
            }
        }
    }
}