using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using SiraLocalizer.Records;

namespace SiraLocalizer.Providers
{
    internal class ResourceLocalizationProvider : ILocalizationProvider
    {
        private static readonly string[] kResourcesToLoad = new[]
        {
            "SiraLocalizer.Resources.sira-localizer.csv",
        };

        public async IAsyncEnumerable<LocalizationFile> GetLocalizationAssetsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            foreach (string resourceName in kResourcesToLoad)
            {
                using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
                using StreamReader reader = new(stream);

                string content = await reader.ReadToEndAsync();

                yield return new LocalizationFile(content, 0);
            }
        }
    }
}
