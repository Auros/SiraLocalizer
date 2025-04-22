using TMPro;
using UnityEngine;

namespace SiraLocalizer.UI
{
    internal static class FontAssetHelper
    {
        public static TMP_FontAsset CopyFontAsset(TMP_FontAsset original, Material referenceMaterial, string newName = null)
        {
            if (string.IsNullOrEmpty(newName))
            {
                newName = original.name;
            }

            TMP_FontAsset copy = Object.Instantiate(original);

            // Unity doesn't copy textures when using Object.Instantiate so we have to do it manually
            Texture2D texture = original.atlasTexture;
            Texture2D newTexture = null;

            if (texture != null && texture.width > 0 && texture.height > 0)
            {
                newTexture = new Texture2D(texture.width, texture.height, texture.format, texture.mipmapCount, true)
                {
                    name = $"{newName} Atlas",
                };

                Graphics.CopyTexture(texture, newTexture);
            }

            Material material = new(referenceMaterial)
            {
                name = $"{newName} Atlas Material",
            };

            material.SetTexture("_MainTex", newTexture);

            copy.m_AtlasTexture = newTexture;
            copy.name = newName;
            copy.atlasTextures = [newTexture];
            copy.material = material;

            return copy;
        }
    }
}
