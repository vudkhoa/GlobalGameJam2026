Shader "Custom/WaveDistortion"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _HorizontalWave ("Horizontal Wave", Range(0, 10)) = 1.0
        _VerticalWave ("Vertical Wave", Range(0, 10)) = 1.0
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

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

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _HorizontalWave;
            float _VerticalWave;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                
                // Hardcoded constants for wave frequency and amplitude
                float waveFrequency = 5.0;
                float waveAmplitude = 0.05;
                
                // Apply horizontal wave distortion (affects X based on Y position)
                float horizontalOffset = sin(uv.y * waveFrequency * 3.14159) * waveAmplitude * _HorizontalWave;
                uv.x += horizontalOffset;
                
                // Apply vertical wave distortion (affects Y based on X position)
                float verticalOffset = sin(uv.x * waveFrequency * 3.14159) * waveAmplitude * _VerticalWave;
                uv.y += verticalOffset;
                
                // Clamp UV to prevent sampling outside texture
                uv = saturate(uv);
                
                // Sample the texture with distorted UVs
                fixed4 col = tex2D(_MainTex, uv);
                
                return col;
            }
            ENDCG
        }
    }
}
