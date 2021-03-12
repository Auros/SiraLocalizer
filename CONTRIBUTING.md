# Contributing
## Becoming a Translator
We use [Crowdin](https://crowdin.com/) to make translating easier. You can request access to [the SiraLocalizer project](https://crowdin.com/project/siralocalizer) by filling out [this form](https://docs.google.com/forms/d/e/1FAIpQLSfk7z1EGqS2zl1jSomigSntvxQH0pTTKsxDlrpd9c53jKNpwA/viewform). We will get back to you as soon as possible!

## Testing new Crowdin translations
Testing translations that haven't officially been added to the mod yet is very easy. First, navigate to the language you want to download (this example uses French):

![SiraLocalizer Crowdin home page](https://i.imgur.com/JRaBEeN.png)

Then, press the 3 dots to the right of *Beat Saber* and press *Download*.

![Download translations file in Crowdin](https://i.imgur.com/d9hJMwk.png)

This will download a file called `sira-locale.csv`. Simply put this file in the `UserData\SIRA\Localizations` folder of your Beat Saber installation and you're good to go! Note that if the language you want to test isn't included in SiraLocalizer yet, you will need to open the `UserData\SiraLocalizer.json` file and change `showIncompleteTranslations` from `false` to `true`.