using System.Threading.Tasks;
using Polyglot;

namespace SiraLocalizer
{
    interface ILocalizer
    {
        LocalizationAsset AddLocalizationAsset(LocalizationAsset localizationAsset);

        LocalizationAsset AddLocalizationAsset(string content, GoogleDriveDownloadFormat format);

        Task<LocalizationAsset> AddLocalizationAssetFromAssembly(string resourceName, GoogleDriveDownloadFormat format);
    }
}
