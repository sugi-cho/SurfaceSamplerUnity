using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class RenderTextureToTexture2D : MonoBehaviour
{

    public static Texture2D Convert(RenderTexture rt)
    {
        TextureFormat format;

        switch (rt.format)
        {
            case RenderTextureFormat.ARGBFloat:
                format = TextureFormat.RGBAFloat;
                break;
            case RenderTextureFormat.ARGBHalf:
                format = TextureFormat.RGBAHalf;
                break;
            case RenderTextureFormat.ARGBInt:
                format = TextureFormat.RGBA32;
                break;
            case RenderTextureFormat.ARGB32:
                format = TextureFormat.ARGB32;
                break;
            default:
                format = TextureFormat.ARGB32;
                Debug.LogWarning("Unsuported RenderTextureFormat.");
                break;
        }

        return Convert(rt, format);
    }

    static Texture2D Convert(RenderTexture rt, TextureFormat format)
    {
        var tex2d = new Texture2D(rt.width, rt.height, format, false);
        var rect = Rect.MinMaxRect(0f, 0f, tex2d.width, tex2d.height);
        RenderTexture.active = rt;
        tex2d.ReadPixels(rect, 0, 0);
        RenderTexture.active = null;
        tex2d.Apply();
        return tex2d;
    }

#if UNITY_EDITOR
    [MenuItem("Assets/Convert/TextureToPNG")]
    public static void ConvertToPNG()
    {
        var tex = Selection.activeObject as Texture;
        if (tex == null) return;
        var rt = new RenderTexture(tex.width, tex.height, 0, RenderTextureFormat.ARGB32);
        Graphics.Blit(tex, rt);
        var tex2d = Convert(rt);

        var pngData = tex2d.EncodeToPNG();
        var path = tex.name + ".png";
        System.IO.File.WriteAllBytes(path, pngData);

        rt.Release();
        DestroyImmediate(rt);
        DestroyImmediate(tex2d);
    }
#endif
}
