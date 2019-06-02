using System;
using System.Collections.Generic;
using Game.Enums;
using Game.Managers;

namespace Game.Systems.Spirit
{
    public class CreatingSystem
    {
        public event Action AddedNewAvailableSpirit;

        public PlayerSystem Owner { get; set; }

        public CreatingSystem(PlayerSystem player)
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
   
            var spiritsFromDatabase = ReferenceHolder.Instance.SpiritDB.Data;
            var leveledUpElements = Owner.Data.ElementLevels.FindAll(elementLevel => elementLevel > 0);

            for (int elementIndex = 0; elementIndex < leveledUpElements.Count; elementIndex++)
            {
                GetNewSpirits((ElementType)elementIndex)
                    .ForEach(spirit =>
                    {
                        Owner.AvailableSpirits.Add(spirit);
                        Owner.BuildUISystem.AddSpiritButton(spirit);
                    });
            }
              
            AddedNewAvailableSpirit?.Invoke();

            List<Data.SpiritEntity.SpiritData> GetNewSpirits(ElementType element)
            {
                var spirits = ReferenceHolder.Instance.SpiritDB.Data;
                var newSpirits = new List<Data.SpiritEntity.SpiritData>();

                spirits.FindAll(spiritInDatabase => spiritInDatabase.Base.Element == element)
                    .ForEach(spirit =>
                    {
                        if (spirit.Get(Numeral.WaveLevel).Value <= Owner.WaveSystem.WaveNumber)
                        {
                            newSpirits.Add(spirit);
                        }
                    });

                return newSpirits;
            }
        }
    }
}