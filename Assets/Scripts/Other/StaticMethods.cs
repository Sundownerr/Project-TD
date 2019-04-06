﻿using Game;
using Game.Systems;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Mirror;
using U = UnityEngine.Object;
using Game.Spirit.Data;
using Game.Spirit;
using Game.Cells;
using Game.Enemy;
using Game.Enums;
using Game.Enemy.Data;
using Lean.Localization;

public static class StaticMethods
{
    public static RangeSystem CreateRange(IPrefabComponent owner, double size, CollideWith collideType)
    {
        var range = UnityEngine.Object.Instantiate(ReferenceHolder.Get.RangePrefab, owner.Prefab.transform);
        range.transform.localScale = new Vector3((float)size, 0.001f, (float)size);

        var rangeSystem = range.GetComponent<RangeSystem>();
        rangeSystem.Owner = owner;
        rangeSystem.CollideType = collideType;

        return rangeSystem;
    }

    public static string KiloFormat(double num)
    {
        if (num >= 1000000000)return (num / 1000000000).ToString("#.0" + "B");
        if (num >= 1000000) return (num / 1000000).ToString("#" + "M");
        if (num >= 100000) return (num / 1000).ToString("#.0" + "K");
        if (num >= 10000) return (num / 1000).ToString("0.#" + "K");
        if (num >= 1000) return (num / 1000).ToString("0.#" + "K");

        return num.ToString("0.#");
    }

    public static string GetLocalized<EnumType>(this EnumType type) where EnumType: struct, Enum =>
        LeanLocalization.GetTranslationText(type.GetStringKey());

    public static Dictionary<EnumType, string> CreateStringKeyDictionary<EnumType>() where EnumType : Enum
    {
        var numerals = Enum.GetValues(typeof(EnumType));
        var dictionary = new Dictionary<EnumType, string>();
        var keyPrefix = 
            typeof(EnumType) == typeof(Numeral) ? "a" :
            typeof(EnumType) == typeof(Game.Enums.Spirit) ? "a" :
            typeof(EnumType) == typeof(Game.Enums.SpiritFlag) ? "a" :
            typeof(EnumType) == typeof(Game.Enums.Enemy) ? "a" :
            typeof(EnumType) == typeof(ElementType) ? "element" :
            typeof(EnumType) == typeof(RarityType) ? "rarity" :
            typeof(EnumType) == typeof(ArmorType) ? "e-armor" :
            typeof(EnumType) == typeof(EnemyType) ? "e-type" :
            typeof(EnumType) == typeof(RaceType) ? "e-race" :
            "error";

        for (int i = 0; i < numerals.Length; i++)
            dictionary.Add(
                (EnumType)numerals.GetValue(i),
                Enum.GetName(typeof(EnumType), i).StringEnumToStringKey(keyPrefix));

        return dictionary;
    }

    public static bool CheckLocalPlayer(NetworkPlayer player)
    {
        if (GameManager.Instance.GameState == GameState.SingleplayerInGame)
            return true;

        return GameManager.Instance.GameState == GameState.MultiplayerInGame && player.isLocalPlayer;
    }

    public static void CreatePlaceEffect(ElementType element, Vector3 position)
    {
        var placeEffect = U.Instantiate(
            ReferenceHolder.Get.ElementPlaceEffects[(int)element],
            position + Vector3.up * 5,
            Quaternion.identity);
        U.Destroy(placeEffect, placeEffect.GetComponent<ParticleSystem>().main.duration);
    }

    public static SpiritSystem CreateSpirit(SpiritData data, Cell cell, PlayerSystem owner)
    {
        var newSpiritPrefab = U.Instantiate(
            data.Prefab,
            cell.transform.position,
            Quaternion.identity,
            ReferenceHolder.Get.SpiritParent);

        var newSpirit = new SpiritSystem(newSpiritPrefab)
        {
            Data = data,
            UsedCell = cell.gameObject
        };

        newSpirit.SetSystem(owner);
        CreatePlaceEffect(newSpirit.Data.Base.Element, newSpiritPrefab.transform.position);

        return newSpirit;
    }

    public static SpiritSystem CreateSpirit(SpiritData data, Vector3 position, PlayerSystem owner)
    {
        var newSpiritPrefab = U.Instantiate(data.Prefab, position, Quaternion.identity, ReferenceHolder.Get.SpiritParent);
        var newSpirit = new SpiritSystem(newSpiritPrefab)
        {
            Data = data,
            UsedCell = null
        };

        newSpirit.SetSystem(owner);
        CreatePlaceEffect(newSpirit.Data.Base.Element, newSpiritPrefab.transform.position);
        return newSpirit;
    }


    public static EnemySystem CreateEnemy(EnemyData data, Vector3 position, PlayerSystem owner, Vector3[] waypoints, GameObject prefab = null)
    {

        var enemy = prefab ?? U.Instantiate(data.Prefab, position, Quaternion.identity, ReferenceHolder.Get.EnemyParent);
        var enemySystem = new EnemySystem(enemy, waypoints) { Data = data };

        enemySystem.SetSystem(owner);

        return enemySystem;
    }
}


public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.Items;
    }

    public static string ToJson<T>(T[] array)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper);
    }

    public static string ToJson<T>(T[] array, bool prettyPrint)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper, prettyPrint);
    }

    [Serializable]
    private class Wrapper<T>
    {
        public T[] Items;
    }
}

public static class StaticRandom
{
    private static int seed;

    private static ThreadLocal<System.Random> threadLocal = new ThreadLocal<System.Random>
                    (() => new System.Random(Interlocked.Increment(ref seed)));

    static StaticRandom() => seed = Environment.TickCount;

    public static System.Random Instance { get { return threadLocal.Value; } }
}
