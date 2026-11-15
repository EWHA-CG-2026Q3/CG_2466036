Shader "CG/S17_MyTexture"
{
    Properties
    {
        _BaseMap ("Base Map", 2D) = "white" {}
    }
    SubShader
    {
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            sampler2D _BaseMap;

            void vert(float3 positionOS : POSITION,
                      float2 uv : TEXCOORD0,
                      out float2 uvOut : TEXCOORD0,
                      out float4 positionCS : SV_POSITION)
            {
                uvOut = uv;
                positionCS = TransformObjectToHClip(positionOS);
            }

            // 격자 텍스처: 4×4 칸, 열 번호 1~4는 u(왼쪽→오른쪽), 행 A~D는 v(위→아래)
            //   1열 u 0~0.25, 2열 0.25~0.5, 3열 0.5~0.75, 4열 0.75~1
            //   A행 v 0.75~1, B행 0.5~0.75, C행 0.25~0.5, D행 0~0.25
            // 한 칸만 표면 전체에: p = uv × 0.25 + (그 칸의 왼쪽 아래 모서리)
            float4 frag(float2 uv : TEXCOORD0) : SV_Target
            {
                float2 p = uv;

                // 미션 1: B2 한 칸만 표면 전체에 (B2 왼쪽 아래 = (0.25, 0.5))
                // p = uv * 0.25 + float2(0.25, 0.5);

                // 미션 2: 그림 전체를 표면의 왼쪽 아래 1/4에, 나머지는 회색
                // p = uv * 2;
                // if (p.x > 1 || p.y > 1) return float4(0.8, 0.8, 0.8, 1);

                // 과제: D3 칸만 표면 전체에 (D3 왼쪽 아래 = (0.5, 0))
                p = uv * 0.25 + float2(0.5, 0);

                return tex2D(_BaseMap, p);
            }
            ENDHLSL
        }
    }
}
