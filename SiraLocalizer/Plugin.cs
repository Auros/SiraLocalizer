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
using System.Collections.Generic;

namespace SiraLocalizer
{
    [Plugin(RuntimeOptions.DynamicInit)]
    public class Plugin
    {
        private const string kHarmonyId = "pro.sira.siralocalizer";

        // keys that aren't actually used
        private static readonly string[] kLocalizationKeyIgnoreList = { "MP_MISSING_SONG_ENTITLEMENT", "MODIFIER_PRO_MODE", "MODIFIER_PRO_MODE_HINT", "LANGUAGE_EN", "LANGUAGE_SC" };

        internal static IPALogger Log { get; private set; }

        private readonly Harmony _harmony;

        [Init]
        public Plugin(Conf conf, IPALogger logger, Zenjector zenjector)
        {
            Log = logger;

            _harmony = new Harmony(kHarmonyId);

            if (Environment.GetCommandLineArgs().Contains("--dump-localization")) DumpBaseGameLocalization();

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
            string filePath = Path.Combine(UnityGame.InstallPath, "localization.csv");

            try
            {
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    LocalizationAsset baseGameAsset = Localization.Instance.InputFiles.First();
                    List<List<string>> rows = CsvReader.Parse(baseGameAsset.TextAsset.text);

                    writer.WriteLine("Polyglot,100,,,,,,,,,,,,,,,,,,,,,,,,,,,,");

                    foreach (List<string> row in rows.SkipWhile(r => r[0] != "Polyglot").Skip(1))
                    {
                        if (string.IsNullOrEmpty(row[0]) || kLocalizationKeyIgnoreList.Contains(row[0])) continue;

                        string key     = EscapeCsvValue(row.ElementAtOrDefault(0));
                        string context = EscapeCsvValue(row.ElementAtOrDefault(1));
                        string english = EscapeCsvValue(row.ElementAtOrDefault(2));

                        writer.WriteLine($"{key},{context},{english},,,,,,,,,,,,,,,,,,,,,,,,,,,");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("Could not dump base game localization: " + ex);
            }
        }

        private string EscapeCsvValue(string value)
        {
            if (string.IsNullOrEmpty(value)) return null;
            if (!value.Contains(',') && !value.Contains('"') && !value.Contains('\n')) return value;

            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }
    }
}