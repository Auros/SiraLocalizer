using System.Collections.Generic;

namespace SiraLocalizer
{
    internal class LocalizationDefinition
    {
        public static IReadOnlyCollection<LocalizationDefinition> loadedDefinitions => kLoadedDefinitions.Values;

        private static readonly Dictionary<string, LocalizationDefinition> kLoadedDefinitions = new();

        public string id { get; }
        public string name { get; }
        public IEnumerable<string> keys { get; }

        public LocalizationDefinition(string id, string name, IEnumerable<string> keys)
        {
            this.id = id;
            this.name = name;
            this.keys = keys;
        }

        public static void Add(LocalizationDefinition definition)
        {
            kLoadedDefinitions.Add(definition.id, definition);
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
