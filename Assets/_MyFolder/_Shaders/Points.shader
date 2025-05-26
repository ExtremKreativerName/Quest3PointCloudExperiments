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

            StructuredBuffer<float3> _graphicsBufferPoints;

            float4 vert(uint id : SV_VertexID): SV_POSITION
            {
                float3 worldPos = _graphicsBufferPoints[id];
                float4 world = float4(worldPos, 1.0);
                
                return mul(UNITY_MATRIX_VP, world);
            }


            float4 frag() : SV_Target
            {
                return float4(1, 0.2, 0.7, 1);
            }
            ENDHLSL
        }
    }
}
