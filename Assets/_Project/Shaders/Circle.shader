Shader "Custom/RangeCircleFull"
{
    Properties
    {
        _Thickness ("Thickness", Range(0.001,0.2)) = 0.01
        [IntRange]_Segments ("Segments", Range(1,64)) = 1
        _SegmentFill ("Segment Fill (0-1)", Range(0,1)) = 1
        _ActiveColor ("Active Color", Color) = (0,1,0,1)
        _InactiveColor ("Inactive Color", Color) = (1,0,0,1)
        [Toggle] _IsActive ("Is Active", Float) = 1
        _RadiusAdjust ("Radius Adjust", Range(0.0,0.5)) = 0.495
        _Alpha ("Global Alpha", Range(0,1)) = 1
        _RotationSpeed ("Rotation Speed", Range(0,1)) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Pass
        {
            Name "Unlit"
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off

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
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float _Thickness;
            int _Segments;
            float _SegmentFill;
            float4 _ActiveColor;
            float4 _InactiveColor;
            float _IsActive;
            float _RadiusAdjust;
            float _Alpha;
            float _RotationSpeed;

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                float2 center = float2(0.5, 0.5);
                float2 dir = IN.uv - center;
                float dist = length(dir);

                // çember hattı
                float edge = smoothstep(_RadiusAdjust - _Thickness, _RadiusAdjust, dist) *
                             (1.0 - smoothstep(_RadiusAdjust, _RadiusAdjust + _Thickness, dist));

                // açı 0..1 normalize
                float angle = atan2(dir.y, dir.x);

                // rotation ekle (sadece hız != 0 ise)
                if (abs(_RotationSpeed) > 0.0001)
                {
                    angle += _Time.y * _RotationSpeed;
                }

                angle = (angle + 3.14159265) / (2.0 * 3.14159265);

                // segment mask
                float segmentMask = 1.0;
                if (_Segments > 1 && _SegmentFill < 1.0)
                {
                    float segFrac = frac(angle * _Segments);
                    segmentMask = step(segFrac, _SegmentFill);
                }

                // renk seçimi
                float4 baseColor = (_IsActive > 0.5) ? _ActiveColor : _InactiveColor;

                // alpha: hem rengin kendi alpha’sı hem global alpha
                baseColor.a *= _Alpha;

                return baseColor * edge * segmentMask;
            }
            ENDHLSL
        }
    }
}
