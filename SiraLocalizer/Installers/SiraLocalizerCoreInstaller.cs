using Zenject;
using SiraLocalizer.UI;
using SiraUtil.Interfaces;
using SiraLocalizer.Providers;
using SiraLocalizer.Crowdin;

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

            Container.Bind<Localizer>().AsSingle();
            Container.Bind<IInitializable>().To<UserLocalizationFileLoader>().AsSingle();

            Container.BindInterfacesAndSelfTo<FontLoader>().AsSingle();

            Container.Bind(typeof(IModelProvider), typeof(ItalicizedFlyingTextEffectModelProvider)).To<ItalicizedFlyingTextEffectModelProvider>().AsSingle();

            Container.Bind<FontAssetHelper>().AsTransient();

            if (_config.automaticallyDownloadLocalizations)
            {
                Container.Bind<IInitializable>().To<CrowdinDownloader>().AsSingle();
            }
        }
    }
}