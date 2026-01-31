Shader "UI/ClarityScale"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        
        // Parameter 1: Làm mờ hình ảnh
        _BlurAmount ("Blur Amount (Độ mờ)", Range(0, 10)) = 0.0
        
        // Parameter 2: Scale theo phương ngang
        _HorizontalScale ("Horizontal Scale (Scale ngang)", Range(1, 3)) = 1.0
        
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
            Name "Default"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

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
            float4 _MainTex_TexelSize;
            float _BlurAmount;
            float _HorizontalScale;

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

            // Apply horizontal scale to UV coordinates
            float2 applyHorizontalScale(float2 uv)
            {
                // Center the UV around 0.5
                float2 centeredUV = uv - 0.5;
                
                // Scale the X coordinate
                centeredUV.x /= _HorizontalScale;
                
                // Move back to 0-1 range
                return centeredUV + 0.5;
            }
            
            // Apply Gaussian blur
            fixed4 applyBlur(float2 uv)
            {
                if (_BlurAmount < 0.01) return tex2D(_MainTex, uv);
                
                float2 texelSize = _MainTex_TexelSize.xy * _BlurAmount;
                fixed4 result = fixed4(0, 0, 0, 0);
                float totalWeight = 0.0;
                
                // Gaussian blur kernel (5x5)
                for (int x = -2; x <= 2; x++)
                {
                    for (int y = -2; y <= 2; y++)
                    {
                        float2 offset = float2(x, y) * texelSize;
                        
                        // Gaussian weight
                        float weight = exp(-(x*x + y*y) / 2.0);
                        
                        result += tex2D(_MainTex, uv + offset) * weight;
                        totalWeight += weight;
                    }
                }
                
                result /= totalWeight;
                return result;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // Step 1: Apply horizontal scale to UV
                float2 scaledUV = applyHorizontalScale(IN.texcoord);
                
                // Check if UV is out of bounds after scaling
                if (scaledUV.x < 0.0 || scaledUV.x > 1.0)
                {
                    return fixed4(0, 0, 0, 0);
                }
                
                // Step 2: Apply blur with scaled UV
                half4 color = (applyBlur(scaledUV) + _TextureSampleAdd) * IN.color;

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
