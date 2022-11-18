using System.Threading;
using System.Threading.Tasks;

namespace SiraLocalizer
{
    internal interface ILocalizationDownloader
    {
        string name { get; }

        Task DownloadLocalizationsAsync(CancellationToken cancellationToken);

        Task<bool> CheckForUpdatesAsync(CancellationToken cancellationToken);
    }
}
