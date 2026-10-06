Shader "Universal Render Pipeline/2D/PixelOutline"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _OutlineThickness ("Outline Thickness", Float) = 1
        _EnableOutline ("Enable Outline", Float) = 0
    }
    SubShader
    {
        Tags
        { 
            "Queue"="Transparent" 
            "RenderType"="Transparent" 
            "RenderPipeline"="UniversalPipeline"
        }
        
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            // This line tells Unity to use URP's internal math library
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            
            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _OutlineColor;
                float _OutlineThickness;
                float _EnableOutline;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                // Translates the 2D position into Camera space using URP math
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.color = input.color * _Color;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Grab the normal pixel color
                half4 col = tex2D(_MainTex, input.uv) * input.color;
                
                // If the script turned the outline on...
                if (_EnableOutline > 0.5)
                {
                    // Check the 4 neighboring pixels for artwork
                    half upAlpha = tex2D(_MainTex, input.uv + float2(0, _MainTex_TexelSize.y * _OutlineThickness)).a;
                    half downAlpha = tex2D(_MainTex, input.uv - float2(0, _MainTex_TexelSize.y * _OutlineThickness)).a;
                    half rightAlpha = tex2D(_MainTex, input.uv + float2(_MainTex_TexelSize.x * _OutlineThickness, 0)).a;
                    half leftAlpha = tex2D(_MainTex, input.uv - float2(_MainTex_TexelSize.x * _OutlineThickness, 0)).a;

                    // If this pixel is empty space, but a neighbor has art, paint it White!
                    if (col.a < 0.1 && (upAlpha > 0.1 || downAlpha > 0.1 || rightAlpha > 0.1 || leftAlpha > 0.1))
                    {
                        return _OutlineColor;
                    }
                }
                
                return col;
            }
            ENDHLSL
        }
    }
}