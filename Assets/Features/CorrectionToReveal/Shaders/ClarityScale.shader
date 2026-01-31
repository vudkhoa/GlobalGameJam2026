Shader "Custom/ClarityScale"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        
        // Parameter 1: Làm mờ hình ảnh
        _BlurAmount ("Blur Amount (Độ mờ)", Range(0, 10)) = 0.0
        
        // Parameter 2: Scale theo phương ngang
        _HorizontalScale ("Horizontal Scale (Scale ngang)", Range(1, 3)) = 1.0
    }
    
    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "RenderType"="Transparent" 
            "IgnoreProjector"="True"
        }
        
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
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            
            // Parameter 1: Blur
            float _BlurAmount;
            
            // Parameter 2: Horizontal Scale
            float _HorizontalScale;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
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
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Step 1: Apply horizontal scale to UV
                float2 scaledUV = applyHorizontalScale(i.uv);
                
                // Check if UV is out of bounds after scaling
                if (scaledUV.x < 0.0 || scaledUV.x > 1.0)
                {
                    // Return transparent or edge color for out of bounds
                    return fixed4(0, 0, 0, 0);
                }
                
                // Step 2: Apply blur with scaled UV
                fixed4 finalColor = applyBlur(scaledUV);
                
                return finalColor;
            }
            ENDCG
        }
    }
    
    FallBack "Sprites/Default"
}
