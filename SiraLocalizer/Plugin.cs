using HarmonyLib;
using IPA;
using SiraUtil.Zenject;
using SiraLocalizer.Installers;
using System.Reflection;
using IPALogger = IPA.Logging.Logger;

namespace SiraLocalizer
{
    [Plugin(RuntimeOptions.DynamicInit)]
    public class Plugin
    {
        private const string kHarmonyId = "dev.auros.siralocalizer";

        internal static IPALogger Log { get; private set; }

        private readonly Harmony _harmony;

        [Init]
        public Plugin(IPALogger logger, Zenjector zenjector)
        {
            Log = logger;

            _harmony = new Harmony(kHarmonyId);

            zenjector.OnApp<SiraLocalizerCoreInstaller>();
            zenjector.OnMenu<SiraLocalizerUIInstaller>();
        }

        [OnEnable]
        public void OnEnable()
        {
            _harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        [OnDisable]
        public void OnDisable()
        {
            _harmony.UnpatchAll(kHarmonyId);
        }
    }
}