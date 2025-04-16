using Zenject;
using SiraLocalizer.UI;

namespace SiraLocalizer.Installers
{
    internal class SiraLocalizerUIInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<LanguageSettingCreator>().AsSingle();
        }
    }
}
