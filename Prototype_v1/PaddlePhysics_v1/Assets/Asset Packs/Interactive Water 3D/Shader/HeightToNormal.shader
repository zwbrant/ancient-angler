// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Interactive Water 3D/Height To Normal" {
	Properties {
		_MainTex ("Main", 2D) = "black" {}
	}
	SubShader {
		Tags { "RenderType" = "Opaque" }
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#include "Utils.cginc"
			
			uniform sampler2D _MainTex;
			uniform float4 _MainTex_TexelSize;
			
			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};
			v2f vert (appdata_base v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord;
				return o;
			}
			float4 frag (v2f i) : SV_TARGET
			{
				float2 offset[4] = {
					float2(-1,  0),
					float2( 1,  0),
					float2( 0, -1),
					float2( 0,  1),
				};
				float L = HEIGHT_MAP_READ(_MainTex, i.uv + offset[0] * _MainTex_TexelSize.xy);
	            float R = HEIGHT_MAP_READ(_MainTex, i.uv + offset[1] * _MainTex_TexelSize.xy);
	            float B = HEIGHT_MAP_READ(_MainTex, i.uv + offset[2] * _MainTex_TexelSize.xy);
	            float F = HEIGHT_MAP_READ(_MainTex, i.uv + offset[3] * _MainTex_TexelSize.xy);

				float ny = 10.0 / _MainTex_TexelSize.z * 2.0;
	            float3 n = normalize(float3(-(R - L), ny, -(F - B)));
				n = (n + 1.0) * 0.5;
	            return float4(n.rgb, 1.0);
			}
			ENDCG
		}
	}
	FallBack Off
}