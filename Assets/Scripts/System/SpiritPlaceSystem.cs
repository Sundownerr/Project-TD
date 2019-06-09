using System;
using Game.Enums;
using Game.Data.NetworkRequests;
using Game.Managers;
using Game.Utility;
using Game.Systems.Spirit;
using Game.Data.SpiritEntity;
using Game.Utility.Creator;

namespace Game.Systems.Spirit
{
    public class PlaceSystem
    {
        public event Action<SpiritSystem> SpiritPlaced;
        public event Action<SpiritCreationRequest> SpiritCreationRequested;

        public PlayerSystem Owner { get; set; }

        public PlaceSystem(PlayerSystem player)
        {
            Owner = player;
        }

        public void SetSystem()
        {
            UIManager.Instance.BuildUISystem.PlaceNewSpiritClicked += OnPlacingNewSpirit;
        }

        public void NetworkCreateSpirit(SpiritSystem spirit)
        {
            SpiritPlaced?.Invoke(spirit);
        }

        void OnPlacingNewSpirit(SpiritData spiritData)
        {
            if (Owner.CellControlSystem.IsGridBuilded)
            {
                var newSpiritLimit = spiritData.Get(Enums.Spirit.SpiritLimit).Value;
                var newGoldCost = spiritData.Get(Numeral.ResourceCost).Value;
                var newMagicCrystalCost = spiritData.Get(Enums.Spirit.MagicCrystalReq).Value;

                if (Owner.ResourceSystem.CheckHaveResources(newSpiritLimit, newGoldCost, newMagicCrystalCost))
                {
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
                        var newSpirit = Create.Spirit(spiritData, Owner.CellControlSystem.ChoosedCell, true);
                        SpiritPlaced?.Invoke(newSpirit);
                    }
                }
            }
        }
    }
}