Shader "Hidden/CircularBrush"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Seed ("Seed", Float) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        
        // Blend mode is overridden by MaskRenderer.cs but good to have defaults
        Blend One One
        ZWrite Off
        Cull Off

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

            float _Seed;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 center = float2(0.5, 0.5);
                float dist = distance(i.uv, center);
                
                // Hard circular limit
                if (dist > 0.5) discard;

                // Hash function for random noise
                // Use _Seed to offset noise pattern per stroke
                float2 noiseUV = i.uv * 10.0 + float2(_Seed, _Seed * 0.5);
                float noise = frac(sin(dot(noiseUV, float2(12.9898, 78.233))) * 43758.5453);
                
                // soft circle falloff for density
                // Edge has less probability of grain than center
                float density = 1.0 - smoothstep(0.2, 0.5, dist);
                
                // Grain mask: 1 if noise < density, else 0
                // This makes it look like sand/grains being added
                float grain = step(1.0 - density, noise);
                
                return i.color * grain;
            }
            ENDCG
        }
    }
}
