using IPA.Config.Stores;
using IPA.Config.Stores.Attributes;
using IPA.Config.Stores.Converters;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace SiraLocalizer
{
    internal class Config
    {
        [UseConverter(typeof(EnumConverter<Locale>))] public virtual Locale language { get; set; } = Locale.None;
        public virtual bool showIncompleteTranslations { get; set; }
    }
}