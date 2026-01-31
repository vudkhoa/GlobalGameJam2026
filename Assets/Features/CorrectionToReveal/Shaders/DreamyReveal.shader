Shader "Custom/DreamyReveal"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        
        [Header(Correction Controls)]
        [Range(0, 1)] _BlurAmount ("Blur Amount", Float) = 0.5
        [Range(0, 1)] _DistortionAmount ("Distortion Amount", Float) = 0.5
    }
    
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        
        Blend SrcAlpha OneMinusSrcAlpha
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

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _BlurAmount;
            float _DistortionAmount;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }

            // Smooth noise function for dreamy distortion
            float2 hash22(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * float3(0.1031, 0.1030, 0.0973));
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.xx + p3.yz) * p3.zy);
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                
                // Smooth interpolation
                f = f * f * (3.0 - 2.0 * f);
                
                float2 a = hash22(i);
                float2 b = hash22(i + float2(1.0, 0.0));
                float2 c = hash22(i + float2(0.0, 1.0));
                float2 d = hash22(i + float2(1.0, 1.0));
                
                return lerp(lerp(a.x, b.x, f.x), lerp(c.x, d.x, f.x), f.y);
            }

            // Multi-octave noise for smoother effect
            float fbm(float2 p)
            {
                float value = 0.0;
                float amplitude = 0.5;
                float frequency = 1.0;
                
                for(int i = 0; i < 4; i++)
                {
                    value += amplitude * noise(p * frequency);
                    frequency *= 2.0;
                    amplitude *= 0.5;
                }
                
                return value;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                
                // Apply dreamy distortion
                float2 distortion = float2(
                    fbm(uv * 3.0 + _Time.y * 0.1),
                    fbm(uv * 3.0 + _Time.y * 0.1 + 100.0)
                ) * 2.0 - 1.0;
                
                // Scale distortion by amount (0 = no distortion, 1 = full distortion)
                uv += distortion * _DistortionAmount * 0.05;
                
                // Multi-tap blur for smooth dreamy effect
                fixed4 col = fixed4(0, 0, 0, 0);
                float blurSize = _BlurAmount * 0.02;
                
                // 9-tap blur kernel
                const int samples = 9;
                float2 offsets[9] = {
                    float2(-1, -1), float2(0, -1), float2(1, -1),
                    float2(-1,  0), float2(0,  0), float2(1,  0),
                    float2(-1,  1), float2(0,  1), float2(1,  1)
                };
                
                float weights[9] = {
                    0.0625, 0.125, 0.0625,
                    0.125,  0.25,  0.125,
                    0.0625, 0.125, 0.0625
                };
                
                for(int j = 0; j < samples; j++)
                {
                    float2 offset = offsets[j] * blurSize;
                    col += tex2D(_MainTex, uv + offset) * weights[j];
                }
                
                // Apply vertex color (for UI tinting)
                col *= i.color;
                
                return col;
            }
            ENDCG
        }
    }
    
    FallBack "UI/Default"
}
