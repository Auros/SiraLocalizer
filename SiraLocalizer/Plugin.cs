using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using IPA;
using IPA.Config.Stores;
using IPA.Logging;
using Polyglot;
using SiraLocalizer.Installers;
using SiraUtil.Zenject;
using Conf = IPA.Config.Config;

namespace SiraLocalizer
{
    [Plugin(RuntimeOptions.DynamicInit)]
    public class Plugin
    {
        private const string kHarmonyId = "pro.sira.siralocalizer";

        private readonly Harmony _harmony;

        [Init]
        public Plugin(Conf conf, Logger logger, Zenjector zenjector)
        {
            if (Environment.GetCommandLineArgs().Contains("--dump-localization")) new LocalizationExporter(logger).DumpBaseGameLocalization();

            _harmony = new Harmony(kHarmonyId);

            LocalizationDefinition.Add(new LocalizationDefinition("beat-saber", "Beat Saber", PolyglotUtil.GetKeysFromLocalizationAsset(Localization.Instance.InputFiles[0])));

            zenjector.UseLogger(logger);
            zenjector.Install<SiraLocalizerCoreInstaller>(Location.App, conf.Generated<Config>());
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
