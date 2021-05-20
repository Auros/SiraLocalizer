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

        private LocalizedPlugin _localizedPlugin;

        protected override bool Initialize(PluginMetadata pluginMetadata, JObject featureData)
        {
            _localizedPlugin = featureData.ToObject<LocalizedPlugin>();

            if (!kValidIdRegex.IsMatch(_localizedPlugin.id))
            {
                Plugin.log.Error($"Invalid localized plugin ID for plugin '{pluginMetadata.Name}': '{_localizedPlugin.id}'");
                return false;
            }

            if (LocalizationDefinition.IsDefinitionLoaded(_localizedPlugin.id))
            {
                Plugin.log.Error($"Plugin '{pluginMetadata.Name}' attempted to register duplicate localized plugin ID '{_localizedPlugin.id}'");
                return false;
            }

            if (string.IsNullOrWhiteSpace(_localizedPlugin.name))
            {
                _localizedPlugin.name = pluginMetadata.Name;
            }

            string resourcePath = _localizedPlugin.resourcePath;

            if (string.IsNullOrWhiteSpace(resourcePath))
            {
                Plugin.log.Error($"Missing resource path for '{_localizedPlugin.id}'");
                return false;
            }

            Plugin.log.Info($"Localized plugin registered: '{_localizedPlugin.name}' ({_localizedPlugin.id})");

            return true;
        }

        public override void AfterInit(PluginMetadata pluginMetadata)
        {
            string resourcePath = _localizedPlugin.resourcePath;
            Stream resourceStream = pluginMetadata.Assembly.GetManifestResourceStream(resourcePath);

            if (resourceStream == null)
            {
                Plugin.log.Error($"Resource '{_localizedPlugin.resourcePath}' does not exist in assembly '{pluginMetadata.Assembly.FullName}'");
                return;
            }

            using (var reader = new StreamReader(resourceStream))
            {
                string extension = resourcePath.Substring(resourcePath.LastIndexOf('.'));
                LocalizationDefinition.Add(new LocalizationDefinition("plugins/" + _localizedPlugin.id, _localizedPlugin.name, PolyglotUtil.GetKeysFromLocalizationAsset(reader.ReadToEnd(), extension == ".tsv" ? GoogleDriveDownloadFormat.TSV : GoogleDriveDownloadFormat.CSV)));
            }
        }
    }
}
