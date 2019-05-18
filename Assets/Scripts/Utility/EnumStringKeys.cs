using System;
using System.Collections.Generic;
using Game.Data.Enemy.Internal;
using Game.Enums;

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

        static Dictionary<EnumType, string> CreateStringKeyDictionary<EnumType>() where EnumType : Enum
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

        public static string GetStringKey<EnumType>(this EnumType type) where EnumType : struct, Enum =>
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