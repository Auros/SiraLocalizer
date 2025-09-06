using System;
using System.Linq;
using SiraLocalizer.Providers;
using SiraLocalizer.Providers.Crowdin;
using SiraLocalizer.Providers.CrowdinApi;
using SiraLocalizer.UI;
using SiraLocalizer.Utilities;
using SiraLocalizer.Utilities.WebRequests;
using SiraUtil.Affinity;
using Zenject;

namespace SiraLocalizer.Installers
{
    internal class SiraLocalizerCoreInstaller : Installer<Settings, SiraLocalizerCoreInstaller>
    {
        private readonly Settings _config;

        internal SiraLocalizerCoreInstaller(Settings config)
        {
            _config = config;
        }

        public override void InstallBindings()
        {
            Container.BindInstance(_config).AsSingle();

            Container.Bind(typeof(LocalizationManager), typeof(IAffinity), typeof(IInitializable), typeof(IDisposable)).To<LocalizationManager>().AsSingle();
            Container.Bind(typeof(FontLoader), typeof(IInitializable), typeof(IDisposable)).To<FontLoader>().AsSingle();

            if (!string.IsNullOrWhiteSpace(_config.crowdinAccessToken))
            {
                Container.Bind(typeof(ILocalizationProvider), typeof(ILocalizationDownloader)).To<CrowdinApiDownloader>().AsSingle();
            }
            else
            {
                Container.Bind(typeof(ILocalizationProvider), typeof(ILocalizationDownloader)).To<CrowdinDownloader>().AsSingle();
            }

            Container.Bind(typeof(ILocalizationProvider)).To<ResourceLocalizationProvider>().AsSingle();
            Container.Bind(typeof(ILocalizationProvider)).To<UserLocalizationFileProvider>().AsSingle();
            Container.Bind<UnityWebRequestHelper>().AsSingle();

            if (Environment.GetCommandLineArgs().Contains("--dump-localization"))
            {
                Container.Bind<IInitializable>().To<LocalizationExporter>().AsSingle();
            }
        }
    }
}
