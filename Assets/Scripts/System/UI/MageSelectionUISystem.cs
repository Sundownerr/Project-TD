using System;
using System.Collections;
using System.Collections.Generic;
using Game;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Game.Consts;
using Game.Data.Mage;
using Game.Managers;

namespace Game.UI
{
    public class MageSelectionUISystem : UIWindow
    {
        public event Action<MageData> MageSelected;

        public Button SelectMageButton;
        public TextMeshProUGUI MageBaseDescription, MageAdvancedDescription;
        public Image MageImage;

        List<MageUI> mageUIs;
        MageData selectedMage;

        void Awake()
        {
            defaultYs = new float[] { transform.GetChild(0).localPosition.y };
            mageUIs = new List<MageUI>(gameObject.GetComponentsInChildren<MageUI>());
            mageUIs.ForEach(mageUI => mageUI.Selected += OnMageSelected);

            SelectMageButton.onClick.AddListener(OnSelectMageClick);

            void OnSelectMageClick() => MageSelected?.Invoke(selectedMage);

            void OnMageSelected(MageData e) => UpdateMageInfo(e);
        }

        void Start()
        {
            UpdateMageInfo(mageUIs[0].MageData);
        }

        void UpdateMageInfo(MageData mage)
        {
            selectedMage = mage;

            // DELETE LATER
            GameData.Instance.ChoosedMage = mage;
            // DELETE LATER
            
            MageImage.sprite = mage.Image;
            MageImage.color += new Color(0, 0, 0, 1);
            MageImage.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = mage.Name;
            MageBaseDescription.text = mage.Description;
            MageAdvancedDescription.text = mage.AdvancedDescription;
        }

        void OnDestroy()
        {
            MageSelected = null;
            SelectMageButton.onClick.RemoveAllListeners();
        }

        public override void Open(float timeToComplete = NumberConsts.UIAnimSpeed)
        {
            mageUIs.ForEach(mageUI => mageUI.MageData.GenerateDescription());
            base.Open(timeToComplete);
        }
    }
}