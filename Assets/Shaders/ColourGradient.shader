Shader "Mobile/ColourGradient"
{
	Properties
	{
		 _StartColour ("Color", Color) = (1,0,0,1)
		 _EndColour ("Color", Color) = (0,1,0,1)


	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			Tags { "Queue"="Transparent" "RenderType"="Transparent" }

        	Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"


			fixed4 _StartColour;
			fixed4 _EndColour;

			struct v2f
            {
             	float2 uv : TEXCOORD0;
             	float4 pos : SV_POSITION;
            };
				
			v2f vert (float4 pos : POSITION, float4 uv : TEXCOORD0)
            {
                v2f o;
                o.pos = mul (UNITY_MATRIX_MVP, pos);
             	o.uv = uv;
                return o;
            }
			
			fixed4 frag (v2f input) : SV_Target
			{
				return lerp(_StartColour, _EndColour, input.uv.y);
			}
			ENDCG
		}
	}
}
