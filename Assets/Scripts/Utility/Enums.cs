using System;
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


    public enum GameState
    {
        InMenu,
        LoadingGame,
        UnloadingGame,
        InGameSingleplayer,
        InGameMultiplayer
    }

    public enum UIState
    {
        MainMenu,
        BrowsingLobbies,
        CreatingLobby,
        InLobby,
        SelectingMage
    }

    public enum CollideWith
    {
        Enemies,
        Spirits
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
        Fire = 6
    }

    [Serializable]
    public enum RarityType
    {
        Common = 0,
        Uncommon = 1,
        Rare = 2,
        Unique = 3
    }

    [Serializable]
    public enum Numeral
    {
        Exp,
        Level,
        WaveLevel,
        ResourceCost,
        ItemDropRate,
        ItemQualityRate,
        BuffTime,
        DebuffTime
    }

    [Serializable]
    public enum Spirit
    {
        AttackSpeed,
        ElementLevel,
        SpiritLimit,
        MagicCrystalReq,
        MaxInventorySlots,
        Damage,
        Range,
        Mana,
        ManaRegen,
        AttackDelay,
        TriggerChance,
        CritChance,
        CritMultiplier,
        MulticritCount,
        SpellDamage,
        SpellCritChance,
        ResourceRate,
        ExpRate
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
        Mana,
        ManaRegen
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