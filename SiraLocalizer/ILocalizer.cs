using System.Threading.Tasks;
using Polyglot;

namespace SiraLocalizer
{
    public interface ILocalizer
    {
        LocalizationAsset AddLocalizationAsset(LocalizationAsset localizationAsset);

        LocalizationAsset AddLocalizationAsset(string content, GoogleDriveDownloadFormat format);

        LocalizationAsset AddLocalizationAssetFromAssembly(string resourceName, GoogleDriveDownloadFormat format);

        Task<LocalizationAsset> AddLocalizationAssetFromAssemblyAsync(string resourceName, GoogleDriveDownloadFormat format);
    }
}
