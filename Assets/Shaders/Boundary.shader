Shader "Mobile/Boundary"
{
	Properties
	{
		 _Colour ("Color", Color) = (1,0,0,1)
		 _HighlightColour ("Highlight Color", Color) = (1,0,0,1)

         _EndHeight ("End Height", Range(0,1)) = 1
         _StartAlpha ("Start Alpha", Range(0,1)) = 1
         _EndAlpha ("End Alpha", Range(0,1)) = 0.5
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


			fixed4 _Colour;
			fixed4 _HighlightColour;

	        float _EndHeight;
	        float _StartAlpha;
	        float _EndAlpha;


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
				float localStartAlpha = _StartAlpha;// +  0.2 * (1 + sin(_Time[3] + input.uv.x));
				float localEndAlpha = _EndAlpha;// + sin(_Time[3]);
				float localEndHeight = _EndHeight +  0.1 * (1 + sin(_Time[3] + input.uv.x*5));
				fixed4 alpha = lerp(localStartAlpha, localEndAlpha, min(1, input.uv.y / localEndHeight));
				float4 c = lerp(_Colour, _HighlightColour, 0.5 + 0.5*sin(_Time[3] + input.uv.x));
				c.a = alpha;
	            return c;
			}
			ENDCG
		}
	}
}
