using UnityEngine;

public static class TextureConverter
{
    /// <summary>
    /// Converts a Texture2D to a Sprite, with optional parameters to specify the pixels per unit
    /// and whether to create a full rectangular sprite or to cut it according to the texture alpha.
    /// </summary>
    /// <param name="texture">The texture to convert to sprite.</param>
    /// <param name="pixelsPerUnit">The number of pixels per unit. Default is 100.</param>
    /// <param name="pivot">The pivot point of the sprite. Default is center.</param>
    /// <param name="extrude">The extrusion amount for the sprite edges. Default is 0.</param>
    /// <param name="meshType">The mesh type for non-rectangular sprites. Default is FullRect.</param>
    /// <returns>Returns the created sprite.</returns>
    public static Sprite ConvertToSprite(Texture2D texture, float pixelsPerUnit = 100.0f, Vector2? pivot = null, uint extrude = 0, SpriteMeshType meshType = SpriteMeshType.FullRect)
    {
        if (texture == null)
        {
            Debug.LogError("ConvertToSprite error: Texture is null");
            return null;
        }

        Vector2 spritePivot = pivot ?? new Vector2(0.5f, 0.5f); // Default pivot is the center
        Rect rect = new Rect(0, 0, texture.width, texture.height);
        Sprite newSprite = Sprite.Create(texture, rect, spritePivot, pixelsPerUnit, extrude, meshType);
        newSprite.name = texture.name;
        return newSprite;
    }


    // Function to copy a Texture2D
    public static Texture2D CopyTexture2D(Texture2D original)
    {
        // Create a new empty texture with the same size and format
        Texture2D copy = new Texture2D(original.width, original.height, original.format, original.mipmapCount > 1);
        // Apply the changes to the new texture
        copy.Apply();

        // Get the pixel data from the original texture
        Color[] pixels = original.GetPixels();
        // Set the pixel data to the new texture
        copy.SetPixels(pixels);
        // Apply the changes to the new texture
        copy.Apply();

        return copy;
    }
}
