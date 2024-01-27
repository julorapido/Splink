Shader "Julorapido/Blur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Value ("Value", Float) = 1.0
        _Color("Color", Color) = (1,1,1,1)
    }
    SubShader // SUBSHADER
    {
        Tags { "RenderType"="Fade" }
        LOD 100

        Pass // ONE PASS FOR BLUR SHADER
        {
            CGPROGRAM
            #pragma vertex vert // vertex shaders calls => [ vert v2f vert (appdata v) ]
            #pragma fragment frag // vertex shaders calls => [ fixed4 frag (v2f i) : SV_Target ]

            // make fog work
            #pragma multi_compile_fog
            
            #include "UnityCG.cginc"
            
            float4 _Color;
            
            struct meshData{// per-vertex mesh data

                float4 vertex : POSITION; // vertex positions
                float3 normals : NORMAL; // mesh normals
                float2 uv : TEXCOORD0; // uv0 coords

                // float4 color : COLOR;
                // float4 tangent: TANGENT;
            };


            // data passed from the vertex shader to the fragment shader
            // this will interpolate/blend across the triangle!
            struct Interpolators{
                float4 vertex : SV_POSITION; // clip space position
                float3 normal : TEXCOORD0;
            };
    

            Interpolators vert (meshData v)
            {
                Interpolators o;
                o.vertex = UnityObjectToClipPos(v.vertex); // local space to clip space
                o.normal = v.normals; // just pass data
                return o;
            }
            
            // float4 => Vector[4] = new Vector[4]{0, 0, 0, 0}
            // float (32 bit float)
            // half (16 bit float)

            fixed4 frag (Interpolators i) : SV_Target
            {
                // return float4(_Value, 0, 0, 1); // Red
                // return _Color;
                return float4(i.normal, 1);
            }
            
            ENDCG
        }
    }
}