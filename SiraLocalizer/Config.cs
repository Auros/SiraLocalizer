using IPA.Config.Stores;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace SiraLocalizer
{
    internal class Config
    {
        public virtual string Language { get; set; }
    }
}
