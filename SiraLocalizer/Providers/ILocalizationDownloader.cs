using System.Threading;
using System.Threading.Tasks;

namespace SiraLocalizer.Providers
{
    internal interface ILocalizationDownloader
    {
        string name { get; }

        Task DownloadLocalizationsAsync(CancellationToken cancellationToken);

        Task<bool> CheckForUpdatesAsync(CancellationToken cancellationToken);
    }
}
