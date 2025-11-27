Shader "Hidden/FoveatedRendering"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _GazePos("Gaze Position", Vector) = (0.5,0.5,0,0)
        _FoveaRadius("Fovea Radius", Float) = 0.1
        _TransitionSize("Transition Size", Float) = 0.1
        _MaxBlurRadius("Max Blur Radius", Float) = 3.0
    }

    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

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
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            float4 _GazePos;
            float _FoveaRadius;
            float _MaxBlurRadius;
            float _TransitionSize;
            float4 _MainTex_TexelSize; // x = 1/width, y = 1/height

            float2 diskKernel16[16];

            void InitKernel()
            {
                diskKernel16[0]  = float2( 0.00,  0.00);
                diskKernel16[1]  = float2( 0.50,  0.00);
                diskKernel16[2]  = float2(-0.50,  0.00);
                diskKernel16[3]  = float2( 0.00,  0.50);
                diskKernel16[4]  = float2( 0.00, -0.50);
                diskKernel16[5]  = float2( 0.35,  0.35);
                diskKernel16[6]  = float2( 0.35, -0.35);
                diskKernel16[7]  = float2(-0.35,  0.35);
                diskKernel16[8]  = float2(-0.35, -0.35);
                diskKernel16[9]  = float2( 0.80,  0.00);
                diskKernel16[10] = float2(-0.80,  0.00);
                diskKernel16[11] = float2( 0.00,  0.80);
                diskKernel16[12] = float2( 0.00, -0.80);
                diskKernel16[13] = float2( 0.55,  0.55);
                diskKernel16[14] = float2( 0.55, -0.55);
                diskKernel16[15] = float2(-0.55,  0.55);
                // optional: last one -0.55, -0.55 if needed
            }


            float4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;

                // Distance of pixel from gaze point
                float dist = distance(uv, _GazePos.xy);

                // Blur factor 0 = sharp, 1 = max blur
                float blurT = saturate((dist - _FoveaRadius) / _TransitionSize);

                float blurRadius = blurT * _MaxBlurRadius;

                // Simple blur: 8-tap sampling
                float2 texel = _MainTex_TexelSize.xy * blurRadius;

                fixed4 sum = 0;
                InitKernel();
                // 16-sample disk blur
                [unroll]
                for (int n = 0; n < 16; n++)
                {
                    float2 offset = diskKernel16[n] * texel;
                    sum += tex2D(_MainTex, uv + offset);
                }

                return sum / 16;
            }
            ENDCG
        }
    }
}