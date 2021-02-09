using IPA.Config.Stores;
using IPA.Config.Stores.Attributes;
using IPA.Config.Stores.Converters;
using Polyglot;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace SiraLocalizer
{
    internal class Config
    {
        [UseConverter(typeof(EnumConverter<Language>))] public virtual Language language { get; set; } = (Language)(-1);
        public virtual bool showIncompleteTranslations { get; set; }
    }
}