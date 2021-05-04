# SiraLocalizer
Community localization support for Beat Saber and its mods. Created by nicoco007 and Auros.

Requires [SiraUtil](https://github.com/Auros/SiraUtil/releases/latest) 2.1.0 or greater.

## Supported Languages for the Base Game
The community has unofficially translated the base game into the following languages:
* Chinese (Simplified)
* Chinese (Traditional)
* Dutch
* French
* German
* Hebrew
* Hungarian
* Italian
* Japanese
* Korean
* Portuguese (Brazilian)
* Russian
* Swedish

See [CONTRIBUTORS](CONTRIBUTORS.md) for a list of everyone who has helped make SiraLocalizer possible!

Since SiraLocalizer is built on top of the game's regular localization system, you can change the language in-game normally by going to *Options* > *Settings* > *Others*. Translation contributors and information regarding which mods are supported is given under the language selection dropdown. The example below shows information for the French translation:
![Localization Location](https://i.imgur.com/hXhGZYi.png)

## Becoming a Translator
See [CONTRIBUTING](CONTRIBUTING.md).

## Getting SiraLocalizer to Translate Your Mod
*This section is under construction!*
### Preparing a Mod for Translation
Beat Saber uses the [Polyglot](https://github.com/agens-no/PolyglotUnity) Unity package for translations. It uses translation keys as unique identifiers for translated text across the game. Therefore, wherever there is text in your mod, it should be replaced with something that can convert a translation key into localized text.

[BeatSaberMarkupLanguage](https://github.com/monkeymanboy/BeatSaberMarkupLanguage) now supports translating various text elements with the `-key` suffix (e.g. `text-key` for text, `tab-name-key` for tabs, etc.). More documentation regarding this is coming soon. If you are displaying text through other means, you can use the `Polyglot.Localization.Get(string)` and `Polyglot.Localization.GetFormat(string, params object[])` methods to get localized strings in the current language.

### Creating a Polyglot Translation File
Polyglot stores translations in CSV or TSV files. We *highly* recommend using a CSV file since it supports line breaks inside values and empty columns are easier to format. Polyglot's file format is very straightforward: the first column is the translation key, the second column is context to help translators, and the rest of the columns are translations following [the order in the Locale enum](SiraLocalizer/Locale.cs).

If you plan on using SiraLocalizer for translations, this is what your mod's CSV file should look like:
```text
Polyglot,100,,,,,,,,,,,,,,,,,,,,,,,,,,,,
"KEY_NAME_1","Context for 1 if necessary","English String 1",,,,,,,,,,,,,,,,,,,,,,,,,,,
"KEY_NAME_2","Context for 2 if necessary","English String 2",,,,,,,,,,,,,,,,,,,,,,,,,,,
```

The first line is required for Polyglot to properly identify the file. Also, note the trailing commas &ndash; these are important since Polyglot will show the translation key instead of the fallback (English) text if a column doesn't exist for the selected language. Since Polyglot supports 28 languages out-of-the-box, there should be at least 27 commas after the English text.

Once you've added all the translation keys for your mod in a CSV file following the format above, it needs to be loaded when the game starts. We recommend doing this by adding your CSV file as an embedded resource within your mod, and then loading it using the following snippet:

```cs
private void AddLocalizationFromResource()
{
    using (StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Assembly.Path.To.Your.translations.csv")))
    {
        string content = reader.ReadToEnd();
        Localization.Instance.InputFiles.Add(new LocalizationAsset { Format = GoogleDriveDownloadFormat.CSV, TextAsset = new TextAsset(content) });
    }
}
```

You should call this method in your plugin's `[OnEnable]` or `[OnStart]` method. If everything works properly, translations keys should now show up as the English text you wrote in the CSV file.

### Registering a Mod for Translation
Registering your mod to be translated by the SiraLocalizer team is simple. First, fill out this request form (coming soon). If it the first time you request translations, you will be given an ID for your mod. Once you have this ID, simply add this JSON object to your manifest's `features` object:

```json
"SiraLocalizer.LocalizedPlugin": {
    "id": "your-mod-id",
    "resourcePath": "Assembly.Path.To.Your.translations.csv"
}
```

Once translations are available, they will automatically be downloaded by SiraLocalizer.
