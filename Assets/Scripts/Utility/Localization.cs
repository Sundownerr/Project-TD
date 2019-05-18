using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Lean.Localization;
using UnityEngine;

namespace Game.Utility.Localization
{
    public static class LocalizationHelper
    {
        public static string[] SplitCamelCase(this string source) => Regex.Split(source, @"(?<!^)(?=[A-Z])");

        public static string GetLocalized<EnumType>(this EnumType type) where EnumType : struct, Enum =>
            LeanLocalization.GetTranslationText(type.GetStringKey());

        public static string GetLocalized(this string key) =>
            LeanLocalization.GetTranslationText(key);

        public static string StringEnumToStringKey(this string source, string keyPrefix)
        {
            var splitted = source.SplitCamelCase();
            var sb = new StringBuilder().Append(keyPrefix);

            for (int i = 0; i < splitted.Length; i++)
                sb.Append("-").Append(splitted[i].ToLower());

            return sb.ToString();
        }
    }
}