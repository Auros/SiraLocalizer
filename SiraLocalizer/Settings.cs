using IPA.Config.Stores;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace SiraLocalizer
{
    internal class Settings
    {
        public virtual bool automaticallyDownloadLocalizations { get; set; } = false;

        public virtual bool startupModalDismissed { get; set; } = false;
    }
}
