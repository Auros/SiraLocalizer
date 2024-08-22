using System.Globalization;
using BGLib.Polyglot;
using HarmonyLib;

namespace SiraLocalizer.HarmonyPatches
{
    [HarmonyPatch(typeof(LocalizationModel), nameof(LocalizationModel.SelectedCultureInfo), MethodType.Setter)]
    internal class LocalizationModel_SelectedCultureInfo_Setter
    {
        public static void Prefix(CultureInfo value)
        {
            // French uses ٪ rather than % for some reason
            if (value.IetfLanguageTag.StartsWith("fr"))
            {
                value.NumberFormat.PercentSymbol = "%";
            }
        }
    }
}
