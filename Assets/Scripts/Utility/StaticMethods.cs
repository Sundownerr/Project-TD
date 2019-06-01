using Game.Systems;
using System;
using System.Threading;
using UnityEngine;
using U = UnityEngine.Object;
using Game.Enums;
using Game.Managers;
using NetworkPlayer = Game.Systems.Network.NetworkPlayer;
using Game.Systems.Spirit;
using Game.Systems.Enemy;
using Game.Data.EnemyEntity;
using Game.Data.SpiritEntity;
using Game.Systems.Cells;

namespace Game.Utility
{
    public static class Uty
    {
        public static string KiloFormat(double num)
        {
            if (num >= 1000000000) return (num / 1000000000).ToString("#.0" + "B");
            if (num >= 1000000) return (num / 1000000).ToString("#" + "M");
            if (num >= 100000) return (num / 1000).ToString("#.0" + "K");
            if (num >= 10000) return (num / 1000).ToString("0.#" + "K");
            if (num >= 1000) return (num / 1000).ToString("0.#" + "K");

            return num.ToString("0.#");
        }

        public static string AttributeToString(this Numeral attribute)
        {
            var spirit = GameData.Instance.Player.PlayerInputSystem.ChoosedSpirit.Data;

            var withPercent =
                attribute == Numeral.ItemDropRate ||
                attribute == Numeral.ItemQualityRate ||
                attribute == Numeral.BuffTime ||
                attribute == Numeral.DebuffTime;

            if (attribute == Numeral.Level)
                return $"{Uty.KiloFormat((int)spirit.Get(attribute).Sum)}";

            return $"{Uty.KiloFormat(spirit.Get(attribute).Sum)}{(withPercent ? "%" : string.Empty)}";
        }

        public static string AttributeToString(this Enums.Spirit attribute)
        {
            var spirit = GameData.Instance.Player.PlayerInputSystem.ChoosedSpirit.Data;

            var withPercent =
                attribute == Enums.Spirit.SpellCritChance ||
                attribute == Enums.Spirit.SpellDamage ||
                attribute == Enums.Spirit.CritChance ||
                attribute == Enums.Spirit.ExpRate ||
                attribute == Enums.Spirit.ResourceRate ||
                attribute == Enums.Spirit.CritMultiplier;

            return $"{Uty.KiloFormat(spirit.Get(attribute).Sum)}{(withPercent ? "%" : string.Empty)}";
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
}