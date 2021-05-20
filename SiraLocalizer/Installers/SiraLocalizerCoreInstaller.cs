using SiraLocalizer.Crowdin;
using SiraLocalizer.Providers;
using SiraLocalizer.UI;
using SiraUtil.Interfaces;
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

            Container.BindInterfacesAndSelfTo<Localizer>().AsSingle();
            Container.BindInterfacesAndSelfTo<FontLoader>().AsSingle();
            Container.Bind<IInitializable>().To<UserLocalizationFileLoader>().AsSingle();

            Container.Bind(typeof(IModelProvider), typeof(ItalicizedFlyingTextEffectModelProvider)).To<ItalicizedFlyingTextEffectModelProvider>().AsSingle();

            Container.Bind<FontAssetHelper>().AsTransient();

            if (_config.automaticallyDownloadLocalizations)
            {
                Container.Bind<IInitializable>().To<CrowdinDownloader>().AsSingle();
            }
        }
    }
}
