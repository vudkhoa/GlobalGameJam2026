Shader "Custom/ScratchReveal"
{
    Properties
    {
        _MainTex ("Scratch Layer Texture", 2D) = "white" {}
        _MaskTex ("Mask Texture", 2D) = "black" {}
        _Color ("Tint Color", Color) = (1,1,1,1)
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
            sampler2D _MaskTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Sample the scratch layer texture (or use white if no texture)
                fixed4 texColor = tex2D(_MainTex, i.uv);
                
                // Apply color tint
                fixed4 col = texColor * _Color;
                
                // Sample the mask texture (red channel contains scratch data)
                fixed maskValue = tex2D(_MaskTex, i.uv).r;
                
                // Reduce alpha based on mask value (scratched areas become transparent)
                // maskValue: 0 = not scratched (show layer), 1 = scratched (hide layer)
                col.a *= (1.0 - maskValue);
                
                return col;
            }
            ENDCG
        }
    }
    
    FallBack "Sprites/Default"
}
