using System;
using SiraLocalizer.Providers;
using SiraLocalizer.Providers.Crowdin;
using SiraLocalizer.UI;
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
            Container.Bind(typeof(IInitializable), typeof(IDisposable)).To<FontLoader>().AsSingle();
            Container.Bind(typeof(ILocalizationProvider), typeof(ILocalizationDownloader)).To<CrowdinDownloader>().AsSingle();
            Container.Bind(typeof(ILocalizationProvider)).To<ResourceLocalizationProvider>().AsSingle();
            Container.Bind(typeof(ILocalizationProvider)).To<UserLocalizationFileProvider>().AsSingle();
            Container.Bind<FontAssetHelper>().AsTransient();
        }
    }
}
