Shader "Custom/HealthBar"
{
    Properties
    {
        _BackgroundColor ("Background Color", Color) = (0, 0, 0, 0.7)
        _HealthColor ("Health Color", Color) = (1, 0, 0, 1)
        _DamageColor ("Damage Color", Color) = (1, 1, 1, 0.8)
        _BorderRadius ("Border Radius", Range(0, 0.5)) = 0.1
        _BorderWidth ("Border Width", Range(0, 0.1)) = 0.02
    }
    
    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "RenderType"="Transparent" 
            "RenderPipeline"="UniversalPipeline"
        }
        LOD 100
        
        Pass
        {
            Name "HealthBarPass"
            Tags { "LightMode"="UniversalForward" }
            
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include <HLSLSupport.cginc>

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionHCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            // Properties
            CBUFFER_START(UnityPerMaterial)
                half4 _BackgroundColor;
                half4 _HealthColor;
                half4 _DamageColor;
                float _BorderRadius;
                float _BorderWidth;
            CBUFFER_END
            
            // Instanced properties
            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float4, _HealthData) // x=health%, y=damage%, z=unused, w=unused
            UNITY_INSTANCING_BUFFER_END(Props)
            
            Varyings vert (Attributes input)
            {
                Varyings output;
                
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                
                return output;
            }
            
            // Rounded rectangle SDF
            float roundedRectSDF(float2 uv, float2 size, float radius)
            {
                float2 d = abs(uv - 0.5) - size + radius;
                return length(max(d, 0.0)) + min(max(d.x, d.y), 0.0) - radius;
            }
            
            half4 frag (Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                
                float4 healthData = UNITY_ACCESS_INSTANCED_PROP(Props, _HealthData);
                float healthPercent = healthData.x;
                float damagePercent = healthData.y;
                
                float2 uv = input.uv;
                half4 finalColor = half4(0, 0, 0, 0);
                
                // Rounded rectangle parameters
                float2 size = float2(0.4, 0.3); // Health bar inner size
                float bgRadius = _BorderRadius;
                
                // Background (siyah rounded rectangle)
                float bgDist = roundedRectSDF(uv, size, bgRadius); // Inner area'yı kaplasın
                float bgAlpha = 1.0 - smoothstep(-0.01, 0.01, bgDist);
                finalColor = lerp(finalColor, _BackgroundColor, bgAlpha);
                
                // Inner area
                float innerDist = roundedRectSDF(uv, size, bgRadius);
                float innerMask = 1.0 - smoothstep(-0.01, 0.01, innerDist);
                
                if (innerMask > 0.5)
                {
                    // İlk önce tüm inner area'yı background color ile doldur
                    fixed4 barColor = _BackgroundColor;
                    
                    // Health bar (kırmızı) - background'un üzerine
                    float healthWidth = healthPercent;
                    float healthMask = step(uv.x, healthWidth);
                    barColor = lerp(barColor, _HealthColor, healthMask);
                    
                    // Damage bar (beyaz) - health bar'ın üzerine
                    float damageStart = healthPercent;
                    float damageEnd = min(1.0, healthPercent + damagePercent);
                    float damageMask = step(damageStart, uv.x) * step(uv.x, damageEnd);
                    barColor = lerp(barColor, _DamageColor, damageMask);
                    
                    // Final color'u inner mask ile blend et
                    finalColor = lerp(finalColor, barColor, innerMask);
                }
                                
                return finalColor;
            }
            ENDHLSL
        }
    }
}