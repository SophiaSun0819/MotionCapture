Shader "Custom/VRWaterSimple"
{
    Properties
    {
        _ColorDeep ("Deep Color", Color) = (0, 0.2, 0.4, 1)
        _ColorShallow ("Shallow Color", Color) = (0.2, 0.6, 0.8, 1)

        _NormalMap1 ("Normal Map 1", 2D) = "bump" {}
        _NormalMap2 ("Normal Map 2", 2D) = "bump" {}

        _NormalSpeed1 ("Normal Speed 1", Vector) = (0.1, 0.05, 0, 0)
        _NormalSpeed2 ("Normal Speed 2", Vector) = (-0.08, 0.07, 0, 0)

        _NormalStrength ("Normal Strength", Range(0, 2)) = 1

        _Smoothness ("Smoothness", Range(0,1)) = 0.8
        _Metallic ("Metallic", Range(0,1)) = 0.0

        _FresnelPower ("Fresnel Power", Range(1, 8)) = 4
        _FresnelStrength ("Fresnel Strength", Range(0, 1)) = 0.5

        _Alpha ("Transparency", Range(0,1)) = 0.8
        _WaveAmplitude ("Wave Height", Range(0,1)) = 0.1
        _WaveFrequency ("Wave Frequency", Range(0,10)) = 2
        _WaveSpeed ("Wave Speed", Range(0,5)) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 200

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
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
                float3 worldPos : TEXCOORD1;
                float3 viewDir : TEXCOORD2;
            };

            sampler2D _NormalMap1;
            sampler2D _NormalMap2;

            float4 _ColorDeep;
            float4 _ColorShallow;

            float4 _NormalSpeed1;
            float4 _NormalSpeed2;
            float _NormalStrength;

            float _Smoothness;
            float _Metallic;

            float _FresnelPower;
            float _FresnelStrength;

            float _Alpha;
            float _WaveAmplitude;
            float _WaveFrequency;
            float _WaveSpeed;

            Varyings vert (Attributes v)
        {
            Varyings o;

            float3 worldPos = TransformObjectToWorld(v.positionOS.xyz);

            // 🌊 波浪计算
            float wave = sin(worldPos.x * _WaveFrequency + _Time.y * _WaveSpeed)
                    + cos(worldPos.z * _WaveFrequency + _Time.y * _WaveSpeed);

            wave *= _WaveAmplitude;

            // 👉 修改高度
            worldPos.y += wave;

            o.positionHCS = TransformWorldToHClip(worldPos);

            o.worldPos = worldPos;
            o.uv = v.uv;

            float3 viewDir = GetWorldSpaceViewDir(worldPos);
            o.viewDir = viewDir;

            return o;
        }

            half4 frag (Varyings i) : SV_Target
            {
                float time = _Time.y;

                // 滚动UV
                float2 uv1 = i.uv + _NormalSpeed1.xy * time;
                float2 uv2 = i.uv + _NormalSpeed2.xy * time;

                // 采样法线
                float3 n1 = UnpackNormal(tex2D(_NormalMap1, uv1));
                float3 n2 = UnpackNormal(tex2D(_NormalMap2, uv2));

                float3 normal = normalize(n1 + n2);
                normal.xy *= _NormalStrength;

                // 视角方向
                float3 viewDir = normalize(i.viewDir);

                // Fresnel
                float fresnel = pow(1.0 - saturate(dot(viewDir, normal)), _FresnelPower);

                // 颜色混合
                float3 color = lerp(_ColorDeep.rgb, _ColorShallow.rgb, fresnel * _FresnelStrength);

                return float4(color, _Alpha);
            }
            ENDHLSL
        }
    }
}