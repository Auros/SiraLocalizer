using IPA.Loader;
using IPA.Loader.Features;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SiraLocalizer.Features
{
    public class LocalizedPluginFeature : Feature
    {
        internal static readonly List<string> localizedPlugins = new List<string>();

        private static readonly Regex kValidIdRegex = new Regex(@"^[a-z_-]+$");

        public static bool IsLocalizedPluginLoaded(string id)
        {
            return localizedPlugins.Contains(id);
        }

        protected override bool Initialize(PluginMetadata meta, JObject featureData)
        {
            LocalizedPlugin localizedPlugin = featureData.ToObject<LocalizedPlugin>();

            if (!kValidIdRegex.IsMatch(localizedPlugin.id))
            {
                Plugin.Log.Info($"Invalid localized plugin ID for plugin '{meta.Name}': '{localizedPlugin.id}'");
                return false;
            }

            localizedPlugins.Add(localizedPlugin.id);

            Plugin.Log.Info($"Localized plugin registered: '{meta.Name}' ({localizedPlugin.id})");

            return true;
        }
    }
}
