Shader "Custom/BitSwitchPBR"
{
    Properties
    {
        _BitValue ("Bit Value (0=Texture0, 1+=Texture1)", Int) = 0
        _BitAlpha ("Alpha", Range(0, 1)) = 1.0
        
        _MainTex0 ("Diffuse Texture 0", 2D) = "white" {}
        _MainTex1 ("Diffuse Texture 1", 2D) = "white" {}
        
        _Color ("Color", Color) = (1, 1, 1, 1)
        
        [HDR] _EmissionColor ("Emission Color", Color) = (0, 0, 0, 1)
        _EmissionStrength ("Emission Strength", Range(0, 10)) = 1.0
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent"
            "RenderPipeline"="UniversalPipeline"
        }
        LOD 100
        
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex0);
            SAMPLER(sampler_MainTex0);
            TEXTURE2D(_MainTex1);
            SAMPLER(sampler_MainTex1);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex0_ST;
                float4 _MainTex1_ST;
                int _BitValue;
                float _BitAlpha;
                float4 _Color;
                float4 _EmissionColor;
                float _EmissionStrength;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 albedo;
                if (_BitValue <= 0)
                    albedo = SAMPLE_TEXTURE2D(_MainTex0, sampler_MainTex0, input.uv);
                else
                    albedo = SAMPLE_TEXTURE2D(_MainTex1, sampler_MainTex1, input.uv);
                
                albedo *= _Color;

                half3 color = albedo.rgb + _EmissionColor.rgb * _EmissionStrength;
                half alpha = albedo.a * _BitAlpha;
                
                return half4(color, alpha);
            }
            ENDHLSL
        }
    }
}