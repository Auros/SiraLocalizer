using System.Collections.Generic;
using Polyglot;
using SiraLocalizer.Utilities;

namespace SiraLocalizer.Records
{
    internal record LocalizationDefinition(string id, string name, IEnumerable<string> keys)
    {
        public static IReadOnlyCollection<LocalizationDefinition> loadedDefinitions => kLoadedDefinitions.Values;

        private static readonly Dictionary<string, LocalizationDefinition> kLoadedDefinitions = new();

        public static void Add(string id, string name, LocalizationAsset localizationAsset)
        {
            Add(id, name, PolyglotUtil.GetKeysFromLocalizationAsset(localizationAsset));
        }

        public static void Add(string id, string name, IEnumerable<string> keys)
        {
            kLoadedDefinitions.Add(id, new LocalizationDefinition(id, name, keys));
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
