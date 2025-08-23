Shader "Unlit/TextureBlend"
{
    Properties
    {
        _MainTex ("Day Texture", 2D) = "white" {}
        _SecondTex ("Night Texture", 2D) = "black" {}
        _Fade ("Fade", Range(0.0, 1.0)) = 0.0
        // This hidden property ensures Unity passes the tiling/offset data for the second texture.
        [HideInInspector] _SecondTex_ST ("Second Texture Tiling/Offset", Vector) = (1,1,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Background" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            // --- DATA STRUCTURES ---

            // Input from the mesh
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            // Data passed from the vertex shader to the fragment shader
            struct v2f
            {
                float2 uv_main   : TEXCOORD0; // UVs for the Day Texture
                float2 uv_second : TEXCOORD1; // UVs for the Night Texture
                float4 vertex    : SV_POSITION;
            };

            // --- SHADER VARIABLES ---

            sampler2D _MainTex;
            sampler2D _SecondTex;
            float4 _MainTex_ST;
            float4 _SecondTex_ST; // Variable to receive tiling/offset for the second texture
            float _Fade;

            // --- VERTEX SHADER ---
            // This runs for every vertex of the mesh
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                // Transform the input UVs using the tiling/offset data for EACH texture
                o.uv_main = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv_second = TRANSFORM_TEX(v.uv, _SecondTex);
                return o;
            }
            
            // --- FRAGMENT (PIXEL) SHADER ---
            // This runs for every pixel on the screen covered by the mesh
            fixed4 frag (v2f i) : SV_Target
            {
                // Sample (get the color of) each texture using its own, correct UV coordinates
                fixed4 col1 = tex2D(_MainTex, i.uv_main);
                fixed4 col2 = tex2D(_SecondTex, i.uv_second);
                
                // Blend the two colors together based on the fade amount
                return lerp(col1, col2, _Fade);
            }
            ENDCG
        }
    }
}