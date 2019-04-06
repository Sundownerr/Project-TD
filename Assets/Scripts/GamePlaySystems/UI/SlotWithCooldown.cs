using System.Collections;
using System.Collections.Generic;
using Game;
using Game.Systems;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SlotWithCooldown : DescriptionBlock
{
    public GameObject CooldownPrefab;
    public Image CooldownImage;

    public ID EntityID { get; set; }
}
