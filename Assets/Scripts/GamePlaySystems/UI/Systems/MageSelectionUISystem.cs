using System;
using System.Collections;
using System.Collections.Generic;
using Game;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MageSelectionUISystem : MonoBehaviour, IWindow
{
    public event EventHandler Activated;
    public event EventHandler<MageData> MageSelected;
    public Button SelectMageButton, CancelButton;
    public TextMeshProUGUI MageBaseDescription, MageAdvancedDescription;
    public Image MageImage;

    List<MageUI> mageUIs;
    MageData selectedMage;

    void Awake()
    {
        mageUIs = new List<MageUI>(gameObject.GetComponentsInChildren<MageUI>());
        mageUIs.ForEach(mageUI => mageUI.Selected += OnMageSelected);

        SelectMageButton.onClick.AddListener(() => MageSelected?.Invoke(null, selectedMage));
        CancelButton.onClick.AddListener(() => gameObject.SetActive(false));
        SelectMageButton.interactable = false;
    }

    void OnEnable()
    {
        Activated?.Invoke(null, null);
    }

    void OnMageSelected(object sender, MageData e)
    {
        selectedMage = e;
        SelectMageButton.interactable = true;
        UpdateMageInfo();

        void UpdateMageInfo()
        {
            MageImage.sprite = e.Image;
            MageImage.color += new Color(0, 0, 0, 1);
            MageImage.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = e.Name;
            MageBaseDescription.text = e.Description;
            MageAdvancedDescription.text = e.AdvancedDescription;
        }
    }

    void OnDestroy()
    {
        MageSelected = null;
        SelectMageButton.onClick.RemoveAllListeners();
        CancelButton.onClick.RemoveAllListeners();
    }
}
