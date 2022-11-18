using System.Collections.Generic;
using System.Threading;

namespace SiraLocalizer
{
    internal interface ILocalizationProvider
    {
        IAsyncEnumerable<LocalizationFile> GetLocalizationAssetsAsync(CancellationToken cancellationToken = default);
    }
}
