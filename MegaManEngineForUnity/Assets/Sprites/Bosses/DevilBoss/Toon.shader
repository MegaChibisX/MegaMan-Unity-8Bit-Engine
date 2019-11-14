// Sources:
//		https://lindenreid.wordpress.com/2017/12/19/cel-shader-with-outline-in-unity/
//		https://roystan.net/articles/toon-shader.html
// Those two shaders were merged to achieve the desired effect, as I don't have the knowledge to code it myself ^^;

Shader "MegaManBareBonesEngine/Toon"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_RampTex("Ramp", 2D) = "white" {}
		_EmisTex("Emission", 2D) = "black" {}
		_Color("Color", Color) = (1, 1, 1, 1)
		_ColorEmis("Color Emission", Color) = (1, 1, 1, 1)
		_OutlineExtrusion("Outline Extrusion", float) = 0
		_OutlineColor("Outline Color", Color) = (0, 0, 0, 1)
		_OutlineDot("Outline Dot", float) = 0.25

		[HDR]
		_AmbientColor("Ambient Color", Color) = (0.4,0.4,0.4,1)
		[HDR]
		_SpecularColor("Specular Color", Color) = (0.9,0.9,0.9,1)
		[HDR]
		_ShadowColor("Shadow Color", Color) = (0.4,0.4,0.4,1)
		_Glossiness("Glossiness", Float) = 32
		[HDR]
		_RimColor("Rim Color", Color) = (1,1,1,1)
		_RimAmount("Rim Amount", Range(0, 1)) = 0.716
		_RimThreshold("Rim Threshold", Range(0, 1)) = 0.1
	}

		SubShader
		{
			Pass
			{
			// Setup our pass to use Forward rendering, and only receive
			// data on the main directional light and ambient light.
			Tags
			{
				"LightMode" = "ForwardBase"
				"PassFlags" = "OnlyDirectional"
			}

			Stencil
			{
				Ref 4
				Comp always
				Pass replace
				ZFail keep
			}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// Compile multiple versions of this shader depending on lighting settings.
			#pragma multi_compile_fwdbase

			#include "UnityCG.cginc"
			// Files below include macros and functions to assist
			// with lighting and shadows.
			#include "Lighting.cginc"
			#include "AutoLight.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float4 uv : TEXCOORD0;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float3 worldNormal : NORMAL;
				float2 uv : TEXCOORD0;
				float3 viewDir : TEXCOORD1;
				// Macro found in Autolight.cginc. Declares a vector4
				// into the TEXCOORD2 semantic with varying precision 
				// depending on platform target.
				SHADOW_COORDS(2)
			};

			sampler2D _MainTex;
			sampler2D _EmisTex;
			float4 _MainTex_ST;

			v2f vert(appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.worldNormal = UnityObjectToWorldNormal(v.normal);
				o.viewDir = WorldSpaceViewDir(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				// Defined in Autolight.cginc. Assigns the above shadow coordinate
				// by transforming the vertex from world space to shadow-map space.
				TRANSFER_SHADOW(o)
				return o;
			}

			float4 _Color;
			float4 _ColorEmis;

			float4 _AmbientColor;
			float4 _ShadowColor;

			float4 _SpecularColor;
			float _Glossiness;

			float4 _RimColor;
			float _RimAmount;
			float _RimThreshold;

			float4 frag(v2f i) : SV_Target
			{
				float3 normal = normalize(i.worldNormal);
				float3 viewDir = normalize(i.viewDir);

				// Lighting below is calculated using Blinn-Phong,
				// with values thresholded to creat the "toon" look.
				// https://en.wikipedia.org/wiki/Blinn-Phong_shading_model

				// Calculate illumination from directional light.
				// _WorldSpaceLightPos0 is a vector pointing the OPPOSITE
				// direction of the main directional light.
				float NdotL = dot(_WorldSpaceLightPos0, normal);

				// Samples the shadow map, returning a value in the 0...1 range,
				// where 0 is in the shadow, and 1 is not.
				float shadow = 1;
				// Partition the intensity into light and dark, smoothly interpolated
				// between the two to avoid a jagged break.
				float lightIntensity = smoothstep(0, 0.01, NdotL * shadow);
				// Multiply by the main directional light's intensity and color.
				float4 light = lightIntensity * _LightColor0;

				// Calculate specular reflection.
				float3 halfVector = normalize(_WorldSpaceLightPos0 + viewDir);
				float NdotH = dot(normal, halfVector);
				// Multiply _Glossiness by itself to allow artist to use smaller
				// glossiness values in the inspector.
				float specularIntensity = pow(NdotH * lightIntensity, _Glossiness * _Glossiness);
				float specularIntensitySmooth = smoothstep(0.005, 0.01, specularIntensity);
				float4 specular = specularIntensitySmooth * _SpecularColor;

				// Calculate rim lighting.
				float rimDot = 1 - dot(viewDir, normal);
				// We only want rim to appear on the lit side of the surface,
				// so multiply it by NdotL, raised to a power to smoothly blend it.
				float rimIntensity = rimDot * pow(NdotL, _RimThreshold);
				rimIntensity = smoothstep(_RimAmount - 0.01, _RimAmount + 0.01, rimIntensity);
				float4 rim = rimIntensity * _RimColor;

				float4 sample = tex2D(_MainTex, i.uv);
				float4 emisSample = tex2D(_EmisTex, i.uv);
				emisSample = emisSample * _ColorEmis.a;
				float maxGlow = max(emisSample.r * _ColorEmis.r, emisSample.g * _ColorEmis.g);
				maxGlow = max(emisSample.b * _ColorEmis.b, maxGlow) * emisSample.a * _ColorEmis.a;

				return ((light + _AmbientColor + specular + rim) * _Color + (1 - light) * _ShadowColor) * sample * (1 - maxGlow) + maxGlow * emisSample * _ColorEmis;
			}
			ENDCG
		}

			// Shadow casting support.
			UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"

			// Shadow pass
			Pass
			{
				Tags
				{
					"LightMode" = "ShadowCaster"
				}

				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_shadowcaster
				#include "UnityCG.cginc"

				struct v2f {
					V2F_SHADOW_CASTER;
				};

				v2f vert(appdata_base v)
				{
					v2f o;
					TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
					return o;
				}

				float4 frag(v2f i) : SV_Target
				{
					SHADOW_CASTER_FRAGMENT(i)
				}
				ENDCG
			}

				// Outline pass
				Pass
				{
					// Won't draw where it sees ref value 4
					Cull OFF
					ZWrite OFF
					ZTest ON
					Stencil
					{
						Ref 4
						Comp notequal
						Fail keep
						Pass replace
					}

					CGPROGRAM
					#pragma vertex vert
					#pragma fragment frag

					// Properties
					uniform float4 _OutlineColor;
					uniform float _OutlineSize;
					uniform float _OutlineExtrusion;
					uniform float _OutlineDot;

					struct vertexInput
					{
						float4 vertex : POSITION;
						float3 normal : NORMAL;
					};

					struct vertexOutput
					{
						float4 pos : SV_POSITION;
						float4 color : COLOR;
					};

					vertexOutput vert(vertexInput input)
					{
						vertexOutput output;

						float4 newPos = input.vertex;

						// normal extrusion technique
						float3 normal = normalize(input.normal);
						newPos += float4(normal, 0.0) * _OutlineExtrusion;

						// convert to world space
						output.pos = UnityObjectToClipPos(newPos);

						output.color = _OutlineColor;
						return output;
					}

					float4 frag(vertexOutput input) : COLOR
					{
						// checker value will be negative for 4x4 blocks of pixels
						// in a checkerboard pattern
						//input.pos.xy = floor(input.pos.xy * _OutlineDot) * 0.5;
						//float checker = -frac(input.pos.r + input.pos.g);

						// clip HLSL instruction stops rendering a pixel if value is negative
						//clip(checker);

						return input.color;
					}

					ENDCG
				}
		}
}