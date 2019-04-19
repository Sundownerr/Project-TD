using Game;
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

    public static bool CheckLocalPlayer(NetworkPlayer player)
    {
        if (GameManager.Instance.GameState == GameState.InGameSingleplayer)
            return true;

        return GameManager.Instance.GameState == GameState.InGameMultiplayer && player.isLocalPlayer;
    }

    public static void CreatePlaceEffect(ElementType element, Vector3 position)
    {
        var placeEffect = U.Instantiate(
            ReferenceHolder.Get.ElementPlaceEffects[(int)element],
            position + Vector3.up * 5,
            Quaternion.identity);
        U.Destroy(placeEffect, placeEffect.GetComponent<ParticleSystem>().main.duration);
    }

    public static SpiritSystem CreateSpirit(SpiritData data, Cell cell, bool isOwnedByPlayer)
    {
        var newSpiritPrefab = U.Instantiate(
            data.Prefab,
            cell.transform.position,
            Quaternion.identity,
            ReferenceHolder.Get.SpiritParent);

        var newSpirit = new SpiritSystem(newSpiritPrefab, isOwnedByPlayer)
        {
            Data = data,
            UsedCell = cell.gameObject
        };

        newSpirit.SetSystem(ReferenceHolder.Get.Player);
        CreatePlaceEffect(newSpirit.Data.Base.Element, newSpiritPrefab.transform.position);

        return newSpirit;
    }

    public static SpiritSystem CreateSpirit(SpiritData data, Vector3 position, bool isOwnedByPlayer = true)
    {
        var newSpiritPrefab = U.Instantiate(data.Prefab, position, Quaternion.identity, ReferenceHolder.Get.SpiritParent);
        var newSpirit = new SpiritSystem(newSpiritPrefab, isOwnedByPlayer)
        {
            Data = data,
            UsedCell = null
        };

        newSpirit.SetSystem(ReferenceHolder.Get.Player);
        CreatePlaceEffect(newSpirit.Data.Base.Element, newSpiritPrefab.transform.position);
        return newSpirit;
    }


    public static EnemySystem CreateEnemy(EnemyData data, Vector3 position, Vector3[] waypoints, bool isOwnedByPlayer = true, GameObject prefab = null)
    {

        var enemy = prefab ?? U.Instantiate(data.Prefab, position, Quaternion.identity, ReferenceHolder.Get.EnemyParent);
        var enemySystem = new EnemySystem(enemy, waypoints, isOwnedByPlayer) { Data = data };

        enemySystem.SetSystem(ReferenceHolder.Get.Player);

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
