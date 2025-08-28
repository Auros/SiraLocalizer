using TMPro;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

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

            bool shouldCopy = texture != null && texture.width > 0 && texture.height > 0;

            // 1 × 1 texture causes TMP to reinitialize the texture
            Texture2D newTexture = new(shouldCopy ? texture.width : 1, shouldCopy ? texture.height : 1, texture.graphicsFormat, texture.mipmapCount, TextureCreationFlags.DontInitializePixels | TextureCreationFlags.DontUploadUponCreate)
            {
                name = $"{newName} Atlas",
            };

            if (shouldCopy)
            {
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
