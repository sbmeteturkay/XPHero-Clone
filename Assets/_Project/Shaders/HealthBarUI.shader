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
                float2 size = float2(0.4, 0.3);
                float bgRadius = _BorderRadius;
                
                // Background rounded rectangle
                float bgDist = roundedRectSDF(uv, size, bgRadius);
                float bgAlpha = 1.0 - smoothstep(-0.01, 0.01, bgDist);
                finalColor = lerp(finalColor, _BackgroundColor, bgAlpha);
                
                // Inner health bar area - padding ekleyerek kenarlardan uzak tutalım
                float2 innerSize = size - float2(_BorderWidth * 2, _BorderWidth * 2);
                float innerRadius = max(0.0, bgRadius - _BorderWidth);
                float innerDist = roundedRectSDF(uv, innerSize, innerRadius);
                float innerMask = 1.0 - smoothstep(-0.01, 0.01, innerDist);
                
                if (innerMask > 0.5)
                {
                    // Inner area'da UV koordinatlarını normalize et (0-1 arası)
                    float2 center = float2(0.5, 0.5);
                    float2 innerUV = (uv - center) / (innerSize * 2.0) + 0.5;
                    
                    // Sadece inner area içindeki UV'leri kullan
                    if (innerUV.x >= 0.0 && innerUV.x <= 1.0 && innerUV.y >= 0.0 && innerUV.y <= 1.0)
                    {
                        // Başlangıçta background color
                        half4 barColor = _BackgroundColor;
                        
                        // Health bar (kırmızı)
                        float healthMask = step(innerUV.x, healthPercent);
                        barColor = lerp(barColor, _HealthColor, healthMask);
                        
                        // Damage bar (beyaz) - health'in hemen yanından başlar
                        float damageStart = healthPercent;
                        float damageEnd = min(1.0, healthPercent + damagePercent);
                        float damageMask = step(damageStart, innerUV.x) * (1.0 - step(damageEnd, innerUV.x));
                        barColor = lerp(barColor, _DamageColor, damageMask);
                        
                        finalColor = lerp(finalColor, barColor, innerMask);
                    }
                }
                                
                return finalColor;
            }
            ENDHLSL
        }
    }
}