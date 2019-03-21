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
        public event EventHandler<SpiritSystem> SpiritCreated = delegate{};
        public event EventHandler<SpiritSystem> SpiritPlaced = delegate{};
        public event EventHandler<SpiritCreationRequest> SpiritCreationRequested = delegate { };

        public PlayerSystem Owner { get; set; }
  
        public SpiritPlaceSystem(PlayerSystem player)
        {
            Owner = player;         
        }

        public void SetSystem()
        {
            if(Owner.NetworkPlayer != null)
            {
                NetworkRequest.SpiritCreatingRequestDone += (s, e) => SpiritPlaced?.Invoke(s, e);
            }
        }

        public void OnPlacingNewSpirit(object sender, SpiritData spiritData)
        {
            if (Owner.CellControlSystem.IsGridBuilded)
            {
                var newSpiritLimit = spiritData.Get(Numeral.SpiritLimit, From.Base).Value;
                var newGoldCost = spiritData.Get(Numeral.GoldCost, From.Base).Value;
                var newMagicCrystalCost = spiritData.Get(Numeral.MagicCrystalReq, From.Base).Value;

                if (Owner.ResourceSystem.CheckHaveResources(newSpiritLimit, newGoldCost, newMagicCrystalCost))
                {
                    if (GameManager.Instance.GameState == GameState.MultiplayerInGame)
                        CreateRequest();
                    else
                    {
                        var newSpirit = StaticMethods.CreateSpirit(spiritData, Owner.CellControlSystem.ChoosedCell, Owner);
                        SpiritPlaced?.Invoke(this, newSpirit);
                    }
                }
            }

            #region Helper functions

            void CreateRequest()
            {
                var requestData = new SpiritCreationRequest()
                {
                    Data = spiritData,
                    Position = Owner.CellControlSystem.ChoosedCell.transform.position
                };

                SpiritCreationRequested?.Invoke(this, requestData);
            } 

            #endregion
        }
    }  
}