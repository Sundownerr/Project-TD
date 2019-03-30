using System.Collections;
using System.Collections.Generic;
using Game;
using Game.Systems;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SlotWithCooldown : MonoBehaviour, IHaveDescription, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject CooldownPrefab;
    public Image CooldownImage;

    public ID EntityID { get; set; }

    [NaughtyAttributes.ResizableTextArea]
    public string Description;

    private void Awake()
    {
        var prefab = Instantiate(CooldownPrefab, transform.position, Quaternion.identity, transform);
        CooldownImage = prefab.GetComponent<Image>();
    }

    public void GetDescription() => ReferenceHolder.Get.DescriptionUISystem.ShowDescription(Description);

    public void OnPointerEnter(PointerEventData eventData) => GetDescription();

    public void OnPointerExit(PointerEventData eventData) => ReferenceHolder.Get.DescriptionUISystem.CloseDescription();
}
