using IPA.Utilities;
using SiraLocalizer.UI;
using TMPro;
using UnityEngine;
using Zenject;

namespace SiraLocalizer.Installers
{
    internal class SiraLocalizerGameplayInstaller : Installer
    {
        private FontAssetHelper _fontAssetHelper;

        public SiraLocalizerGameplayInstaller(FontAssetHelper fontAssetHelper)
        {
            _fontAssetHelper = fontAssetHelper;
        }

        public override void InstallBindings()
        {
            Container.Bind<TextBasedMissedNoteEffectSpawner>().FromNewComponentOn(new GameObject(nameof(TextBasedMissedNoteEffectSpawner))).AsSingle().NonLazy();
            
            Container.BindMemoryPool<FlyingTextEffect, FlyingTextEffect.Pool>().WithInitialSize(20).FromComponentInNewPrefab(CreateFlyingTextEffectPrefab()).WhenInjectedInto<ItalicizedFlyingTextSpawner>();
        }

        private FlyingTextEffect CreateFlyingTextEffectPrefab()
        {
            var gameObject = new GameObject("ItalicizedFlyingTextEffect");
            var textObject = new GameObject("Text");

            textObject.transform.parent = gameObject.transform;
            textObject.transform.localPosition = Vector3.zero;
            textObject.transform.localRotation = Quaternion.identity;

            TextMeshPro text = textObject.AddComponent<TextMeshPro>();
            text.fontStyle = FontStyles.Italic | FontStyles.UpperCase | FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = 3;

            // default font is Teko-Medium SDF No Glow (standard UI text)
            // we create a copy that doesn't do weird stuff when not on a curved canvas (i.e. doesn't have the CURVED shader keyword)
            TMP_FontAsset originalFont = text.font;
            TMP_FontAsset replacementFont = _fontAssetHelper.CopyFontAsset(originalFont, originalFont.material, "Teko-Medium SDF No Glow Flat");
            replacementFont.material.shaderKeywords = new string[0];
            text.font = replacementFont;

            // TODO figure out how to get a reference to EffectPoolsManualInstaller._flyingSpriteEffectPrefab to get animations
            var fadeAnimationCurve = new AnimationCurve();
            fadeAnimationCurve.AddKey(new Keyframe() { time = 0, value = 0, inWeight = 0.33333f, outWeight = 0.33333f });
            fadeAnimationCurve.AddKey(new Keyframe() { time = 0.151385f, value = 1, inWeight = 0.33333f, outWeight = 0.33333f, inTangent = 0.02442418f, outTangent = 0.02442418f });
            fadeAnimationCurve.AddKey(new Keyframe() { time = 1, value = 0, inWeight = 0.33333f, outWeight = 0.33333f });

            var moveAnimationCurve = new AnimationCurve();
            moveAnimationCurve.AddKey(new Keyframe() { time = 0, value = 0, inWeight = 0.3333333f, outWeight = 0.3333333f, inTangent = 4.531406f, outTangent = 4.531406f });
            moveAnimationCurve.AddKey(new Keyframe() { time = 0.1588104f, value = 0.7196346f, inWeight = 0.3333333f, outWeight = 0.3333333f, inTangent = 1.049638f, outTangent = 1.049638f });
            moveAnimationCurve.AddKey(new Keyframe() { time = 1, value = 1, inWeight = 0.3333333f, outWeight = 0.3333333f, inTangent = 0, outTangent = 0 });

            gameObject.SetActive(false);

            FlyingTextEffect flyingTextEffect = gameObject.AddComponent<ItalicizedFlyingTextEffect>();
            flyingTextEffect.SetField("_text", text);
            flyingTextEffect.SetField("_fadeAnimationCurve", fadeAnimationCurve);
            flyingTextEffect.SetField<FlyingObjectEffect, AnimationCurve>("_moveAnimationCurve", moveAnimationCurve);

            return flyingTextEffect;
        }
    }
}
