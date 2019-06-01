using System;
using System.Collections.Generic;
using Game.Data.EnemyEntity.Internal;
using Game.Enums;
using Game.Utility.Localization;

namespace Game.Utility
{
    public static class EnumStringKeys
    {
        public static readonly Dictionary<Numeral, string> Numeral = CreateStringKeyDictionary<Numeral>();
        public static readonly Dictionary<Game.Enums.Spirit, string> Spirit = CreateStringKeyDictionary<Game.Enums.Spirit>();
        public static readonly Dictionary<Game.Enums.SpiritFlag, string> SpiritFlag = CreateStringKeyDictionary<Game.Enums.SpiritFlag>();
        public static readonly Dictionary<Game.Enums.Enemy, string> Enemy = CreateStringKeyDictionary<Game.Enums.Enemy>();
        public static readonly Dictionary<RarityType, string> Rarity = CreateStringKeyDictionary<RarityType>();
        public static readonly Dictionary<ElementType, string> Element = CreateStringKeyDictionary<ElementType>();
        public static readonly Dictionary<EnemyType, string> EnemyType = CreateStringKeyDictionary<EnemyType>();
        public static readonly Dictionary<RaceType, string> Race = CreateStringKeyDictionary<RaceType>();
        public static readonly Dictionary<ArmorType, string> Armor = CreateStringKeyDictionary<ArmorType>();

        static Dictionary<T, string> CreateStringKeyDictionary<T>() where T : Enum
        {
            var enums = Enum.GetValues(typeof(T));
            var stringKeyDictionary = new Dictionary<T, string>();
            var keyPrefix =
                typeof(T) == typeof(Numeral) ? "a" :
                typeof(T) == typeof(Game.Enums.Spirit) ? "a" :
                typeof(T) == typeof(Game.Enums.SpiritFlag) ? "a" :
                typeof(T) == typeof(Game.Enums.Enemy) ? "a" :
                typeof(T) == typeof(ElementType) ? "element" :
                typeof(T) == typeof(RarityType) ? "rarity" :
                typeof(T) == typeof(ArmorType) ? "e-armor" :
                typeof(T) == typeof(EnemyType) ? "e-type" :
                typeof(T) == typeof(RaceType) ? "e-race" :
                "error";

            for (int i = 0; i < enums.Length; i++)
            {
                var enumKey = (T)enums.GetValue(i);
                var stringValue = Enum.GetName(typeof(T), i).StringEnumToStringKey(keyPrefix);

                stringKeyDictionary.Add(enumKey, stringValue);
            }
            return stringKeyDictionary;
        }

        public static string GetStringKey<T>(this T type) where T : struct, Enum =>
                type is Numeral numeral ? EnumStringKeys.Numeral[numeral] :
                type is Game.Enums.Spirit spirit ? EnumStringKeys.Spirit[spirit] :
                type is Game.Enums.SpiritFlag spiritFlag ? EnumStringKeys.SpiritFlag[spiritFlag] :
                type is Game.Enums.Enemy enemy ? EnumStringKeys.Enemy[enemy] :
                type is RarityType rarity ? EnumStringKeys.Rarity[rarity] :
                type is ArmorType armor ? EnumStringKeys.Armor[armor] :
                type is EnemyType enemyType ? EnumStringKeys.EnemyType[enemyType] :
                type is RaceType race ? EnumStringKeys.Race[race] :
                type is ElementType element ? EnumStringKeys.Element[element] : "error";
    }
}