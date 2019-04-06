using System;
using System.Collections;
using System.Collections.Generic;
using Game;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MageSelectionUISystem : MonoBehaviour
{
    public event EventHandler<MageData> MageSelected = delegate { };
    public Button SelectMageButton, CancelButton;
    public TextMeshProUGUI MageName, MageBaseDescription, MageAdvancedDescription;

    private List<MageUI> mageUIs;
    private MageData selectedMage;

    private void Awake()
    {
        mageUIs = new List<MageUI>(gameObject.GetComponentsInChildren<MageUI>());
        mageUIs.ForEach(mageUI => mageUI.Selected += OnMageSelected);

        SelectMageButton.onClick.AddListener(() => MageSelected?.Invoke(null, selectedMage));
        CancelButton.onClick.AddListener(() => gameObject.SetActive(false));
        SelectMageButton.interactable = false;
    }

    private void OnMageSelected(object sender, MageData e)
    {
        selectedMage = e;
        SelectMageButton.interactable = true;
        UpdateMageInfo();

        void UpdateMageInfo()
        {
            MageName.text = e.Name;
            MageBaseDescription.text = e.Description;
            MageAdvancedDescription.text = e.AdvancedDescription;
        }
    }

    private void OnDestroy()
    {
        MageSelected = null;
        SelectMageButton.onClick.RemoveAllListeners();
        CancelButton.onClick.RemoveAllListeners();
    }
}
