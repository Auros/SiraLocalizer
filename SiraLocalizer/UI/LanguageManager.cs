using System;
using Zenject;
using Polyglot;

namespace SiraLocalizer.UI
{
    internal class LanguageManager : IInitializable, IDisposable, ILocalize
    {
        public Language selectedLanguage;
        private readonly Config _config;

        internal LanguageManager(Config config)
        {
            _config = config;
        }

        public void Initialize()
        {
            Localization.Instance.AddOnLocalizeEvent(this);

            if (Enum.TryParse(_config.Language, out Language language))
            {
                selectedLanguage = language;
                Localization.Instance.SelectLanguage(language);
            }
        }

        public void Dispose()
        {
            _config.Language = selectedLanguage.ToString();

            Localization.Instance.RemoveOnLocalizeEvent(this);
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
    }
}