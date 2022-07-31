using System.ComponentModel;
using UnityEngine;

namespace Nash1m.Extensions
{
    public static class ScreenshotExtensions
    {
        public static Texture2D ToTexture2D(this RenderTexture rTex, TextureFormat textureFormat)
        {
            var tex = new Texture2D(rTex.width, rTex.height, textureFormat, false);
            RenderTexture.active = rTex;
            tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
            tex.Apply();
            return tex;
        }
        public static void SaveTexture(this Texture2D texture, string path, EncodeFormat encodeFormat)
        {
            var bytes = encodeFormat switch
            {
                EncodeFormat.PNG => texture.EncodeToPNG(),
                EncodeFormat.EXR => texture.EncodeToEXR(),
                EncodeFormat.JPG => texture.EncodeToJPG(),
                EncodeFormat.TGA => texture.EncodeToTGA(),
                _ => throw new InvalidEnumArgumentException()
            };
            System.IO.File.WriteAllBytes(path, bytes);
        }
        
        public static void SaveScreenshot(this Camera camera, string path, EncodeFormat encodeFormat, RenderTexture textureTemplate)
        {
            var renderTexture =
                new RenderTexture(textureTemplate.width, textureTemplate.height, textureTemplate.depth, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB)
                {
                    antiAliasing = textureTemplate.antiAliasing
                };
            
            camera.targetTexture = renderTexture;
            camera.Render();
            RenderTexture.active = renderTexture;
            
            var texture = renderTexture.ToTexture2D(TextureFormat.RGB24);
            
            SaveTexture(texture, path, encodeFormat);

            Object.DestroyImmediate(texture);
            camera.targetTexture = null;
            camera.Render();
            RenderTexture.active = null;
            
            Object.DestroyImmediate(renderTexture);
        }
    }
    
    public enum EncodeFormat
    {
        EXR,
        JPG,
        PNG,
        TGA
    }
}