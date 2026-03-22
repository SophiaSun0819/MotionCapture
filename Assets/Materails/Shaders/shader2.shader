Shader "Custom/SwitchWorld_TransparentInner"
{
    Properties
    {
        _Position("Interactor Position", Vector) = (0, 0, 0, 0)
        _Radius("Radius", Range(0.8, 1.2)) = 1
        [NoScaleOffset]_noise_texture("Noise Texture (Triplanar)", 2D) = "white" {}
        _noise_scale("Noise Scale", Float) = 2
        _moveSpeed("Noise Move Speed", Float) = 0.5
        _noise_Strengh("Edge Distortion Strength", Range(0, 1)) = 0.5
        _edge_width("Edge Glow Width", Float) = 0.1
        _edge_color("Edge Color", Color) = (0, 1, 1, 1)
        // 删除了 _main_texture，保留 _secondary_texture 作为外部贴图
        [NoScaleOffset]_secondary_texture("Outer Texture (Secondary)", 2D) = "white" {}
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent" // 修改为透明类型
            "Queue"="Transparent"      // 修改为透明队列
        }
        
        Pass
        {
            Name "Universal Forward"
            Tags { "LightMode" = "UniversalForward" }

            // 添加混合模式：源颜色 * Alpha + 目标颜色 * (1 - Alpha)
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off // 透明物体通常关闭深度写入以避免遮挡错误

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : INTERP0;
                float3 normalWS : INTERP1;
                float2 uv : INTERP2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            CBUFFER_START(UnityPerMaterial)
                float3 _Position;
                float _Radius;
                float _noise_scale;
                float _moveSpeed;
                float _noise_Strengh;
                float _edge_width;
                float4 _edge_color;
            CBUFFER_END

            TEXTURE2D(_noise_texture); SAMPLER(sampler_noise_texture);
            TEXTURE2D(_secondary_texture); SAMPLER(sampler_secondary_texture);

            float4 SampleTriplanarNoise(float3 worldPos, float3 worldNormal)
            {
                float3 blending = abs(worldNormal);
                blending /= (blending.x + blending.y + blending.z);
                float2 uvX = worldPos.zy * _noise_scale + _Time.y * _moveSpeed;
                float2 uvY = worldPos.xz * _noise_scale + _Time.y * _moveSpeed;
                float2 uvZ = worldPos.xy * _noise_scale + _Time.y * _moveSpeed;
                return SAMPLE_TEXTURE2D(_noise_texture, sampler_noise_texture, uvX) * blending.x +
                       SAMPLE_TEXTURE2D(_noise_texture, sampler_noise_texture, uvY) * blending.y +
                       SAMPLE_TEXTURE2D(_noise_texture, sampler_noise_texture, uvZ) * blending.z;
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = input.uv;
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                float dist = distance(input.positionWS, _Position);
                float maskRaw = saturate(dist / max(0.0001, _Radius));

                float4 noise = SampleTriplanarNoise(input.positionWS, input.normalWS);
                float distortedMask = lerp(maskRaw, noise.r, _noise_Strengh * (1 - maskRaw));
                
                // finalSwitch: 1 为内部（现在要变透明），0 为外部（显示贴图）
                float finalSwitch = step(distortedMask, 0.5); 

                // 采样外部贴图
                float4 colOuter = SAMPLE_TEXTURE2D(_secondary_texture, sampler_secondary_texture, input.uv);

                // --- 逻辑修改点 ---
                // 1. 颜色：如果是内部(finalSwitch=1)，颜色不重要（因为Alpha会是0），我们直接取外部颜色
                float3 finalColor = colOuter.rgb;

                // 2. 透明度：外部 Alpha 是 colOuter.a (通常是 1)，内部 Alpha 是 0
                float finalAlpha = lerp(colOuter.a, 0.0, finalSwitch);

                // 3. 边缘发光：即使内部透明，边缘线通常还是需要的
                float edgeMask = step(distortedMask, 0.5) - step(distortedMask, 0.5 - _edge_width);
                float3 emission = edgeMask * _edge_color.rgb * _edge_color.a;
                
                // 如果在边缘处，强制 Alpha 变为 1 确保发光可见
                finalAlpha = max(finalAlpha, edgeMask);

                return float4(finalColor + emission, finalAlpha);
            }
            ENDHLSL
        }
    }
}