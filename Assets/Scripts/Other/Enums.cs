﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Enums
{
    public enum ConsumableType
    {
        Essence,
        MagicCrystals,
        SpiritVessel
    }

    public enum CollideWith
    {
        Enemies,
        Spirits,
        EnemiesAndSpirits
    }

    public enum EnemyType
    {
        Small,
        Normal,
        Boss,
        Commander,
        Flying
    }

    [Serializable]
    public enum RaceType
    {
        Humanoid = 0,
        Magical = 1,
        Undead = 2,
        Nature = 3,
        RiftCreature = 4
    }

    [Serializable]
    public enum ElementType
    {
        Astral = 0,
        Darkness = 1,
        Ice = 2,
        Iron = 3,
        Storm = 4,
        Nature = 5,
        Fire = 6,
    }

    [Serializable]
    public enum RarityType
    {
        Common = 0,
        Uncommon = 1,
        Rare = 2,
        Unique = 3,
    }

    [Serializable]
    public enum Numeral
    {
        Exp,
        Level,
        WaveLevel,
        GoldCost,
        ItemDropRatio,
        ItemQuialityRatio,
        BuffDuration,
        DebuffDuration,
    }

    [Serializable]
    public enum Spirit
    {
        AttackSpeedModifier,
        ElementLevel,
        SpiritLimit,
        MagicCrystalReq,
        MaxInventorySlots,
        Damage,
        Range,
        Mana,
        ManaRegen,
        AttackSpeed,
        TriggerChance,
        CritChance,
        CritMultiplier,
        MulticritCount,
        SpellDamage,
        SpellCritChance,
        GoldRatio,
        ExpRatio,
    }

    [Serializable]
    public enum Enemy
    {
        HealthRegen,
        ArmorValue,
        DefaultMoveSpeed,
        MoveSpeed,
        Health,
        MaxHealth,
    }

    [Serializable]
    public enum SpiritFlag
    {
        IsGradeSpirit,
        CanAttackFlying
    }

    [Serializable]
    public enum Increase
    {
        ByPercent,
        ByValue
    }

    public enum From
    {
        Applied,
        Base
    }
}