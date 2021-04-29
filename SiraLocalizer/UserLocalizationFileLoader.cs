using Zenject;
using Polyglot;
using System.IO;
using System.Linq;
using IPA.Utilities;
using System.Threading.Tasks;

namespace SiraLocalizer
{
    internal class UserLocalizationFileLoader : IInitializable
    {
        private readonly Localizer _localizer;

        public UserLocalizationFileLoader(Localizer localizer)
        {
            _localizer = localizer;
        }

        public void Initialize()
        {
            _ = LoadLocales();
        }

        public async Task LoadLocales()
        {
            var folder = Path.Combine(UnityGame.UserDataPath, "SIRA", "Localizations");

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            foreach (FileInfo file in new DirectoryInfo(folder).EnumerateFiles().Where(x => x.Extension == ".csv" || x.Extension == ".tsv"))
            {
                using (var reader = File.OpenText(file.FullName))
                {
                    var fileText = await reader.ReadToEndAsync();
                    _localizer.AddLocalizationSheet(fileText, file.Extension.EndsWith("csv") ? GoogleDriveDownloadFormat.CSV : GoogleDriveDownloadFormat.TSV);
                }
            }
        }
    }
}