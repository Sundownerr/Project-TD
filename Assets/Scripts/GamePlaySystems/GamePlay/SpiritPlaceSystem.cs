using System;
using Game.Enums;
using Game.Data.NetworkRequests;
using Game.Managers;
using Game.Utility;
using Game.Systems.Spirit;
using Game.Data.Spirit;

namespace Game.Systems
{
    public class SpiritPlaceSystem
    {
        public event Action<SpiritSystem> SpiritPlaced;
        public event Action<SpiritCreationRequest> SpiritCreationRequested;

        public PlayerSystem Owner { get; set; }

        public SpiritPlaceSystem(PlayerSystem player)
        {
            Owner = player;
        }

        public void SetSystem() { }

        public void NetworkCreateSpirit(SpiritSystem spirit)
        {
            SpiritPlaced?.Invoke(spirit);
        }

        public void OnPlacingNewSpirit(SpiritData spiritData)
        {
            if (Owner.CellControlSystem.IsGridBuilded)
            {
                var newSpiritLimit = spiritData.Get(Enums.Spirit.SpiritLimit).Value;
                var newGoldCost = spiritData.Get(Numeral.ResourceCost).Value;
                var newMagicCrystalCost = spiritData.Get(Enums.Spirit.MagicCrystalReq).Value;

                if (Owner.ResourceSystem.CheckHaveResources(newSpiritLimit, newGoldCost, newMagicCrystalCost))
                    if (GameManager.Instance.GameState == GameState.InGameMultiplayer)
                    {
                        var choosedCell = Owner.CellControlSystem.ChoosedCell;

                        var position = new Coordinates3D(
                            (int)choosedCell.transform.position.x,
                            (int)choosedCell.transform.position.y,
                            (int)choosedCell.transform.position.z);

                        SpiritCreationRequested?.Invoke(new SpiritCreationRequest()
                        {
                            Index = spiritData.Index,
                            Rarity = (int)spiritData.Base.Rarity,
                            Element = (int)spiritData.Base.Element,
                            Position = position,
                            CellIndex = Owner.CellControlSystem.Cells.IndexOf(choosedCell)
                        });
                    }
                    else
                    {
                        var newSpirit = StaticMethods.CreateSpirit(spiritData, Owner.CellControlSystem.ChoosedCell, true);
                        SpiritPlaced?.Invoke(newSpirit);
                    }
            }
        }
    }
}