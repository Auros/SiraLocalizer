using System.Collections.Generic;
using System.Threading;
using SiraLocalizer.Records;

namespace SiraLocalizer.Providers
{
    internal interface ILocalizationProvider
    {
        IAsyncEnumerable<LocalizationFile> GetLocalizationAssetsAsync(CancellationToken cancellationToken = default);
    }
}
