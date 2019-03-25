using System;
using System.Collections;
using Game.Spirit;
using Game.Spirit.Data;
using Game.Spirit.Data.Stats;
using UnityEngine;
using U = UnityEngine.Object;
using Mirror;

namespace Game.Systems
{
    public class SpiritPlaceSystem
    {
        public event EventHandler<SpiritSystem> SpiritCreated = delegate { };
        public event EventHandler<SpiritSystem> SpiritPlaced = delegate { };
        public event EventHandler<SpiritCreationRequest> SpiritCreationRequested = delegate { };

        public PlayerSystem Owner { get; set; }

        public SpiritPlaceSystem(PlayerSystem player)
        {
            Owner = player;
        }

        public void SetSystem()
        {
            if (Owner.NetworkPlayer != null)
            {
                Owner.NetworkPlayer.SpiritCreatingRequestDone += (_, e) => SpiritPlaced?.Invoke(_, e);
            }
        }

        public void OnPlacingNewSpirit(object _, SpiritData spiritData)
        {
            if (Owner.CellControlSystem.IsGridBuilded)
            {
                var newSpiritLimit = spiritData.Get(Numeral.SpiritLimit, From.Base).Value;
                var newGoldCost = spiritData.Get(Numeral.GoldCost, From.Base).Value;
                var newMagicCrystalCost = spiritData.Get(Numeral.MagicCrystalReq, From.Base).Value;

                if (Owner.ResourceSystem.CheckHaveResources(newSpiritLimit, newGoldCost, newMagicCrystalCost))
                    if (GameManager.Instance.GameState == GameState.MultiplayerInGame)
                    {
                        var position = new Coordinates3D(
                            (int)Owner.CellControlSystem.ChoosedCell.transform.position.x,
                            (int)Owner.CellControlSystem.ChoosedCell.transform.position.y,
                            (int)Owner.CellControlSystem.ChoosedCell.transform.position.z);

                        SpiritCreationRequested?.Invoke(null, new SpiritCreationRequest(spiritData.ID, (int)spiritData.Rarity, (int)spiritData.Element, position));
                    }
                    else
                    {
                        var newSpirit = StaticMethods.CreateSpirit(spiritData, Owner.CellControlSystem.ChoosedCell, Owner);
                        SpiritPlaced?.Invoke(null, newSpirit);
                    }               
            }
        }
    }
}