﻿using System;
using System.Collections.Generic;
using Game.Spirit.Data.Stats;
using UnityEngine;
using Game.Systems;


namespace Game.Data
{
    public class Resources
    {
        public double MagicCrystals, Gold, CurrentSpiritLimit, MaxSpiritLimit, StartSpiritRerollCount;

       
    }

    [Serializable]
    public class Player : ScriptableObject
    {
        [SerializeField]
        public Inventory Inventory;

        [SerializeField]
        public Resources Resources;

        [SerializeField]
        public List<int> ElementLevels;

        public PlayerSystem System { get; set; }
 
        private void Awake()
        {
            var elementAmount = Enum.GetValues(typeof(ElementType)).Length;
            ElementLevels = new List<int>();

            Inventory = new Inventory(30);
            Resources = new Resources
            {
                Gold = 100,
                MagicCrystals = 100,
                MaxSpiritLimit = 500,
                StartSpiritRerollCount = 3
            };

            for (int i = 0; i < elementAmount; i++)
                ElementLevels.Add(0);
        }
    }
}