
using System;
using UnityEngine;

namespace Game.Systems
{
    public class ElementSystem
    {
        public event EventHandler<int> LearnedElement;

        PlayerSystem owner;

        public ElementSystem(PlayerSystem player) => owner = player;

        public void SetSystem()
        {
            owner.ResourceSystem.ResourcesChanged += OnResourcesChanged;
        }

        void OnResourcesChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < owner.ElementUISystem.Buttons.Length; i++)
            {
                var button = owner.ElementUISystem.Buttons[i];
                var check = CheckCanLearn(owner.Data.ElementLevels[i]);
                button.interactable = check.canLearn;
            }
        }

        public void LearnElement(int elementId)
        {
            var check = CheckCanLearn(owner.Data.ElementLevels[elementId]);

            if (check.canLearn)
            {
                owner.Data.ElementLevels[elementId]++;
                LearnedElement?.Invoke(null, check.learnCost);
            }
        }

        (bool canLearn, int learnCost) CheckCanLearn(int elementLevel)
        {
            var baseLearnCost = 20;
            var levelLimit = 15;
            var learnCost = elementLevel + baseLearnCost;
            var canLearn = elementLevel < levelLimit;
            var isLearnCostOk = learnCost <= owner.Data.Resources.MagicCrystals;

            return (canLearn && isLearnCostOk, learnCost);
        }
    }
}
