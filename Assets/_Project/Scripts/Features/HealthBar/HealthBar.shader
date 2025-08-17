Shader "Custom/URP_HealthBar_Instanced_Size"
{
    Properties
    {
        _MainTex ("Bar Texture", 2D) = "white" {}
        _BackColor ("Back Color", Color) = (0,0,0,1)
        _FillColor ("Fill Color", Color) = (1,0,0,1)
        _Size ("Size (Width, Height)", Vector) = (1.5, 0.2, 0, 0)
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Pass
        {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float, _Fill)
            UNITY_INSTANCING_BUFFER_END(Props)

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _BackColor;
            float4 _FillColor;
            float2 _Size;

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);

                float fill = UNITY_ACCESS_INSTANCED_PROP(Props, _Fill);

                float3 centerWS = mul(GetObjectToWorldMatrix(), float4(0,0,0,1)).xyz;
                float3 camPosWS = GetCameraPositionWS();
                float3 forward = normalize(camPosWS - centerWS);
                float3 up = float3(0,1,0);
                float3 right = normalize(cross(up, forward));
                up = cross(forward, right);

                float3 localPos = float3(IN.positionOS.x * _Size.x, IN.positionOS.y * _Size.y, 0);
                float3 worldPos = centerWS + right * localPos.x + up * localPos.y;

                OUT.positionCS = TransformWorldToHClip(worldPos);
                OUT.uv = IN.uv;

                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);

                float fill = UNITY_ACCESS_INSTANCED_PROP(Props, _Fill);
                float4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);

                if (IN.uv.x > fill)
                    texColor.rgb = _BackColor.rgb;
                else
                    texColor.rgb = _FillColor.rgb;

                texColor.a *= 1.0;
                return texColor;
            }
            ENDHLSL
        }
    }
}
