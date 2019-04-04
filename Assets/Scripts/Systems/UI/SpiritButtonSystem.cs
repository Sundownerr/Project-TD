using UnityEngine.UI;
using Game.Spirit.Data;
using TMPro;
using System;
using Game.Enums;

namespace Game.Systems
{
    public class SpiritButtonSystem : ExtendedMonoBehaviour
    {
        public PlayerSystem Owner { get; set; }
        public int Count { get; set; }
        public SpiritData SpiritData { get; set; }
        public TextMeshProUGUI SpiritCountText { get; set; }
        public event EventHandler<SpiritData> PlaceNewSpirit = delegate { };
        public event EventHandler<SpiritButtonSystem> AllThisSpiritsPlaced = delegate { };

        protected override void Awake()
        {
            base.Awake();

            transform.GetChild(0).GetComponent<Button>().onClick.AddListener(OnClick);
            SpiritCountText = transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
        }

        private void OnClick()
        {
            var goldCost            = SpiritData.Get(Numeral.GoldCost).Value;
            var spiritLimit          = SpiritData.Get(Enums.Spirit.SpiritLimit).Value;
            var magicCrystalsCost   = SpiritData.Get(Enums.Spirit.MagicCrystalReq).Value;

            if (Owner.ResourceSystem.CheckHaveResources(spiritLimit, goldCost, magicCrystalsCost))
            {
                PlaceNewSpirit?.Invoke(null, SpiritData);
                Count--;

                if (Count >= 1)
                    SpiritCountText.text = Count.ToString();
                else
                {
                    AllThisSpiritsPlaced?.Invoke(null, this);
                    Destroy(gameObject);
                }                              
            }
        }
    }
}

