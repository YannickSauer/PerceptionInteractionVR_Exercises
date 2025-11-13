using UnityEngine;
public class TextureGenerator
{
    public static Texture2D Randot(int width, int height)
    {
        Texture2D texture = new Texture2D(width, height);
        Color[] pixels = new Color[width * height];

        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Random.value > 0.5f ? Color.black : Color.white;
        }

        texture.SetPixels(pixels);
        texture.Apply();

        return texture;
    }
}