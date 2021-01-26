using Zenject;
using SiraLocalizer.UI;
using SiraUtil.Interfaces;
using SiraLocalizer.Providers;

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

            Container.BindInterfacesAndSelfTo<LanguageEnforcer>().AsSingle();
            Container.BindInterfacesAndSelfTo<FontLoader>().AsSingle();

            Container.Bind(typeof(IModelProvider), typeof(FlyingTextYoinkerProvider)).To<FlyingTextYoinkerProvider>().AsSingle();
        }
    }
}