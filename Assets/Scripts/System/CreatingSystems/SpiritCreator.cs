using System;
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
            var elementLevels = Owner.Data.ElementLevels;
            var dataBaseElements =  ReferenceHolder.Instance.SpiritDB.Data;

            for (int lvldUpElementId = 0; lvldUpElementId < elementLevels.Count; lvldUpElementId++)
                if (elementLevels[lvldUpElementId] > 0)
                    for (int dbElementId = 0; dbElementId < dataBaseElements.Count; dbElementId++)
                        if (dbElementId == lvldUpElementId)
                            GetNewSpirit(lvldUpElementId);

            AddedNewAvailableSpirit?.Invoke();

            void GetNewSpirit(int elementId)
            {
                var spirits =  ReferenceHolder.Instance.SpiritDB.Data;

                for (int i = 0; i < spirits.Count; i++)
                {
                    if (spirits[i].Get(Numeral.WaveLevel).Value <= Owner.WaveSystem.WaveNumber)
                    {
                        Owner.AvailableSpirits.Add(spirits[i]);
                        Owner.BuildUISystem.AddSpiritButton(spirits[i]);
                    }
                }
            }                                       
        }
    }
}