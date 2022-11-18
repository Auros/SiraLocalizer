using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IPA.Utilities;
using Polyglot;
using SiraUtil.Logging;
using Zenject;

namespace SiraLocalizer
{
    internal class UserLocalizationFileLoader : IInitializable
    {
        private readonly SiraLog _logger;
        private readonly LocalizationManager _localizationManager;

        public UserLocalizationFileLoader(SiraLog logger, LocalizationManager localizationManager)
        {
            _logger = logger;
            _localizationManager = localizationManager;
        }

        public async void Initialize()
        {
            try
            {
                await LoadLocalesAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        public async Task LoadLocalesAsync()
        {
            string folder = Path.GetFullPath(Path.Combine(UnityGame.UserDataPath, "SIRA", "Localizations", "User"));

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            foreach (FileInfo file in new DirectoryInfo(folder).EnumerateFiles().Where(x => x.Extension is ".csv" or ".tsv"))
            {
                using (var reader = new StreamReader(file.FullName))
                {
                    string fileText = await reader.ReadToEndAsync();
                    _localizationManager.RegisterTranslation(fileText, file.Extension.EndsWith("csv") ? GoogleDriveDownloadFormat.CSV : GoogleDriveDownloadFormat.TSV, 100);
                }
            }

            LocalizationImporter.Refresh();
        }
    }
}
