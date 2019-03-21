
using System;
using UnityEngine;

namespace Game.Systems
{
    public class ElementSystem
    {
        public event EventHandler<int> LearnedElement = delegate { };

        private PlayerSystem player;

        public ElementSystem(PlayerSystem player)
        {
            this.player = player;
        }

        public void LearnElement(int elementId)
        {
            if (CheckCanLearn(player.Data.ElementLevels[elementId], out int learnCost))
            {
                player.Data.ElementLevels[elementId]++;
                LearnedElement?.Invoke(null, learnCost);
            }

            #region Helper functions 

            bool CheckCanLearn(int elementLevel, out int learnPrice)
            {
                var baseLearnCost = 20;
                var levelLimit = 15;
                learnPrice = elementLevel + baseLearnCost;
                var isCanLearn = elementLevel < levelLimit;
                var isLearnCostOk = learnPrice <= player.Data.Resources.MagicCrystals;

                return isCanLearn && isLearnCostOk;
            }

            #endregion
        }
    }
}
