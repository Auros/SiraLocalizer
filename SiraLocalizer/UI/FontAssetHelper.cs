using IPA.Utilities;
using TMPro;
using UnityEngine;

namespace SiraLocalizer.UI
{
    internal class FontAssetHelper
    {
        private static readonly FieldAccessor<TMP_FontAsset, Texture2D>.Accessor kAtlasTextureAccessor = FieldAccessor<TMP_FontAsset, Texture2D>.GetAccessor("m_AtlasTexture");

        public TMP_FontAsset CopyFontAsset(TMP_FontAsset original, Material referenceMaterial, string newName = null)
        {
            if (string.IsNullOrEmpty(newName))
            {
                newName = original.name;
            }

            TMP_FontAsset copy = Object.Instantiate(original);

            // Unity doesn't copy textures when using Object.Instantiate so we have to do it manually
            Texture2D texture = original.atlasTexture;
            var newTexture = new Texture2D(texture.width, texture.height, texture.format, texture.mipmapCount, true)
            {
                name = $"{newName} Atlas",
            };
            Graphics.CopyTexture(texture, newTexture);

            var material = new Material(referenceMaterial)
            {
                name = $"{newName} Atlas Material",
            };
            material.SetTexture("_MainTex", newTexture);

            kAtlasTextureAccessor(ref copy) = newTexture;
            copy.name = newName;
            copy.atlasTextures = new[] { newTexture };
            copy.material = material;

            return copy;
        }
    }
}
