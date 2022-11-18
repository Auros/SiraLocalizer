using System;
using SiraLocalizer.Crowdin;
using SiraLocalizer.UI;
using SiraUtil.Affinity;
using Zenject;

namespace SiraLocalizer.Installers
{
    internal class SiraLocalizerCoreInstaller : Installer<Config, SiraLocalizerCoreInstaller>
    {
        private readonly Config _config;

        internal SiraLocalizerCoreInstaller(Config config)
        {
            _config = config;
        }

        public override void InstallBindings()
        {
            Container.BindInstance(_config).AsSingle();

            Container.Bind(typeof(Localizer), typeof(IAffinity), typeof(ILocalizer), typeof(IInitializable), typeof(IDisposable)).To<Localizer>().AsSingle();
            Container.Bind(typeof(IInitializable), typeof(IDisposable)).To<FontLoader>().AsSingle();
            Container.Bind<IInitializable>().To<UserLocalizationFileLoader>().AsSingle();
            Container.Bind<FontAssetHelper>().AsTransient();
            Container.Bind(typeof(CrowdinDownloader), typeof(IInitializable), typeof(IDisposable)).To<CrowdinDownloader>().AsSingle();
        }
    }
}
