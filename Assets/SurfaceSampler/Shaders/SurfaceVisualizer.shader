Shader "Unlit/SurfaceVisualizer"
{
	Properties
	{
		_PosTex ("position", 2D) = "white"{}
		_NormTex ("normal", 2D) = "white"{}
		_ColTex ("color", 2D) = "white"{}
		_UvTex ("uv", 2D) = "white"{}
		_NumUvs ("num points", Float) = 1000

		_T ("visualize", Range(0,1)) = 0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 col : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _PosTex, _NormTex, _ColTex, _UvTex;
			float4 _UvTex_TexelSize;
			float _NumUvs, _T;
			
			v2f vert (appdata v)
			{
				uint idx = v.uv.y * _NumUvs;
				float x = floor(idx % _UvTex_TexelSize.z) / _UvTex_TexelSize.z;
				float y = floor(idx / _UvTex_TexelSize.z) / _UvTex_TexelSize.w;

				float4 uv = float4(x,y,0,0);
				uv = tex2Dlod(_UvTex, uv);
				uv.w = 0;

				float4 vertex = tex2Dlod(_PosTex, uv);
				float4 normal = tex2Dlod(_NormTex, uv);
				float4 color = tex2Dlod(_ColTex, uv);
				vertex.xyz = lerp( float3(uv.xy,0), vertex.xyz, _T);

				v2f o;
				o.vertex = UnityObjectToClipPos(vertex);
				o.col = color;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				return i.col;
			}
			ENDCG
		}
	}
}
