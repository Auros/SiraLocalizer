using IPA.Utilities;
using Polyglot;
using System;
using System.IO;
using Zenject;

namespace SiraLocalizer.UI
{
    internal class LanguageManager : IInitializable, IDisposable, ILocalize
    {
        private static readonly string kSettingsFilePath = Path.Combine(UnityGame.UserDataPath, "language");

        public Language selectedLanguage;

        public void Initialize()
        {
            Localization.Instance.AddOnLocalizeEvent(this);

            LoadLanguageFromFile();
        }

        public void Dispose()
        {
            Localization.Instance.RemoveOnLocalizeEvent(this);

            SaveLanguageToFile();
        }

        public void OnLocalize()
        {
            // enforce our language selection
            if (Localization.Instance.SelectedLanguage != selectedLanguage)
            {
                Plugin.Log.Trace("Enforcing language " + selectedLanguage);
                Localization.Instance.SelectLanguage(selectedLanguage);
            }
        }

        private void LoadLanguageFromFile()
        {
            if (!File.Exists(kSettingsFilePath)) return;

            try
            {
                using (var reader = new StreamReader(kSettingsFilePath))
                {
                    if (!Enum.TryParse(reader.ReadToEnd(), out Language language)) return;
                    if (!Localization.Instance.SupportedLanguages.Contains(selectedLanguage)) return;

                    selectedLanguage = language;

                    Plugin.Log.Debug("Set language to " + selectedLanguage);
                    Localization.Instance.SelectLanguage(selectedLanguage);
                }
            }
            catch (IOException ex)
            {
                Plugin.Log.Error("Failed to load language from settings");
                Plugin.Log.Error(ex);
            }
        }

        private void SaveLanguageToFile()
        {
            try
            {
                using (var writer = new StreamWriter(kSettingsFilePath))
                {
                    writer.Write(selectedLanguage.ToString());
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.Error("Failed to save language to settings");
                Plugin.Log.Error(ex);
            }
        }
    }
}
