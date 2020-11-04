using IPA;
using HarmonyLib;
using SiraUtil.Zenject;
using System.Reflection;
using IPA.Config.Stores;
using SiraLocalizer.Installers;
using Conf = IPA.Config.Config;
using IPALogger = IPA.Logging.Logger;

namespace SiraLocalizer
{
    [Plugin(RuntimeOptions.DynamicInit)]
    public class Plugin
    {
        private const string kHarmonyId = "pro.sira.siralocalizer";

        internal static IPALogger Log { get; private set; }

        private readonly Harmony _harmony;

        [Init]
        public Plugin(Conf conf, IPALogger logger, Zenjector zenjector)
        {
            Log = logger;

            _harmony = new Harmony(kHarmonyId);

            zenjector.OnApp<SiraLocalizerCoreInstaller>().WithParameters(conf.Generated<Config>());
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