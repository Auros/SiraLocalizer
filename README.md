# SiraLocalizer
 Community localization support in Beat Saber. Created by nicoco007 and Auros.

 Requires [SiraUtil](https://github.com/Auros/SiraUtil/releases/latest) 2.0.0 or greater.
# Supported Languages for the BASE GAME
 The community has unofficially translated the base game into the following languages:
* French
* Chinese

You can change the settings here.
![Localization Location](https://cdn.sira.pro/images/language_option_location.png)

## French Contributors
* [Nicolas (nicoco007)](https://github.com/nicoco007)

## Chinese Contributors
* 乾杯君
* [WGzeyu](https://github.com/WGzeyu)
* 暗黑幻想Dark师
* 包子侯爵
* 四条
* 椰子

# Creating Custom Localization Sheets
 You can create a copy of [this spreadsheet](https://docs.google.com/spreadsheets/d/1NERV_PftlFQFKByvCxWV6hs9XaRLmNyMBOSLf4285AY/edit?usp=sharing) and fill out the keys for your language. Once completed, it can be exported as a **csv** and be placed into `Beat Saber/UserData/SIRA/Localizations`. If the mod is installed, this folder will be created for you. Launch the game and the localizations will be loaded into the game.

# Mod Support
 Modders can implement localizations into their own mods as well. Requires SiraUtil and some very basic knowledge of Zenject.
 
 **In YourClass.cs that can receive objects through zenject**
 
 ```cs
 private readonly ILocalizer localizer;
 
 internal YourClass([InjectOptional(Id = "SIRA.Localizer")] ILocalizer localizer)
 {
   this.localizer = localizer;
 }
 // This can be done anyway you'd like (constructor injection or method injection if you're in a MonoBehaviour)
 ```

 Usage for the localizer goes as follows.
 ```cs
 localizer.AddLocalizationSheetFromAssembly("YourAssemblyPath.ToThe.Localization.sheet.csv", GoogleDriveDownloadFormat.CSV);
 ```

 The `ILocalizer` interface is located in SiraUtil. This means you can add full localization support to your mod without having to depend on the `SiraLocalizer` mod.
 
 A useful extension method for strings exists in SiraUtil...
 ```cs
 myLocalizedText = "MY_MOD_LOCALIZATION_KEY".LocalizationGetOr("My Localized Text");
 ```
 This will run the key through Polyglot, and if it does not exist for the current language, return the string in the parameter.
 
 ## Shadow Properties
 In the .AddLocalizationSheet... methods you can specify a parameter called `shadow`. Setting this to true will make that sheet a shadow sheet, and your localizations for a specific language will not show unless another sheet exists that has localizations for that same specific language is NOT marked as a shadow sheet. This is to prevent one mod having a large number of localizations for different languages "bloating" the language selection list.
