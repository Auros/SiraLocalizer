using System.Collections.Generic;
using System.Reflection;
using BGLib.Polyglot;
using HarmonyLib;

namespace SiraLocalizer.HarmonyPatches
{
    [HarmonyPatch(typeof(LanguageExtensions))]
    internal static class LocalizationExtensions_ToSerializedName
    {
        public static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.DeclaredMethod(typeof(LanguageExtensions), nameof(LanguageExtensions.ToSerializedName));
            yield return AccessTools.DeclaredMethod(typeof(LanguageExtensions), nameof(LanguageExtensions.ToCultureInfoName));
        }

        public static bool Prefix(Language lang, ref string __result)
        {
            __result = GetIetfLanguageCode((Locale)lang);
            return false;
        }

        // see https://en.wikipedia.org/wiki/IETF_language_tag
        // and https://learn.microsoft.com/en-us/openspecs/windows_protocols/ms-lcid/a9eac961-e77d-41a6-90a5-ce1a8b0cdb9c
        private static string GetIetfLanguageCode(Locale locale)
        {
            return locale switch
            {
                Locale.English => "en",
                Locale.French => "fr",
                Locale.Spanish => "es",
                Locale.German => "de",
                Locale.Italian => "it",
                Locale.Portuguese_Brazil => "pt-BR",
                Locale.Portuguese => "pt",
                Locale.Russian => "ru",
                Locale.Greek => "el",
                Locale.Turkish => "tr",
                Locale.Danish => "da",
                Locale.Norwegian => "no",
                Locale.Swedish => "sv",
                Locale.Dutch => "nl",
                Locale.Polish => "pl",
                Locale.Finnish => "fi",
                Locale.Japanese => "ja",
                Locale.SimplifiedChinese => "zh-Hans",
                Locale.TraditionalChinese => "zh-Hant",
                Locale.Korean => "ko",
                Locale.Czech => "cs",
                Locale.Hungarian => "hu",
                Locale.Romanian => "ro",
                Locale.Thai => "th",
                Locale.Bulgarian => "bg",
                Locale.Hebrew => "he",
                Locale.Arabic => "ar",
                Locale.Bosnian => "bs",
                Locale.Icelandic => "is",
                Locale.Irish => "ga",
                _ => "en",
            };
        }
    }

    [HarmonyPatch(typeof(LanguageExtensions), nameof(LanguageExtensions.ToLanguage))]
    internal static class LocalizationExtensions_ToLanguage
    {
        public static bool Prefix(string serializedName, ref Language __result)
        {
            __result = (Language)FromIetfLanguageCode(serializedName);
            return false;
        }

        // see https://en.wikipedia.org/wiki/IETF_language_tag
        private static Locale FromIetfLanguageCode(string serializedName)
        {
            // settings are stored lowercase for some godforsaken reason
            return serializedName.ToLowerInvariant() switch
            {
                "en" => Locale.English,
                "fr" => Locale.French,
                "es" => Locale.Spanish,
                "de" => Locale.German,
                "it" => Locale.Italian,
                "pt-br" => Locale.Portuguese_Brazil,
                "pt" => Locale.Portuguese,
                "ru" => Locale.Russian,
                "el" => Locale.Greek,
                "tr" => Locale.Turkish,
                "da" => Locale.Danish,
                "no" => Locale.Norwegian,
                "sv" => Locale.Swedish,
                "nl" => Locale.Dutch,
                "pl" => Locale.Polish,
                "fi" => Locale.Finnish,
                "ja" => Locale.Japanese,
                "zh-hans" => Locale.SimplifiedChinese,
                "zh-hant" => Locale.TraditionalChinese,
                "ko" => Locale.Korean,
                "cs" => Locale.Czech,
                "hu" => Locale.Hungarian,
                "ro" => Locale.Romanian,
                "th" => Locale.Thai,
                "bg" => Locale.Bulgarian,
                "he" => Locale.Hebrew,
                "ar" => Locale.Arabic,
                "bs" => Locale.Bosnian,
                "is" => Locale.Icelandic,
                "ga" => Locale.Irish,
                _ => Locale.English,
            };
        }
    }
}
