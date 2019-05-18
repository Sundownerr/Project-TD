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
using Game.Data.Enemy;
using Game.Data.Spirit;
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