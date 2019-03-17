/*
	This is a simple unlit shader Unity creates, to which I made a few changes to allow transparency,
	and then I made transparency work for a cutoff shader.
*/


Shader "MegaManBareBonesEngine/CutoutSpriteShader"
{
	Properties
	{
		_Tint("Color", Color) = (1,1,1,1)
		_MainTex("Texture", 2D) = "white" {}
		_CutoutMap("Cutout", 2D) = "white" {}
		_CutoffStrength("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
	}
	SubShader
	{
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha
		Tags { "Queue" = "Transparent" "RenderType"="Transparent" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			fixed4 _Tint;
			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _CutoutMap;
			float _CutoffStrength;
			
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				// get cutoff shader strength
				fixed4 cut = tex2D(_CutoutMap, i.uv);
				if (cut.a > _CutoffStrength)
					col.a = 1.0;
				else
					col.a = 0.0;
				// tint the cutoff sprite
				col = col * _Tint;
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
