using System;
using System.Reflection;
using BGLib.Polyglot;

namespace SiraLocalizer.Utilities
{
    internal static class LocalizationModelExtensions
    {
        private delegate void OnLocalizeDelegate(LocalizationModel self, Action<LocalizationModel> action);

        private static readonly OnLocalizeDelegate kRemoveOnChangeLanguage = (OnLocalizeDelegate)typeof(LocalizationModel).GetEvent("_onChangeLanguage", BindingFlags.Instance | BindingFlags.NonPublic).GetRemoveMethod(true).CreateDelegate(typeof(OnLocalizeDelegate));

        public static string Get(this LocalizationModel model, string key, Language language)
        {
            model.TryGet(key, language, out string value);
            return value;
        }

        public static void RemoveOnLocalizeEvent(this LocalizationModel model, ILocalize localize)
        {
            kRemoveOnChangeLanguage(model, localize.OnLocalize);
        }
    }
}
