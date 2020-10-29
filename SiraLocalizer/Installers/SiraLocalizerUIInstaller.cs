using SiraLocalizer.UI;
using Zenject;

namespace SiraLocalizer.Installers
{
    internal class SiraLocalizerUIInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<LanguageSettingCreator>().AsSingle().NonLazy();
        }
    }
}
