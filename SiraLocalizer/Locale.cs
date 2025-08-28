using BGLib.Polyglot;

namespace SiraLocalizer
{
    /// <summary>
    /// Extended version of the <see cref="LocalizationLanguage"/> enum.
    /// </summary>
    public enum Locale
    {
#pragma warning disable CS1591 // enum entries are self-explanatory
        English = LocalizationLanguage.English,
        French = LocalizationLanguage.French,
        Spanish = LocalizationLanguage.Spanish,
        German = LocalizationLanguage.German,
        Italian = LocalizationLanguage.Italian,
        Portuguese_Brazil = LocalizationLanguage.Portuguese_Brazil,
        Portuguese = LocalizationLanguage.Portuguese,
        Russian = LocalizationLanguage.Russian,
        Greek = LocalizationLanguage.Greek,
        Turkish = LocalizationLanguage.Turkish,
        Danish = LocalizationLanguage.Danish,
        Norwegian = LocalizationLanguage.Norwegian,
        Swedish = LocalizationLanguage.Swedish,
        Dutch = LocalizationLanguage.Dutch,
        Polish = LocalizationLanguage.Polish,
        Finnish = LocalizationLanguage.Finnish,
        Japanese = LocalizationLanguage.Japanese,
        SimplifiedChinese = LocalizationLanguage.Simplified_Chinese,
        TraditionalChinese = LocalizationLanguage.Traditional_Chinese,
        Korean = LocalizationLanguage.Korean,
        Czech = LocalizationLanguage.Czech,
        Hungarian = LocalizationLanguage.Hungarian,
        Romanian = LocalizationLanguage.Romanian,
        Thai = LocalizationLanguage.Thai,
        Bulgarian = LocalizationLanguage.Bulgarian,
        Hebrew = LocalizationLanguage.Hebrew,
        Arabic = LocalizationLanguage.Arabic,
        Bosnian = LocalizationLanguage.Bosnian,
        Icelandic,
        Irish,
        // these 3 should always be at the end
        DebugKeys,
        DebugEnglishReverted,
        DebugEntryWithMaxLength,
#pragma warning restore CS1591
    }
}
