using BGLib.Polyglot;

namespace SiraLocalizer.Utilities
{
    internal static class LocalizationModelExtensions
    {
        public static string Get(this LocalizationModel model, string key, Language language)
        {
            model.TryGet(key, language, out string value);
            return value;
        }
    }
}
