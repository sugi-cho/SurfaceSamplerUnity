Shader "Unlit/CopyTextureUV0ToUV1"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Alpha ("alpha val", Float) = 0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100 ZTest Always Cull Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float2 uv2 : TEXCOORD1;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _Alpha;
			
			v2f vert (appdata v)
			{
				v.uv2.y = 1.0-v.uv2.y;

				v2f o;
				o.vertex = float4(v.uv2*2.0-1.0,0.0,1.0);
				o.uv = v.uv;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				col.a *= _Alpha;
				return col;
			}
			ENDCG
		}
	}
}
