Shader "Custom/SpherePortal_Transparent"
{
    Properties
    {
        [Header(Interaction Settings)]
        _Position("World Position", Vector) = (0, 0, 0, 0)
        _Radius("Radius", Float) = 1.0
        
        [Header(Noise Settings)]
        [NoScaleOffset]_noise_texture("Noise Texture (Triplanar)", 2D) = "white" {}
        _noise_scale("Noise Scale", Float) = 2
        _moveSpeed("Noise Move Speed", Float) = 0.5
        _noise_Strengh("Edge Distortion Strength", Range(0, 1)) = 0.5

        [Header(Edge Settings)]
        _edge_width("Edge Glow Width", Range(0, 0.5)) = 0.1
        [HDR]_edge_color("Edge Color", Color) = (0, 1, 1, 1)

        [Header(Visuals)]
        [NoScaleOffset]_secondary_texture("Main Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent"
            "Queue"="Transparent"
        }
        
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off 

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings {
                float4 positionCS : SV_POSITION;
                float3 positionWS : INTERP0;
                float3 normalWS : INTERP1;
                float2 uv : INTERP2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _Position; // 使用 float4 兼容 Vector
                float _Radius;
                float _noise_scale;
                float _moveSpeed;
                float _noise_Strengh;
                float _edge_width;
                float4 _edge_color;
            CBUFFER_END

            TEXTURE2D(_noise_texture); SAMPLER(sampler_noise_texture);
            TEXTURE2D(_secondary_texture); SAMPLER(sampler_secondary_texture);

            // 三平面噪声，确保球体表面扭曲均匀
            float4 SampleTriplanarNoise(float3 worldPos, float3 worldNormal) {
                float3 blending = abs(worldNormal);
                blending /= (blending.x + blending.y + blending.z);
                float time = _Time.y * _moveSpeed;
                float2 uvX = worldPos.zy * _noise_scale + time;
                float2 uvY = worldPos.xz * _noise_scale + time;
                float2 uvZ = worldPos.xy * _noise_scale + time;
                return SAMPLE_TEXTURE2D(_noise_texture, sampler_noise_texture, uvX) * blending.x +
                       SAMPLE_TEXTURE2D(_noise_texture, sampler_noise_texture, uvY) * blending.y +
                       SAMPLE_TEXTURE2D(_noise_texture, sampler_noise_texture, uvZ) * blending.z;
            }

            Varyings vert(Attributes input) {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = input.uv;
                return output;
            }

            float4 frag(Varyings input) : SV_Target {
                // 1. 计算距离 field
                float dist = distance(input.positionWS, _Position.xyz);
                
                // 防止半径为0时除以0导致的闪烁，并修正 0 半径时的判定
                float safeRadius = max(0.001, _Radius);
                float maskRaw = saturate(dist / safeRadius);

                // 2. 噪声扭曲
                float4 noise = SampleTriplanarNoise(input.positionWS, input.normalWS);
                // (1-maskRaw) 确保扭曲只发生在圆圈边缘，中心保持圆滑
                float distortedMask = lerp(maskRaw, noise.r, _noise_Strengh * (1 - maskRaw));
                
                // 3. 判定：1 为内部透明，0 为外部材质
                // 增加 step(_Radius, 0.001) 的反向判定，确保半径为0时完全不透明
                float finalSwitch = step(distortedMask, 0.5) * (1.0 - step(_Radius, 0.005)); 

                // 4. 颜色采样
                float4 colOuter = SAMPLE_TEXTURE2D(_secondary_texture, sampler_secondary_texture, input.uv);

                // 5. 边缘线计算
                // 当半径极小时，不显示边缘线
                float edgeLogic = (step(distortedMask, 0.5) - step(distortedMask, 0.5 - _edge_width)) * (1.0 - step(_Radius, 0.005));
                float3 emission = edgeLogic * _edge_color.rgb * _edge_color.a;

                // 6. 最终合成
                float finalAlpha = lerp(colOuter.a, 0.0, finalSwitch);
                // 确保边缘线是不透明的
                finalAlpha = saturate(finalAlpha + edgeLogic);

                return float4(colOuter.rgb + emission, finalAlpha);
            }
            ENDHLSL
        }
    }
}