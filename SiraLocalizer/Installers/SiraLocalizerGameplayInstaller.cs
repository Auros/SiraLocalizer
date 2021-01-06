using IPA.Utilities;
using SiraLocalizer.UI;
using System.Linq;
using TMPro;
using UnityEngine;
using Zenject;

namespace SiraLocalizer.Installers
{
    internal class SiraLocalizerGameplayInstaller : Installer
    {
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
            FlyingTextEffect flyingTextEffect = gameObject.AddComponent<FlyingTextEffect>();

            // TODO figure out how to get a reference to EffectPoolsManualInstaller._flyingTextEffectPrefab to get font
            text.fontStyle = FontStyles.Italic | FontStyles.UpperCase | FontStyles.Bold;
            text.font = Resources.FindObjectsOfTypeAll<TMP_FontAsset>().First(f => f.name == "Teko-Medium SDF No Glow");
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = 3;

            // TODO figure out how to get a reference to EffectPoolsManualInstaller._flyingSpriteEffectPrefab to get animations
            var fadeAnimationCurve = new AnimationCurve();
            fadeAnimationCurve.AddKey(new Keyframe() { time = 0, value = 0, inWeight = 0.33333f, outWeight = 0.33333f });
            fadeAnimationCurve.AddKey(new Keyframe() { time = 0.151385f, value = 1, inWeight = 0.33333f, outWeight = 0.33333f, inTangent = 0.02442418f, outTangent = 0.02442418f });
            fadeAnimationCurve.AddKey(new Keyframe() { time = 1, value = 0, inWeight = 0.33333f, outWeight = 0.33333f });

            var moveAnimationCurve = new AnimationCurve();
            moveAnimationCurve.AddKey(new Keyframe() { time = 0, value = 0, inWeight = 0.3333333f, outWeight = 0.3333333f, inTangent = 4.531406f, outTangent = 4.531406f });
            moveAnimationCurve.AddKey(new Keyframe() { time = 0.1588104f, value = 0.7196346f, inWeight = 0.3333333f, outWeight = 0.3333333f, inTangent = 1.049638f, outTangent = 1.049638f });
            moveAnimationCurve.AddKey(new Keyframe() { time = 1, value = 1, inWeight = 0.3333333f, outWeight = 0.3333333f, inTangent = 0, outTangent = 0 });

            flyingTextEffect.SetField("_text", text);
            flyingTextEffect.SetField("_fadeAnimationCurve", fadeAnimationCurve);
            flyingTextEffect.SetField<FlyingObjectEffect, AnimationCurve>("_moveAnimationCurve", moveAnimationCurve);

            return flyingTextEffect;
        }
    }
}
