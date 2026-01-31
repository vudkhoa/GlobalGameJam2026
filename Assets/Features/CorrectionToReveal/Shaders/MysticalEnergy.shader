Shader "UI/MysticalEnergy"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        
        // Parameter 1: Darkness - Độ tối ma mị
        _Darkness ("Darkness (Độ tối)", Range(0, 1)) = 0.5
        
        // Parameter 2: Shadow Distortion - Biến dạng bóng tối
        _ShadowDistortion ("Shadow Distortion (Biến dạng)", Range(0, 1)) = 0.5
        
        // Parameter 3: Vignette - Viền tối xung quanh
        _VignetteStrength ("Vignette (Viền tối)", Range(0, 1)) = 0.5
        
        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
        [HideInInspector] [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "MysticalEnergy"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;
            float _Darkness;
            float _ShadowDistortion;
            float _VignetteStrength;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.color = v.color;
                return OUT;
            }

            // Hash function for procedural noise
            float hash(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            // Smooth noise
            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                
                float a = hash(i);
                float b = hash(i + float2(1.0, 0.0));
                float c = hash(i + float2(0.0, 1.0));
                float d = hash(i + float2(1.0, 1.0));
                
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            // Fractal Brownian Motion for complex shadow patterns
            float fbm(float2 p)
            {
                float value = 0.0;
                float amplitude = 0.5;
                
                for(int i = 0; i < 4; i++)
                {
                    value += amplitude * noise(p);
                    p *= 2.0;
                    amplitude *= 0.5;
                }
                
                return value;
            }

            // Swirling shadow pattern
            float2 swirlDistortion(float2 uv, float strength)
            {
                if (strength < 0.001) return uv;
                
                float2 center = float2(0.5, 0.5);
                float2 delta = uv - center;
                float dist = length(delta);
                float angle = atan2(delta.y, delta.x);
                
                // Create swirling effect
                float swirl = fbm(uv * 3.0 + _Time.y * 0.1) * 6.28318;
                angle += swirl * strength * 0.5;
                
                // Reconstruct position
                float2 newDelta = float2(cos(angle), sin(angle)) * dist;
                return center + newDelta;
            }

            // Vignette effect
            float vignette(float2 uv, float strength)
            {
                if (strength < 0.001) return 1.0;
                
                float2 center = uv - 0.5;
                float dist = length(center);
                
                // Smooth vignette falloff
                float vignette = smoothstep(0.8, 0.3, dist);
                return lerp(1.0, vignette, strength);
            }

            // Darkness overlay with mystical patterns
            float3 applyDarkness(float3 color, float2 uv, float strength)
            {
                if (strength < 0.001) return color;
                
                // Create mystical shadow patterns
                float shadowPattern = fbm(uv * 5.0 + _Time.y * 0.05);
                shadowPattern = pow(shadowPattern, 2.0);
                
                // Flowing dark tendrils
                float tendrils = fbm(uv * 8.0 + float2(_Time.y * 0.1, -_Time.y * 0.15));
                tendrils = smoothstep(0.3, 0.7, tendrils);
                
                // Combine shadow effects
                float darknessMask = lerp(shadowPattern, tendrils, 0.5);
                
                // Apply darkness with purple/blue tint for mystical feel
                float3 darkColor = float3(0.05, 0.02, 0.15); // Deep purple-blue
                float3 darkenedColor = lerp(color, color * darkColor, darknessMask * strength);
                
                // Overall darkening
                darkenedColor *= lerp(1.0, 0.3, strength);
                
                return darkenedColor;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 uv = IN.texcoord;
                
                // === SHADOW DISTORTION ===
                // Apply swirling distortion based on parameter
                float2 distortedUV = swirlDistortion(uv, _ShadowDistortion);
                
                // Sample texture with distorted UV
                fixed4 color = tex2D(_MainTex, distortedUV);
                
                // === DARKNESS ===
                // Apply mystical darkness overlay
                color.rgb = applyDarkness(color.rgb, uv, _Darkness);
                
                // === VIGNETTE ===
                // Apply dark vignette around edges
                float vignetteValue = vignette(uv, _VignetteStrength);
                color.rgb *= vignetteValue;
                
                // Add subtle purple glow in shadows when parameters are active
                if (_Darkness > 0.01 || _ShadowDistortion > 0.01 || _VignetteStrength > 0.01)
                {
                    float glowPattern = fbm(uv * 10.0 + _Time.y * 0.2);
                    glowPattern = pow(glowPattern, 3.0);
                    
                    float3 mysticalGlow = float3(0.3, 0.1, 0.5) * glowPattern * 0.15;
                    float glowStrength = (_Darkness + _ShadowDistortion + _VignetteStrength) / 3.0;
                    color.rgb += mysticalGlow * glowStrength;
                }
                
                // Apply vertex color
                color *= IN.color;
                color += _TextureSampleAdd;

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

                return color;
            }
        ENDCG
        }
    }
}
