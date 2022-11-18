using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using IPA.Utilities;

namespace SiraLocalizer
{
    internal class UserLocalizationFileProvider : ILocalizationProvider
    {
        public async IAsyncEnumerable<LocalizationFile> GetLocalizationAssetsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            string folder = Path.GetFullPath(Path.Combine(UnityGame.UserDataPath, "SiraLocalizer", "Localizations", "User"));

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            foreach (string filePath in Directory.EnumerateFiles(folder, "*.csv"))
            {
                using StreamReader reader = new(filePath);
                string fileText = await reader.ReadToEndAsync();
                yield return new LocalizationFile(fileText, 2000);
            }
        }
    }
}
