using IPA.Config.Stores;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace SiraLocalizer
{
    internal class Config
    {
        public virtual bool automaticallyDownloadLocalizations { get; set; } = true;
    }
}