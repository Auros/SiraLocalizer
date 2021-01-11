using System;
using Zenject;
using Polyglot;

namespace SiraLocalizer.UI
{
    internal class LanguageEnforcer : IInitializable, IDisposable, ILocalize
    {
        private readonly Config _config;

        internal LanguageEnforcer(Config config)
        {
            _config = config;
        }

        public void Initialize()
        {
            Localization.Instance.AddOnLocalizeEvent(this);
            Localization.Instance.SelectLanguage(_config.language);
        }

        public void Dispose()
        {
            Localization.Instance.RemoveOnLocalizeEvent(this);
        }

        public void OnLocalize()
        {
            Language wantedLanguage = _config.language;

            // enforce our language selection
            if (Localization.Instance.SelectedLanguage != wantedLanguage)
            {
                Plugin.Log.Trace("Enforcing language " + wantedLanguage);
                Localization.Instance.SelectLanguage(wantedLanguage);
            }
        }
    }
}