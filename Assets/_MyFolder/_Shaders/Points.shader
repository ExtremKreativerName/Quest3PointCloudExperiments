Shader "Custom/Points"
{/*
    Properties
    {
        _Scale("Scale", Float) = 0.01
    }
    */
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            ZWrite Off
            Cull Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            StructuredBuffer<float4> _graphicsBufferPoints;

            float4 vert(uint id : SV_VertexID): SV_POSITION
            {
                float4 worldPos = _graphicsBufferPoints[id];
                
                return mul(UNITY_MATRIX_VP, worldPos);
            }


            float4 frag() : SV_Target
            {
                return float4(1, 0.2, 0.7, 1);
            }
            ENDHLSL
        }
    }
}
