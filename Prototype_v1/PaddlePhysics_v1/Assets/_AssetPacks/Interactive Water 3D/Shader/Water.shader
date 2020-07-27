// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

#warning Upgrade NOTE: unity_Scale shader variable was removed; replaced 'unity_Scale.w' with '1.0'

Shader "Interactive Water 3D/Water" {
	Properties {
		_HeightMap     ("Height", 2D) = "black" {}
		_NormalMap     ("Normal", 2D) = "black" {}
		_ReflectionMap ("Reflection", 2D) = "black" {}
		_RefractionMap ("Refraction", 2D) = "black" {}
		_FresnelMap    ("Fresnel", 2D) = "black" {}
		_BumpMap       ("Bump", 2D) = "bump" {}
		_HeightScale   ("Height Scale", Float) = 0.3
		_Diffuse       ("Diffuse", Color) = (0, 0.5, 1, 1)
		_Specular      ("Specular", Color) = (1, 1, 1, 1)
		_Transparence  ("Transparence", Float) = 1
	}
	SubShader
	{
		Tags { "Queue" = "Transparent" }
		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile _ IW3D_USE_VTF
			#include "UnityCG.cginc"
			#include "Utils.cginc"

			uniform sampler2D _HeightMap, _NormalMap, _ReflectionMap, _RefractionMap, _FresnelMap, _BumpMap;
			uniform float4x4 _OrthCamViewProj;
			uniform float4 _Diffuse, _Specular;
			uniform float _HeightScale, _Transparence;

			struct v2f
			{
				float4 pos     : SV_POSITION;
				float2  uv      : TEXCOORD0;
				float3 wldpos  : TEXCOORD1;
				float3 wldview : TEXCOORD2;
				float3 wldlit  : TEXCOORD3;
				float4 ref     : TEXCOORD4;
				float2 bumpuv0 : TEXCOORD5;
				float2 bumpuv1 : TEXCOORD6;
			};
			v2f vert (appdata_base v)
			{
				float4x4 orthMVP = mul(_OrthCamViewProj, unity_ObjectToWorld);
				float4 proj = mul(orthMVP, v.vertex);
				float2 uv = 0.5 * proj.xy / proj.w + 0.5;
#if UNITY_UV_STARTS_AT_TOP
				uv.y = 1 - uv.y;
#endif
#ifdef IW3D_USE_VTF
				float height = 0;
				height = DecodeHeightmap(tex2Dlod(_HeightMap, float4(uv, 0, 1))) * _HeightScale;
				float3 newPos = v.vertex.xyz + float3(0, height, 0);
#else
				float3 newPos = v.vertex.xyz;
#endif
				v2f o;
				o.pos = UnityObjectToClipPos(float4(newPos,1.0));
				o.uv = uv;
				o.wldpos = mul(unity_ObjectToWorld, newPos);   // the modified vertex world position
				o.wldview = WorldSpaceViewDir(v.vertex);
				o.wldlit = WorldSpaceLightDir(v.vertex);
				o.ref = ComputeScreenPos(o.pos);

				// scroll bump waves
				float4 temp;
				temp.xyzw = v.vertex.xzxz * 0.063 + _Time.y * 0.03;
				o.bumpuv0 = temp.xy;
				o.bumpuv1 = temp.wz;
				return o;
			}
			float4 frag (v2f i) : COLOR
			{
				float3 bump1 = UnpackNormal(tex2D(_BumpMap, i.bumpuv0)).rgb;
				float3 bump2 = UnpackNormal(tex2D(_BumpMap, i.bumpuv1)).rgb;
				float3 bump = (bump1 + bump2) * 0.5;

				float4 uv1 = i.ref;
				uv1.xy += bump * 0.3;
				float4 refl = tex2Dproj(_ReflectionMap, UNITY_PROJ_COORD(uv1));

				float4 uv2 = i.ref;
				uv2.xy -= bump * 0.3;
				float4 refr = tex2Dproj(_RefractionMap, UNITY_PROJ_COORD(uv2));

				float4 normalmap = tex2D(_NormalMap, i.uv);
				float3 N = (normalmap.rgb - 0.5) * 2.0;
				float3 L = normalize(i.wldlit);
				
				float3 incident = normalize(i.wldpos.xyz - _WorldSpaceCameraPos.xyz);
				float3 R = normalize(reflect(incident, N));
				
				float fresnelFac = dot(incident, N);
				float fresnel = UNITY_SAMPLE_1CHANNEL(_FresnelMap, fresnelFac.xx);
				float4 c2 = lerp(refr, refl, fresnel);
				
				float diffuse_factor = 0.2 + 0.2 * dot(L, N);
				float specular_factor = pow(max(0, dot(L, R)), 100);
				float3 c = float3(0, 0, 0);
				c += diffuse_factor * _Diffuse.rgb;
				c += specular_factor * _Specular.rgb;
				c += c2;
				return float4(c, _Transparence);
			}
			ENDCG
		}
	}
}