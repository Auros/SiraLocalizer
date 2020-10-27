using IPA;
using SiraUtil.Zenject;
using SiraLocalizer.Installers;
using IPALogger = IPA.Logging.Logger;

namespace SiraLocalizer
{
    [Plugin(RuntimeOptions.DynamicInit)]
    public class Plugin
    {
        internal static IPALogger Log { get; private set; }

        [Init]
        public Plugin(IPALogger logger, Zenjector zenjector)
        {
            Log = logger;
            zenjector.OnApp<SiraLocalizerCoreInstaller>();
        }

        [OnEnable, OnDisable]
        public void OnState() { }
    }
}