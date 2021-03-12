# SiraLocalizer
Community localization support in Beat Saber. Created by nicoco007 and Auros.

Requires [SiraUtil](https://github.com/Auros/SiraUtil/releases/latest) 2.1.0 or greater.

## Supported Languages for the Base Game
The community has unofficially translated the base game into the following languages:
* Chinese (Simplified)
* French
* German
* Italian
* Korean
* Portuguese (Brazilian)
* Russian

See [CONTRIBUTORS](CONTRIBUTORS.md) for a list of everyone who has helped make SiraLocalizer possible!

You can change the language in-game by going to *Options* > *Settings* > *Others*.
![Localization Location](https://i.imgur.com/vAZwUkU.png)

## Becoming a Translator
See [CONTRIBUTING](CONTRIBUTING.md).

## Using SiraLocalizer in Your Mod
Modders can implement localizations into their own mods as well. Requires SiraUtil and some very basic knowledge of Zenject. Check out [SiraUtil's README](https://github.com/Auros/SiraUtil#zenject) for more information.

### Creating a Localization Sheet
Create a copy of [the Polyglot Template Google Sheet](https://docs.google.com/spreadsheets/d/17f0dQawb-s_Fd7DHgmVvJoEGDMH_yoSd8EYigrb0zmM/edit), erase all the keys that are there (starting at line 7 then all the way down), and add your own instead. **Do not delete any of the columns even if you don't plan on supporting that language** because Polyglot expects the specific order of languages used in the sheet.

### Retrieving and Using an `ILocalizer` Instance
You can get access to an `ILocalizer` instance by injecting it into a class. The example below uses constructor injection and [Zenject's IInitializable interface](https://github.com/svermeulen/Extenject#iinitializable). Note that you **must** use the `[InjectOptional]` attribute.

```cs
internal class MyLocalizationHandler : IInitializable
{
    private readonly ILocalizer _localizer;

    internal MyLocalizationHandler([InjectOptional(Id = "SIRA.Localizer")] ILocalizer localizer)
    {
        _localizer = localizer;
    }

    public void Initialize()
    {
        _localizer?.AddLocalizationSheetFromAssembly("YourAssemblyPath.ToThe.Localization.sheet.csv", GoogleDriveDownloadFormat.CSV);
    }
}
```

The `ILocalizer` interface is located in SiraUtil. This means you can add full localization support to your mod without having to depend on the `SiraLocalizer` mod.

### Other Goodies
A useful extension method for strings exists in SiraUtil...
```cs
myLocalizedText = "MY_MOD_LOCALIZATION_KEY".LocalizationGetOr("My Localized Text");
```
This will run the key through Polyglot, and if it does not exist for the current language, return the string in the parameter.

### Shadow Localizations
When calling `.AddLocalizationSheet` methods you can specify a parameter called `shadow`. Setting this to true will make that sheet a shadow sheet, and your localizations for a specific language will not show unless another sheet exists that has localizations for that same specific language is NOT marked as a shadow sheet. This is to prevent one mod having a large number of localizations for different languages "bloating" the language selection list.
