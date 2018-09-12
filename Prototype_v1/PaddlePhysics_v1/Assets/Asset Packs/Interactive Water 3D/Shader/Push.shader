// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Interactive Water 3D/Push" {
	Properties {
	}
	SubShader {
		Tags { "RenderType" = "Opaque" }
		Pass {
			ZWrite Off
			Cull Off
			Blend One One

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct v2f
			{
				float4 pos : SV_POSITION;
			};
			v2f vert (appdata_base v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				return o;
			}
			float4 frag (v2f i) : SV_TARGET
			{
				return float4(1, 0, 0, 1);
			}
			ENDCG
		}
	}
	FallBack Off
}