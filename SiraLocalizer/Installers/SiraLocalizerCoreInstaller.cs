using SiraLocalizer.Crowdin;
using SiraLocalizer.UI;
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
            Container.Bind<FontAssetHelper>().AsTransient();
            Container.BindInterfacesAndSelfTo<CrowdinDownloader>().AsSingle();
        }
    }
}
