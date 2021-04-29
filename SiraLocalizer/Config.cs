using IPA.Config.Stores;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace SiraLocalizer
{
    internal class Config
    {
        public virtual bool showIncompleteTranslations { get; set; }
        public virtual bool autoDownloadNewLocalizations { get; set; } = true;
    }
}