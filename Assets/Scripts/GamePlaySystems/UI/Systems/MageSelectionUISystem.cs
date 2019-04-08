using System;
using System.Collections;
using System.Collections.Generic;
using Game;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class MageSelectionUISystem : MonoBehaviour, IUIWindow
{
    public event EventHandler<MageData> MageSelected;

    public Button SelectMageButton;
    public TextMeshProUGUI MageBaseDescription, MageAdvancedDescription;
    public Image MageImage;

    List<MageUI> mageUIs;
    MageData selectedMage;
    float defaultY;

    void Awake()
    {
    
        defaultY = transform.GetChild(0).position.y;
        mageUIs = new List<MageUI>(gameObject.GetComponentsInChildren<MageUI>());
        mageUIs.ForEach(mageUI => mageUI.Selected += OnMageSelected);

        SelectMageButton.onClick.AddListener(() => MageSelected?.Invoke(null, selectedMage));
        SelectMageButton.interactable = false;
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
    }

    public void Open()
    {
        GameManager.Instance.GameState = GameState.SelectingMage; 
        transform.GetChild(0).DOLocalMoveY(0, 0.5f);
    }

    public void Close()
    {
       transform.GetChild(0).DOLocalMoveY(defaultY, 0.5f);
    }
}
