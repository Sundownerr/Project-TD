using System;
using System.Collections.Generic;
using Game.Enums;
using Game.Spirit;
using Game.Spirit.Data.Stats;
using UnityEngine;
using U = UnityEngine.Object;

namespace Game.Systems
{
    public class SpiritCreatingSystem
    {
        public event EventHandler AddedNewAvailableSpirit = delegate { };

        public PlayerSystem Owner { get; set; }

        public SpiritCreatingSystem(PlayerSystem player)
        {
            Owner = player;
        }

        public void SetSystem() { }

        public void CreateRandomSpirit()
        {
            // if (GM.Instance.PlayerData.StartTowerRerollCount > 0)
            // {
            //     GM.Instance.AvailableTowerList.Clear();
            //     GM.Instance.PlayerData.StartTowerRerollCount--;
            // }
            var elementLevels = Owner.Data.ElementLevels;
            var dataBaseElements = ReferenceHolder.Get.SpiritDataBase.Spirits.Elements;

            for (int lvldUpElementId = 0; lvldUpElementId < elementLevels.Count; lvldUpElementId++)
                if (elementLevels[lvldUpElementId] > 0)
                    for (int dbElementId = 0; dbElementId < dataBaseElements.Length; dbElementId++)
                        if (dbElementId == lvldUpElementId)
                            GetNewSpirit(lvldUpElementId);

            AddedNewAvailableSpirit?.Invoke(null, null);

            #region  Helper functions

            void GetNewSpirit(int elementId)
            {
                var elements = ReferenceHolder.Get.SpiritDataBase.Spirits.Elements;


                for (int i = 0; i < elements[elementId].Rarities.Length; i++)
                {
                    var rarities = elements[elementId].Rarities[i];
                    for (int j = 0; j < rarities.Spirits.Count; j++)
                    {
                        if (rarities.Spirits[j].Get(Numeral.WaveLevel).Value <= Owner.WaveSystem.WaveNumber)
                        {
                            Owner.AvailableSpirits.Add(rarities.Spirits[j]);
                            Owner.BuildUISystem.AddSpiritButton(rarities.Spirits[j]);
                        }
                    }
                }
            }

            #endregion                                            
        }
    }
}