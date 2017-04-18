Shader "Mobile/Boundary"
{
	Properties
	{
		 _Colour ("Color", Color) = (1,0,0,1)
		 _HighlightColour ("Highlight Color", Color) = (1,0,0,1)
		 _Density ("Density", Range(0,1)) = 0.4
		 _FullHeight  ("Full Height", Range(0,10)) = 1
         _TargetHeight ("Target Height", Range(0,10)) = 1
         _TargetAlpha ("Target Alpha", Range(0,1)) = 0.5
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

			float _Density;
			fixed4 _Colour;
			fixed4 _HighlightColour;
			float _FullHeight;
	        float _TargetHeight;
	        float _TargetAlpha;

			


			struct v2f
            {
                float4 pos : SV_POSITION;
             	float3 coords : TEXCOORD0;
            };
				
			v2f vert (float4 pos : POSITION, float4 uv : TEXCOORD0)
            {
                v2f o;
                o.pos = mul (UNITY_MATRIX_MVP, pos);
             	o.coords = pos.xyz * _Density;
                return o;
            }
			
			fixed4 frag (v2f input) : SV_Target
			{

				float _Middle = _TargetHeight;// + sin(input.coords.x + _Time[3]) * 0.1f;

				float distBottom = input.coords.y+0.5f;

				float4 localColour = lerp (_Colour, _HighlightColour, sin(input.coords.x + _Time[3])); 

				float4 _ColorBot = localColour;

				float4 _ColorMid = localColour;
				_ColorMid.a = _TargetAlpha;
				float4 _ColorTop = localColour;
				_ColorTop.a = 0.0f;

				fixed4 c = lerp(_ColorBot, _ColorMid, distBottom / _Middle) * step(distBottom, _Middle);
	            c += lerp(_ColorMid, _ColorTop, (distBottom - _Middle) / (_FullHeight - _Middle)) * step(_Middle, distBottom);
	            return c;
			}
			ENDCG
		}
	}
}
