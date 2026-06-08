Shader "Game/UI/UIOutlineEx_NoAtlas"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	    _MainAlphaClip("MainTex Alpha Clip",float) = 0.001
		_OutlineColor1("Outline Color1",Color) = (1,1,1,1)
		_OutlineWidth1("Outline Width1",float) = 1
		_OutlineColor2("Outline Color2",Color) = (1,1,1,1)
		_OutlineWidth2("Outline Width2",float) = 1
	}

	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100
		Blend SrcAlpha OneMinusSrcAlpha

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
				float4 color : COLOR;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float2 _MainTex_TexelSize;
			float _MainAlphaClip;
			float _OutlineWidth1;
			float4 _OutlineColor1;
			float _OutlineWidth2;
			float4 _OutlineColor2;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.color = v.color;
				
				float outlineSize = _OutlineWidth1 + _OutlineWidth2;
				float2 scaler = 2 * outlineSize * _MainTex_TexelSize.xy + float2(1, 1);
				o.uv = (o.uv - float2(0.5, 0.5)) * scaler + float2(0.5, 0.5);
				
				return o;
			}

			//裁剪掉超出范围的像素
			fixed4 SampleMainTex2D(float2 uv)
			{
				fixed4 color = tex2D(_MainTex, uv);
				float2 inRect = step(float2(0, 0), uv) * step(uv, float2(1, 1));
				color.a *= inRect.x * inRect.y;
				return color;
			}

			//采样区域alpha值
			fixed2 SampleAlpha(int pIndex, v2f IN)
			{
				const fixed sinArray[12] = { 0, 0.5, 0.866, 1, 0.866, 0.5, 0, -0.5, -0.866, -1, -0.866, -0.5 };
				const fixed cosArray[12] = { 1, 0.866, 0.5, 0, -0.5, -0.866, -1, -0.866, -0.5, 0, 0.5, 0.866 };
				
				float2 pos1 = IN.uv + _MainTex_TexelSize.xy * float2(cosArray[pIndex], sinArray[pIndex]) * _OutlineWidth1;
				float2 pos2 = IN.uv + _MainTex_TexelSize.xy * float2(cosArray[pIndex], sinArray[pIndex]) * (_OutlineWidth1 + _OutlineWidth2);
				
				float alpha1 = step(0.001, SampleMainTex2D(pos1).a);
				float alpha2 = step(0.001, SampleMainTex2D(pos2).a);
				
				return fixed2(alpha1, alpha2);
			}

			fixed4 frag (v2f IN) : SV_Target
			{
				fixed4 color = SampleMainTex2D(IN.uv) * IN.color;

			    fixed2 outlineAlpha = SampleAlpha(0, IN);
				for (int i = 1; i < 12; i++)
				{
					outlineAlpha += SampleAlpha(i, IN);
				}

				outlineAlpha *= fixed2(_OutlineColor1.a, _OutlineColor2.a);

				half4 outlineColor1 = half4(_OutlineColor1.xyz, outlineAlpha.x);
				half4 outlineColor2 = half4(_OutlineColor2.xyz, outlineAlpha.y);
				half4 outlineColor = lerp(outlineColor2, outlineColor1, step(0.001,outlineAlpha.x));
				color = lerp(outlineColor, color, step(_MainAlphaClip, color.a));
				return color;
			}
			ENDCG
		}
	}
}
