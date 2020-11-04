using Zenject;
using SiraLocalizer.UI;
using SiraUtil.Interfaces;

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

            Container.Bind<ILocalizer>().WithId("SIRA.Localizer").To<Localizer>().AsSingle();
            Container.Bind<IInitializable>().To<CustomLocaleLoader>().AsSingle();

            Container.BindInterfacesAndSelfTo<LanguageManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<FontLoader>().AsSingle();
        }
    }
}