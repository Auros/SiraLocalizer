using TMPro;
using System;
using Zenject;
using UnityEngine;
using IPA.Utilities;
using SiraLocalizer.UI;
using SiraUtil.Interfaces;

namespace SiraLocalizer.Providers
{
    internal class ItalicizedFlyingTextEffectModelProvider : IModelProvider
    {
        public Type Type => typeof(ItalicizedFlyingTextEffectPrefabProvider);

        public int Priority => 5;

        private class ItalicizedFlyingTextEffectPrefabProvider : IPrefabProvider<FlyingTextEffect>
        {
            public bool Chain => true;

            private DiContainer _container;
            private FontAssetHelper _fontAssetHelper;

            [Inject]
            public void Construct(DiContainer container, FontAssetHelper fontAssetHelper)
            {
                _container = container;
                _fontAssetHelper = fontAssetHelper;
            }

            public FlyingTextEffect Modify(FlyingTextEffect original)
            {
                if (_container != null)
                {
                    AnimationCurve fade = original.GetField<AnimationCurve, FlyingTextEffect>("_fadeAnimationCurve");
                    AnimationCurve move = original.GetField<AnimationCurve, FlyingObjectEffect>("_moveAnimationCurve");

                    _container.BindMemoryPool<FlyingTextEffect, FlyingTextEffect.Pool>().WithInitialSize(20).FromComponentInNewPrefab(CreateFlyingTextEffectPrefab(fade, move)).WhenInjectedInto<ItalicizedFlyingTextSpawner>();
                }

                return original;
            }

            private FlyingTextEffect CreateFlyingTextEffectPrefab(AnimationCurve fadeCurve, AnimationCurve moveCurve)
            {
                var gameObject = new GameObject("ItalicizedFlyingTextEffect");
                var textObject = new GameObject("Text");

                textObject.transform.parent = gameObject.transform;
                textObject.transform.localPosition = Vector3.zero;
                textObject.transform.localRotation = Quaternion.identity;

                TextMeshPro text = textObject.AddComponent<TextMeshPro>();

                TMP_FontAsset originalFont = text.font;
                TMP_FontAsset replacementFont = _fontAssetHelper.CopyFontAsset(originalFont, originalFont.material, "Teko-Medium SDF No Glow Flat");
                replacementFont.material.shaderKeywords = new string[0];
                text.font = replacementFont;

                text.fontStyle = FontStyles.Italic | FontStyles.UpperCase | FontStyles.Bold;
                text.alignment = TextAlignmentOptions.Center;
                text.fontSize = 3;

                gameObject.SetActive(false);

                FlyingTextEffect flyingTextEffect = gameObject.AddComponent<ItalicizedFlyingTextEffect>();
                flyingTextEffect.SetField("_text", text);
                flyingTextEffect.SetField("_fadeAnimationCurve", fadeCurve);
                flyingTextEffect.SetField<FlyingObjectEffect, AnimationCurve>("_moveAnimationCurve", moveCurve);

                return flyingTextEffect;
            }
        }
    }
}