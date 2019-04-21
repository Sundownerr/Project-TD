using System;
using System.Collections;
using Game.Spirit;
using Game.Spirit.Data;
using Game.Spirit.Data.Stats;
using UnityEngine;
using U = UnityEngine.Object;
using Mirror;
using Game.Enums;

namespace Game.Systems
{
    public class SpiritPlaceSystem
    {
        public event EventHandler<SpiritSystem> SpiritPlaced;
        public event EventHandler<SpiritCreationRequest> SpiritCreationRequested;

        public PlayerSystem Owner { get; set; }

        public SpiritPlaceSystem(PlayerSystem player)
        {
            Owner = player;
        }

        public void SetSystem() { }

        public void NetworkCreateSpirit(SpiritSystem spirit)
        {
            SpiritPlaced?.Invoke(null, spirit);
        }

        public void OnPlacingNewSpirit(object _, SpiritData spiritData)
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

                        SpiritCreationRequested?.Invoke(null, new SpiritCreationRequest()
                        {
                            ID = spiritData.ID,
                            Rarity = (int)spiritData.Base.Rarity,
                            Element = (int)spiritData.Base.Element,
                            DataBaseIndex = spiritData.ID[spiritData.ID.Count - 1],
                            Position = position,
                            CellIndex = Owner.CellControlSystem.Cells.IndexOf(choosedCell)
                        });
                    }
                    else
                    {
                        var newSpirit = StaticMethods.CreateSpirit(spiritData, Owner.CellControlSystem.ChoosedCell, true);
                        SpiritPlaced?.Invoke(null, newSpirit);
                    }
            }
        }
    }
}