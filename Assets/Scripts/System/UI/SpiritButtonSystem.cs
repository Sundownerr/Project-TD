using UnityEngine.UI;
using TMPro;
using System;
using Game.Enums;
using Game.Utility;
using Game.Systems;
using Game.Data.SpiritEntity;
using UnityEngine;

namespace Game.UI
{
    public class SpiritButtonSystem : ExtendedMonoBehaviour
    {
        public event Action<SpiritData> PlaceNewSpiritClicked;
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
                Debug.Log(1);
                var goldCost = SpiritData.Get(Numeral.ResourceCost).Value;
                var spiritLimit = SpiritData.Get(Enums.Spirit.SpiritLimit).Value;
                var magicCrystalsCost = SpiritData.Get(Enums.Spirit.MagicCrystalReq).Value;
                Debug.Log(Owner.ResourceSystem);
                if (Owner.ResourceSystem.CheckHaveResources(spiritLimit, goldCost, magicCrystalsCost))
                {
                    Debug.Log(3);
                    PlaceNewSpiritClicked?.Invoke(SpiritData);
                    Count--;

                    if (Count >= 1)
                    {
                        Debug.Log(4);
                        SpiritCountText.text = Count.ToString();
                        Debug.Log(5);
                    }
                    else
                    {
                        AllThisSpiritsPlaced?.Invoke(this);
                        Destroy(gameObject);
                    }
                }
            }
        }

        public void Set(SpiritData spiritData, PlayerSystem owner, Vector2 position, int index)
        {
            transform.GetChild(0).GetComponent<Image>().sprite = spiritData.Image;

            owner = Owner;
            SpiritData = spiritData;

            GetComponent<RectTransform>().localPosition = position * index;

            Count = 1;
        }
    }
}

