using IPA.Loader;
using IPA.Loader.Features;
using IPA.Logging;
using Newtonsoft.Json.Linq;
using Polyglot;
using SiraLocalizer.Records;
using SiraLocalizer.Utilities;
using System.IO;
using System.Text.RegularExpressions;

namespace SiraLocalizer.Features
{
    public class LocalizedPluginFeature : Feature
    {
        internal static Logger logger;

        private static readonly Regex kValidIdRegex = new(@"^[a-z_-]+$");

        private LocalizedPlugin _localizedPlugin;

        protected override bool Initialize(PluginMetadata pluginMetadata, JObject featureData)
        {
            _localizedPlugin = featureData.ToObject<LocalizedPlugin>();

            if (!kValidIdRegex.IsMatch(_localizedPlugin.id))
            {
                logger.Error($"Invalid localized plugin ID for plugin '{pluginMetadata.Name}': '{_localizedPlugin.id}'");
                return false;
            }

            if (LocalizationDefinition.IsDefinitionLoaded(_localizedPlugin.id))
            {
                logger.Error($"Plugin '{pluginMetadata.Name}' attempted to register duplicate localized plugin ID '{_localizedPlugin.id}'");
                return false;
            }

            if (string.IsNullOrWhiteSpace(_localizedPlugin.name))
            {
                _localizedPlugin.name = pluginMetadata.Name;
            }

            string resourcePath = _localizedPlugin.resourcePath;

            if (string.IsNullOrWhiteSpace(resourcePath))
            {
                logger.Error($"Missing resource path for '{_localizedPlugin.id}'");
                return false;
            }

            logger.Info($"Localized plugin registered: '{_localizedPlugin.name}' ({_localizedPlugin.id})");

            return true;
        }

        public override void AfterInit(PluginMetadata pluginMetadata)
        {
            string resourcePath = _localizedPlugin.resourcePath;
            Stream resourceStream = pluginMetadata.Assembly.GetManifestResourceStream(resourcePath);

            if (resourceStream == null)
            {
                logger.Error($"Resource '{_localizedPlugin.resourcePath}' does not exist in assembly '{pluginMetadata.Assembly.FullName}'");
                return;
            }

            using (var reader = new StreamReader(resourceStream))
            {
                string extension = resourcePath.Substring(resourcePath.LastIndexOf('.'));
                LocalizationDefinition.Add("plugins/" + _localizedPlugin.id, _localizedPlugin.name, PolyglotUtil.GetKeysFromLocalizationAsset(reader.ReadToEnd(), extension == ".tsv" ? GoogleDriveDownloadFormat.TSV : GoogleDriveDownloadFormat.CSV));
            }
        }
    }
}
