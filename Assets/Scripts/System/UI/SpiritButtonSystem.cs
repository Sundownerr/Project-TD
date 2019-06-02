using UnityEngine.UI;
using TMPro;
using System;
using Game.Enums;
using Game.Utility;
using Game.Systems;
using Game.Data.SpiritEntity;

namespace Game.UI
{
    public class SpiritButtonSystem : ExtendedMonoBehaviour
    {
        public event Action<SpiritData> PlaceNewSpirit;
        public event Action<SpiritButtonSystem> AllThisSpiritsPlaced;

        public PlayerSystem Owner { get; set; }
        public int Count { get; set; }
        public RarityType Rarity => SpiritData.Base.Rarity;
        public ElementType Element => SpiritData.Base.Element;
        public SpiritData SpiritData { get; set; }
        public TextMeshProUGUI SpiritCountText { get; set; }


        protected override void Awake()
        {
            base.Awake();

            transform.GetChild(0).GetComponent<Button>().onClick.AddListener(OnClick);
            SpiritCountText = transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();

            void OnClick()
            {
                var goldCost = SpiritData.Get(Numeral.ResourceCost).Value;
                var spiritLimit = SpiritData.Get(Enums.Spirit.SpiritLimit).Value;
                var magicCrystalsCost = SpiritData.Get(Enums.Spirit.MagicCrystalReq).Value;

                if (Owner.ResourceSystem.CheckHaveResources(spiritLimit, goldCost, magicCrystalsCost))
                {
                    PlaceNewSpirit?.Invoke(SpiritData);
                    Count--;

                    if (Count >= 1)
                    {
                        SpiritCountText.text = Count.ToString();
                    }
                    else
                    {
                        AllThisSpiritsPlaced?.Invoke(this);
                        Destroy(gameObject);
                    }
                }
            }
        }
    }
}

