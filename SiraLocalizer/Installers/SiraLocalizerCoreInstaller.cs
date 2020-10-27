using Zenject;
using SiraUtil.Interfaces;

namespace SiraLocalizer.Installers
{
    internal class SiraLocalizerCoreInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.Bind<ILocalizer>().WithId("SIRA.Localizer").To<Localizer>().AsSingle();
            Container.Bind<IInitializable>().To<CustomLocaleLoader>().AsSingle();
        }
    }
}