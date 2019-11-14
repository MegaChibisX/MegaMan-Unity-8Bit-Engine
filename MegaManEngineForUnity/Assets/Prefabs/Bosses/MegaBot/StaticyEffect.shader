// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//Copyright (c) 2014 Tilman Schmidt (@KeyMaster_)

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in
//all copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//THE SOFTWARE.

// Source: https://gist.github.com/KeyMaster-/363d3d5c35b956dfacdd

// I wanted to make a static effect myself, but realising I'm nearly useless without Amplify,
// it was time for the internet to help me with this effect. Thank you person!

Shader "MegaManBareBonesEngine/StaticyEffect"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}

		_Color("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
		_GlitchInterval("Glitch interval time [seconds]", Float) = 0.16
		_DispProbability("Displacement Glitch Probability", Float) = 0.022
		_DispIntensity("Displacement Glitch Intensity", Float) = 0.09
		_ColorProbability("Color Glitch Probability", Float) = 0.02
		_ColorIntensity("Color Glitch Intensity", Float) = 0.07
		[MaterialToggle] _WrapDispCoords("Wrap disp glitch (off = clamp)", Float) = 1
		[MaterialToggle] _DispGlitchOn("Displacement Glitch On", Float) = 1
		[MaterialToggle] _ColorGlitchOn("Color Glitch On", Float) = 1
	}

		SubShader
		{
			Tags
			{
				"Queue" = "Transparent"
				"IgnoreProjector" = "True"
				"RenderType" = "Transparent"
				"PreviewType" = "Plane"
				"CanUseSpriteAtlas" = "True"
			}

			Cull Off
			Lighting Off
			ZWrite Off
			Fog { Mode Off }
			Blend One OneMinusSrcAlpha

			Pass
			{
			CGPROGRAM
			// Upgrade NOTE: excluded shader from Xbox360; has structs without semantics (struct v2f members pos)
			#pragma exclude_renderers xbox360
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile DUMMY PIXELSNAP_ON
			#include "UnityCG.cginc"

			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color : COLOR;
				half2 texcoord  : TEXCOORD0;
			};

			fixed4 _Color;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;

				OUT.color = IN.color * _Color;
				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap(OUT.vertex);
				#endif
				return OUT;
			}

			sampler2D _MainTex;

			//Takes two values and returns a pseudo-random number between 0 (included) and 1 (excluded)
			//It samples the sin function, scales it up (presumably to increase floating point error) and then takes it's fraction part (to get value between 0 and 1)
			float rand(float x, float y) {
				return frac(sin(x*12.9898 + y * 78.233)*43758.5453);
			}

			float _GlitchInterval;
			float _DispIntensity;
			float _DispProbability;
			float _ColorIntensity;
			float _ColorProbability;
			float _DispGlitchOn;
			float _ColorGlitchOn;
			float _WrapDispCoords;
			fixed4 frag(v2f IN) : SV_Target
			{
				//This ensures that the shader only generates new random variables every [_GlitchInterval] seconds, e.g. every 0.5 seconds
				//During each interval the value wether the glitch occurs and how much the sprites glitches stays the same
				float intervalTime = floor(_Time.y / _GlitchInterval) * _GlitchInterval;

			//Second value increased by arbitrary number just to get more possible different random values
			float intervalTime2 = intervalTime + 2.793;

			//These values depend on time and the x/y translation of that sprite (top right and middle right value in the transformation matrix are translation)
			//The transformation matrix values are included so sprites with differen x/y values don't glitch at the same time
			float timePositionVal = intervalTime + UNITY_MATRIX_MV[0][3] + UNITY_MATRIX_MV[1][3];
			float timePositionVal2 = intervalTime2 + UNITY_MATRIX_MV[0][3] + UNITY_MATRIX_MV[1][3];

			//Random chance that the displacement glich or color glitch occur
			float dispGlitchRandom = rand(timePositionVal, -timePositionVal);
			float colorGlitchRandom = rand(timePositionVal, timePositionVal);

			//Precalculate color channel shift
			float rShiftRandom = (rand(-timePositionVal, timePositionVal) - 0.5) * _ColorIntensity;
			float gShiftRandom = (rand(-timePositionVal, -timePositionVal) - 0.5) * _ColorIntensity;
			float bShiftRandom = (rand(-timePositionVal2, -timePositionVal2) - 0.5) * _ColorIntensity;

			//For the displacement glitch, the sprite is divided into strips of 0.2 * sprite height (5 stripes)
			//This value is the random offset each of the strip boundries get either up or down
			//Without this, each strip would be exactly a 5th of the sprite height, with this their height is slightly randomised
			float shiftLineOffset = float((rand(timePositionVal2, timePositionVal2) - 0.5) / 50);

			//If the randomly rolled value is below the probability boundry and the displacement effect is turned on, apply the displacement effect
			if (dispGlitchRandom < _DispProbability && _DispGlitchOn == 1) {
				IN.texcoord.x += (rand(floor(IN.texcoord.y / (0.2 + shiftLineOffset)) - timePositionVal, floor(IN.texcoord.y / (0.2 + shiftLineOffset)) + timePositionVal) - 0.5) * _DispIntensity;
				//Prevent the texture coordinate from going into other parts of the texture, especially when using texture atlases
				//Instead, loop the coordinate between 0 and 1
				if (_WrapDispCoords == 1) {
					IN.texcoord.x = fmod(IN.texcoord.x, 1);
				}
				else {
					IN.texcoord.x = clamp(IN.texcoord.x, 0, 1);
				}
			}

			//Sample the texture at the normal position and at the shifted color channel positions
			fixed4 normalC = tex2D(_MainTex, IN.texcoord);
			fixed4 rShifted = tex2D(_MainTex, float2(IN.texcoord.x + rShiftRandom, IN.texcoord.y + rShiftRandom));
			fixed4 gShifted = tex2D(_MainTex, float2(IN.texcoord.x + gShiftRandom, IN.texcoord.y + gShiftRandom));
			fixed4 bShifted = tex2D(_MainTex, float2(IN.texcoord.x + bShiftRandom, IN.texcoord.y + bShiftRandom));

			fixed4 c = fixed4(0.0,0.0,0.0,0.0);

			//If the randomly rolled value is below the probability boundry and the color effect is turned on, apply the color glitch effect
			//Sets the output color to the shifted r,g,b channels and averages their alpha
			if (colorGlitchRandom < _ColorProbability && _ColorGlitchOn == 1) {
				c.r = rShifted.r;
				c.g = gShifted.g;
				c.b = bShifted.b;
				c.a = (rShifted.a + gShifted.a + bShifted.a) / 3;
			}
			else {
				c = normalC;
			}
			//Apply tint and tint color alpha
			c.rgb *= IN.color;
			c.a *= IN.color.a;
			c.rgb *= c.a;
			return c;
		}
	ENDCG
	}
		}
			SubShader
		{
			Tags
			{
				"Queue" = "Transparent"
				"IgnoreProjector" = "True"
				"RenderType" = "Transparent"
				"PreviewType" = "Plane"
				"CanUseSpriteAtlas" = "True"
			}

			Cull Off
			Lighting Off
			ZWrite Off
			Fog { Mode Off }
			Blend One OneMinusSrcAlpha

			Pass
			{
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile DUMMY PIXELSNAP_ON
				#include "UnityCG.cginc"

				struct appdata_t
				{
					float4 vertex   : POSITION;
					float4 color    : COLOR;
					float2 texcoord : TEXCOORD0;
				};

				struct v2f
				{
					float4 vertex   : SV_POSITION;
					fixed4 color : COLOR;
					half2 texcoord  : TEXCOORD0;
				};

				fixed4 _Color;

				v2f vert(appdata_t IN)
				{
					v2f OUT;
					OUT.vertex = UnityObjectToClipPos(IN.vertex);
					OUT.texcoord = IN.texcoord;

					OUT.color = IN.color * _Color;
					#ifdef PIXELSNAP_ON
					OUT.vertex = UnityPixelSnap(OUT.vertex);
					#endif
					return OUT;
				}

				sampler2D _MainTex;

				float rand(float x, float y) {
					return frac(sin(x*12.9898 + y * 78.233)*43758.5453);
				}

				float _DispIntensity;
				float _DispProbability;
				float _GlitchInterval;
				float _DispGlitchOn;
				float _WrapDispCoords;

				fixed4 frag(v2f IN) : SV_Target
				{
					float intervalTime = floor(_Time.y / _GlitchInterval) * _GlitchInterval;
					float timePositionVal = float(intervalTime + UNITY_MATRIX_MV[0][3] + UNITY_MATRIX_MV[1][3]);
					float timeRandom = rand(timePositionVal, -timePositionVal);
					if (timeRandom < _DispProbability && _DispGlitchOn == 1) {
						IN.texcoord.x += (rand(floor(IN.texcoord.y / 0.2) - intervalTime, floor(IN.texcoord.y / 0.2) + intervalTime) - 0.5) * _DispIntensity;
						if (_WrapDispCoords == 1) {
							IN.texcoord.x = fmod(IN.texcoord.x, 1);
						}
						else {
							IN.texcoord.x = clamp(IN.texcoord.x, 0, 1);
						}
					}
					fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;

					c.a *= IN.color.a;
					c.rgb *= c.a;
					return c;
				}
			ENDCG
			}
		}
}