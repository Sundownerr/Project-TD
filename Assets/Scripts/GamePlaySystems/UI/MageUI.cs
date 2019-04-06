using System.Collections;
using System.Collections.Generic;
using Game;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Game.Systems;
using System;

public class MageUI : MonoBehaviour
{
    public event EventHandler<MageData> Selected = delegate { };
    public MageData MageData;

    private Button button;
    
    private void Awake()
    {
        MageData = Instantiate(MageData);
        button = transform.GetChild(0).GetComponent<Button>();
        button.onClick.AddListener(() => Selected?.Invoke(null, MageData));    
    }

    private void OnDestroy()
    {
        Selected = null;
        button.onClick.RemoveAllListeners();    
    }
}
