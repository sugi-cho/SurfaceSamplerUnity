using UnityEngine;

public class MeshInfoTexture : MonoBehaviour
{
    public static RenderTexture[] GeneratePositionNormalTexture(Mesh mesh, int width = 512, int height = 512)
    {
        var texes = new RenderTexture[2];
        for (var i = 0; i < 2; i++)
        {
            var tex = texes[i] = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat);
            RenderTexture.active = tex;
            GL.Clear(true, true, Color.clear);
        }
        var buffers = new[] { texes[0].colorBuffer, texes[1].colorBuffer };

        infoGen.SetPass(0);
        Graphics.SetRenderTarget(buffers, texes[0].depthBuffer);
        Graphics.DrawMeshNow(mesh, Matrix4x4.identity);

        return texes;
    }
    static Material infoGen { get { if (_infoGen == null) _infoGen = new Material(Shader.Find("Generator/mesh Info texture")); return _infoGen; } }
    static Material _infoGen;

    public static Texture2D ConvertTextureUV(Texture tex, Mesh mesh, int width = 512, int height = 512)
    {
        var rt = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
        RenderTexture.active = rt;
        GL.Clear(true, true, Color.clear);

        uvConverter.mainTexture = tex;
        uvConverter.SetFloat("_Alpha", 1f);
        uvConverter.SetPass(0);
        Graphics.SetRenderTarget(rt);
        Graphics.DrawMeshNow(mesh, Matrix4x4.identity);

        var tex2d = RenderTextureToTexture2D.Convert(rt);
        if(tex2d.format == TextureFormat.ARGB32)
        {
            var data = tex2d.EncodeToPNG();
            System.IO.File.WriteAllBytes("converted.png", data);
        }
        rt.Release();
        return tex2d;
    }
    static Material uvConverter { get { if (_converter == null) _converter = new Material(Shader.Find("Unlit/CopyTextureUV0ToUV1")); return _converter; } }
    static Material _converter;
}