using IPA;
using HarmonyLib;
using SiraUtil.Zenject;
using System.Reflection;
using IPA.Config.Stores;
using SiraLocalizer.Installers;
using Conf = IPA.Config.Config;
using IPALogger = IPA.Logging.Logger;
using System;
using System.Linq;
using Polyglot;
using System.IO;
using IPA.Utilities;
using System.Text;

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

            if (Environment.GetCommandLineArgs().Contains("--dump-localization")) DumpBaseGameLocalization();
            if (Environment.GetCommandLineArgs().Contains("--dump-keys")) DumpBaseGameKeys();

            zenjector.OnApp<SiraLocalizerCoreInstaller>().WithParameters(conf.Generated<Config>());
            zenjector.OnMenu<SiraLocalizerUIInstaller>();
            zenjector.Register<SiraLocalizerGameplayInstaller>().On<GameplayCoreInstaller>().Expose<MissedNoteEffectSpawner>();
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

        private void DumpBaseGameLocalization()
        {
            LocalizationAsset baseAsset = Localization.Instance.InputFiles.FirstOrDefault();

            if (baseAsset == null)
            {
                Log.Error("Could not dump base game localization: no input files found!");
                return;
            }

            string filePath = Path.Combine(UnityGame.InstallPath, baseAsset.TextAsset.name + ".csv");

            try
            {
                File.WriteAllText(filePath, baseAsset.TextAsset.text, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Log.Error("Could not dump base game localization: " + ex);
            }
        }

        private void DumpBaseGameKeys()
        {
            string filePath = Path.Combine(UnityGame.InstallPath, "keys.txt");

            try
            {
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    foreach (string key in Localization.GetKeys())
                    {
                        writer.WriteLine(key);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("Could not dump base game localization: " + ex);
            }
        }
    }
}