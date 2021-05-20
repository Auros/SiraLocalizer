using IPA;
using System;
using HarmonyLib;
using System.Linq;
using SiraUtil.Zenject;
using System.Reflection;
using IPA.Config.Stores;
using SiraLocalizer.Installers;
using Conf = IPA.Config.Config;
using IPALogger = IPA.Logging.Logger;
using Polyglot;

namespace SiraLocalizer
{
    [Plugin(RuntimeOptions.DynamicInit)]
    public class Plugin
    {
        private const string kHarmonyId = "pro.sira.siralocalizer";

        internal static IPALogger log { get; private set; }

        private readonly Harmony _harmony;

        [Init]
        public Plugin(Conf conf, IPALogger logger, Zenjector zenjector)
        {
            log = logger;

            if (Environment.GetCommandLineArgs().Contains("--dump-localization")) LocalizationExporter.DumpBaseGameLocalization();

            _harmony = new Harmony(kHarmonyId);

            LocalizationDefinition.Add(new LocalizationDefinition("beat-saber", "Beat Saber", PolyglotUtil.GetKeysFromLocalizationAsset(Localization.Instance.InputFiles[0])));

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