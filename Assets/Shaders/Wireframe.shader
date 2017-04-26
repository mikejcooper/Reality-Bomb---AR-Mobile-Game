Shader "Mobile/Wireframe"
{
    Properties
    {
    	_MainTex("Base texture", 2D) = "white" {}
		_Density ("Density", Range(0,1)) = 0.5
        _CenterX ("CenterX", Float) = 0
        _CenterZ ("CenterZ", Float) = 0
        _Speed ("Speed", Float) = 3
        _PhaseLength ("Phase Length", Float) = 10
        _Alpha ("Alpha", Range(0,1)) = 0.6
        _PrimaryColour ("Primary Colour", Color) = (0.09, 0.78, 1, 1)
        _SecondaryColour ("Primary Colour", Color) = (0.09, 0.18, 1, 1)
        _BackgroundColour ("Background Colour", Color) = (1, 1, 1, 1)

    }
    SubShader
    {
        Pass
        {

        	Tags { "Queue"="Transparent" "RenderType"="Transparent" }

        	Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 coords : TEXCOORD1;
            };

            sampler2D _MainTex;
            float _Density;
            float _CenterX;
			float _CenterZ;
			float _Speed;
			float _PhaseLength;
			float _Alpha;
			float4 _PrimaryColour;
			float4 _SecondaryColour;
			float4 _BackgroundColour;

            v2f vert (float4 pos : POSITION, float2 uv : TEXCOORD0)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(pos);
                o.uv = pos.xz * _Density;
                o.coords = pos.xyz * _Density;
                return o;
            }
            
            fixed4 frag (v2f input) : SV_Target
            {
				
 
				// origin point
				float x = (input.coords.x - _CenterX) / 10.0f;
				float z = (input.coords.z - _CenterZ) / 10.0f;

				// degradation
				float r = sqrt (x*x + z*z) / _PhaseLength;

				// speed
				float inTime = r-_Time[1] * _Speed;

				float phase = sin (inTime);

//				float normalBlue = 0.78f;
//				float highlightBlue = 0.18f;
//				float thisBlue = normalBlue - phase * (normalBlue - highlightBlue);
				fixed4 c = 0;
                c.rgb = _PrimaryColour - phase * (_PrimaryColour - _SecondaryColour);// float3(0.09f, thisBlue, 1.0f);
                c.a = tex2D(_MainTex, input.uv).a;

                if (c.a > 0.1f) {
                	c.a = _Alpha;
                } else {
                	c = _BackgroundColour;
                }

                return c;


            }
            ENDCG
        }
    }
}