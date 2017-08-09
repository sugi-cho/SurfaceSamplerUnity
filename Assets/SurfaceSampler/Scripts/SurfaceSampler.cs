using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;

#endif

public class SurfaceSampler : MonoBehaviour
{
    const string assetFolderName = "MeshInfoTexture";
    string assetFolderPath = "Assets/" + assetFolderName;

    public Mesh mesh;
    public ComputeShader sampler;
    public Texture originColTex;
    public int texSize = 512;

    int SizeOf(System.Type type)
    {
        return System.Runtime.InteropServices.Marshal.SizeOf(type);
    }
    // Use this for initialization
    void Start()
    {
        if (mesh == null && GetComponent<MeshFilter>() != null)
            mesh = GetComponent<MeshFilter>().sharedMesh;
        if (mesh == null && GetComponent<SkinnedMeshRenderer>() != null)
            mesh = GetComponent<SkinnedMeshRenderer>().sharedMesh;

        var rts = MeshInfoTexture.GeneratePositionNormalTexture(mesh, texSize, texSize);

        var positionTex = RenderTextureToTexture2D.Convert(rts[0]);
        var normalTex = RenderTextureToTexture2D.Convert(rts[1]);
        Texture colorTex = null;
        if (originColTex != null)
            colorTex = MeshInfoTexture.ConvertTextureUV(originColTex, mesh, texSize, texSize);

        var uvBuffer = new ComputeBuffer(
            texSize * texSize,
            SizeOf(typeof(Vector2)),
            ComputeBufferType.Append);
        var uvCounter = new ComputeBuffer(
            1,
            SizeOf(typeof(int)),
            ComputeBufferType.IndirectArguments);
        uvBuffer.SetCounterValue(0);

        var kernel = sampler.FindKernel("sampleOpaqueTexel");
        sampler.SetInt("TexSize", texSize);
        sampler.SetTexture(kernel, "Tex", positionTex);
        sampler.SetBuffer(kernel, "OpaqueUv", uvBuffer);
        sampler.Dispatch(kernel, texSize / 8, texSize / 8, 1);

        var count = new[] { 0 };
        uvCounter.SetData(count);
        ComputeBuffer.CopyCount(uvBuffer, uvCounter, 0);
        uvCounter.GetData(count);
        var numUvs = count[0];
        Debug.Log(numUvs);

        var width = texSize / 2;
        var height = Mathf.NextPowerOfTwo(numUvs / width);
        var rt = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat);
        rt.enableRandomWrite = true;
        rt.Create();
        RenderTexture.active = rt;
        GL.Clear(true, true, Color.clear);

        kernel = sampler.FindKernel("buildUvTex");
        sampler.SetInt("TexSize", width);
        sampler.SetInt("NumUvs", numUvs);
        sampler.SetBuffer(kernel, "UvPool", uvBuffer);
        sampler.SetTexture(kernel, "Output", rt);
        sampler.Dispatch(kernel, width / 8, height / 8, 1);

        uvBuffer.Release();
        uvCounter.Release();

        var uvTex = RenderTextureToTexture2D.Convert(rt);
        positionTex.filterMode = normalTex.filterMode = colorTex.filterMode = uvTex.filterMode = FilterMode.Point;
        positionTex.wrapMode = normalTex.wrapMode = colorTex.wrapMode = uvTex.wrapMode = TextureWrapMode.Clamp;

        rt.Release();
        rts[0].Release();
        rts[1].Release();


#if UNITY_EDITOR
        if (!AssetDatabase.IsValidFolder(assetFolderPath))
            AssetDatabase.CreateFolder("Assets", assetFolderName);
        AssetDatabase.CreateAsset(positionTex, string.Format("{0}/{1}_pos.asset", assetFolderPath, mesh.name));
        AssetDatabase.CreateAsset(normalTex, string.Format("{0}/{1}_norm.asset", assetFolderPath, mesh.name));
        if (colorTex != null)
            AssetDatabase.CreateAsset(colorTex, string.Format("{0}/{1}_col.asset", assetFolderPath, mesh.name));
        AssetDatabase.CreateAsset(uvTex, string.Format("{0}/{1}_uv.asset", assetFolderPath, mesh.name));

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
#endif
    }
}
