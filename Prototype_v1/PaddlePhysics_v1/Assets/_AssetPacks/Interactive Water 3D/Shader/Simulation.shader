Shader "Interactive Water 3D/Simulation" {
	Properties {
		_HeightPrevTex    ("Height Previous", 2D) = "black" {}
		_HeightCurrTex ("Height Current", 2D) = "black" {}
		_Damping          ("Water Damping", Float) = 0.99
		_Parameters       ("Parameters", Vector) = (0,0,0,0)
	}
	SubShader {
		Tags { "RenderType" = "Opaque" }
		Pass {
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			#include "UnityCG.cginc"
			#include "Utils.cginc"
			
			uniform sampler2D _HeightPrevTex, _HeightCurrTex;
			uniform float4 _HeightCurrTex_TexelSize;
			uniform float3 _Parameters;
			uniform float  _Damping;
			
			float4 frag (v2f_img i) : SV_TARGET
			{
				float2 offset[4] = {
					float2 (-1,  0),
					float2 ( 1,  0),
					float2 ( 0, -1),
					float2 ( 0,  1),
				};
				float prev = HEIGHT_MAP_READ(_HeightPrevTex, i.uv);
				float curr = HEIGHT_MAP_READ(_HeightCurrTex, i.uv);
				float adjoin = HEIGHT_MAP_READ(_HeightCurrTex, i.uv + offset[0] * _HeightCurrTex_TexelSize.xy);
				adjoin += HEIGHT_MAP_READ(_HeightCurrTex, i.uv + offset[1] * _HeightCurrTex_TexelSize.xy);
				adjoin += HEIGHT_MAP_READ(_HeightCurrTex, i.uv + offset[2] * _HeightCurrTex_TexelSize.xy);
				adjoin += HEIGHT_MAP_READ(_HeightCurrTex, i.uv + offset[3] * _HeightCurrTex_TexelSize.xy);

	            float next = _Parameters.x * curr + _Parameters.y * prev + _Parameters.z * adjoin;
				next *= _Damping;
	            return HEIGHT_MAP_WRITE(next);
			}
			ENDCG
		}
	}
	FallBack Off
}
