using System.Reflection;
using HarmonyLib;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using IPA.Logging;
using SiraLocalizer.Features;
using SiraLocalizer.Installers;
using SiraUtil.Zenject;

namespace SiraLocalizer
{
    [Plugin(RuntimeOptions.DynamicInit)]
    internal class Plugin
    {
        private const string kHarmonyId = "pro.sira.siralocalizer";

        private readonly Harmony _harmony;

        [Init]
        public Plugin(Config conf, Logger logger, Zenjector zenjector)
        {
            _harmony = new Harmony(kHarmonyId);

            LocalizedPluginFeature.logger = logger;

            zenjector.UseLogger(logger);
            zenjector.Install<SiraLocalizerCoreInstaller>(Location.App, conf.Generated<Settings>());
            zenjector.Install<SiraLocalizerUIInstaller>(Location.Menu);
        }

        [OnEnable]
        public void OnEnable()
        {
            _harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        [OnDisable]
        public void OnDisable()
        {
            _harmony.UnpatchSelf();
        }
    }
}
