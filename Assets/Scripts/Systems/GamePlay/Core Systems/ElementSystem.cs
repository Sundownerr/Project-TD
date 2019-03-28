
using System;
using UnityEngine;

namespace Game.Systems
{
    public class ElementSystem
    {
        public event EventHandler<int> LearnedElement = delegate { };

        private PlayerSystem owner;

        public ElementSystem(PlayerSystem player) => owner = player;
        
        public void LearnElement(int elementId)
        {
            var check = CheckCanLearn(owner.Data.ElementLevels[elementId]);

            if (check.canLearn)
            {
                owner.Data.ElementLevels[elementId]++;
                LearnedElement?.Invoke(null, check.learnCost);
            }

            #region Helper functions 

            (bool canLearn, int learnCost) CheckCanLearn(int elementLevel)
            {
                var baseLearnCost = 20;
                var levelLimit = 15;
                var learnCost = elementLevel + baseLearnCost;
                var canLearn = elementLevel < levelLimit;
                var isLearnCostOk = learnCost <= owner.Data.Resources.MagicCrystals;

                return (canLearn && isLearnCostOk, learnCost);
            }

            #endregion
        }
    }
}
