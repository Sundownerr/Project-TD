using System.Collections;
using System.Collections.Generic;
using Game;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Game.Systems;
using System;
using TMPro;

public class MageUI : MonoBehaviour
{
    public event EventHandler<MageData> Selected = delegate { };
    public MageData MageData;

    Button button;
    
    void Awake()
    {
        MageData = Instantiate(MageData);
        MageData.GenerateDescription();
        button = transform.GetChild(0).GetComponent<Button>();
        button.onClick.AddListener(() => Selected?.Invoke(null, MageData));    

        GetComponentInChildren<Image>().sprite = MageData.Image;
        GetComponentInChildren<TextMeshProUGUI>().text = MageData.Name;
    }

    void OnDestroy()
    {
        Destroy(MageData);
        Selected = null;
        button.onClick.RemoveAllListeners();    
    }
}
