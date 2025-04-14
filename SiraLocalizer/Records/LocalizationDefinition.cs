using System.Collections.Generic;
using System.Linq;
using SiraLocalizer.Utilities;
using UnityEngine;

namespace SiraLocalizer.Records
{
    internal record LocalizationDefinition(string id, string name, IReadOnlyList<string> keys)
    {
        public static IReadOnlyCollection<LocalizationDefinition> loadedDefinitions => kLoadedDefinitions.Values;

        private static readonly Dictionary<string, LocalizationDefinition> kLoadedDefinitions = [];

        public static void Add(string id, string name, IEnumerable<TextAsset> textAssets)
        {
            Add(id, name, PolyglotUtil.GetKeysFromLocalizationAsset(textAssets));
        }

        public static void Add(string id, string name, IEnumerable<string> keys)
        {
            kLoadedDefinitions.Add(id, new LocalizationDefinition(id, name, keys.ToArray()));
        }

        public static bool Remove(string id)
        {
            return kLoadedDefinitions.Remove(id);
        }

        public static bool IsDefinitionLoaded(string id)
        {
            return kLoadedDefinitions.ContainsKey(id);
        }

        public static bool TryGetLoadedDefinition(string id, out LocalizationDefinition localizedPlugin)
        {
            return kLoadedDefinitions.TryGetValue(id, out localizedPlugin);
        }
    }
}
