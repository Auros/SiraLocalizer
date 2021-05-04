using IPA.Loader;
using IPA.Loader.Features;
using Newtonsoft.Json.Linq;
using Polyglot;
using System.IO;
using System.Text.RegularExpressions;

namespace SiraLocalizer.Features
{
    public class LocalizedPluginFeature : Feature
    {
        private static readonly Regex kValidIdRegex = new Regex(@"^[a-z_-]+$");

        private LocalizedPlugin localizedPlugin;

        protected override bool Initialize(PluginMetadata pluginMetadata, JObject featureData)
        {
            localizedPlugin = featureData.ToObject<LocalizedPlugin>();

            if (!kValidIdRegex.IsMatch(localizedPlugin.id))
            {
                Plugin.Log.Error($"Invalid localized plugin ID for plugin '{pluginMetadata.Name}': '{localizedPlugin.id}'");
                return false;
            }

            if (LocalizationDefinition.IsDefinitionLoaded(localizedPlugin.id))
            {
                Plugin.Log.Error($"Plugin '{pluginMetadata.Name}' attempted to register duplicate localized plugin ID '{localizedPlugin.id}'");
                return false;
            }
            
            if (string.IsNullOrWhiteSpace(localizedPlugin.name))
            {
                localizedPlugin.name = pluginMetadata.Name;
            }

            string resourcePath = localizedPlugin.resourcePath;

            if (string.IsNullOrWhiteSpace(resourcePath))
            {
                Plugin.Log.Error($"Missing resource path for '{localizedPlugin.id}'");
                return false;
            }

            Plugin.Log.Info($"Localized plugin registered: '{localizedPlugin.name}' ({localizedPlugin.id})");

            return true;
        }

        public override void AfterInit(PluginMetadata pluginMetadata)
        {
            string resourcePath = localizedPlugin.resourcePath;
            Stream resourceStream = pluginMetadata.Assembly.GetManifestResourceStream(resourcePath);

            if (resourceStream == null)
            {
                Plugin.Log.Error($"Resource '{localizedPlugin.resourcePath}' does not exist in assembly '{pluginMetadata.Assembly.FullName}'");
                return;
            }

            using (StreamReader reader = new StreamReader(resourceStream))
            {
                string extension = resourcePath.Substring(resourcePath.LastIndexOf('.'));
                LocalizationDefinition.Add(new LocalizationDefinition("plugins/" + localizedPlugin.id, localizedPlugin.name, PolyglotUtil.GetKeysFromLocalizationAsset(reader.ReadToEnd(), extension == ".tsv" ? GoogleDriveDownloadFormat.TSV : GoogleDriveDownloadFormat.CSV)));
            }
        }
    }
}
