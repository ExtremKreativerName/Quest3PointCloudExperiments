Shader "Custom/DrawProceduralNowDemo"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            Name "DrawProcedural"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag

            // XR and URP defines
            #pragma multi_compile_instancing
            #pragma multi_compile _ _STEREO_INSTANCING_ON
            #pragma multi_compile _ UNITY_SINGLE_PASS_STEREO

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UnityInput.hlsl"

            StructuredBuffer<float4> _graphicsBufferPoints;

            struct ToFrag
            {
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            ToFrag Vert(uint id : SV_VertexID)
            {
                ToFrag o;
                UNITY_SETUP_INSTANCE_ID(o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                float4 worldPos = _graphicsBufferPoints[id];

                float4 clipPos = TransformWorldToHClip(worldPos.xyz);
                o.vertex = clipPos;
                return o;
            }

            float4 Frag(ToFrag i) : SV_Target
            {
                return float4(1, 1, 0, 1); // Yellow
            }

            ENDHLSL
        }
    }
}

/*
Shader "Custom/DrawProceduralNowDemo"
{
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		
		Pass
		{
			HLSLPROGRAM
			#pragma vertex Vert
			#pragma fragment Frag
	
	#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
	
	struct ToFrag
	{
		float4 vertex : SV_POSITION;
	};
	
	StructuredBuffer<float4> _graphicsBufferPoints;
	
	
	ToFrag Vert( uint vi : SV_VertexID )
	{
		ToFrag o;
		o.vertex = mul(UNITY_MATRIX_VP, _graphicsBufferPoints[ vi ]);
		return o;
	}
	
	
	float4 Frag( ToFrag i ) : SV_Target
	{
		return float4( 1, 1, 0, 1 ); // Yellow
	}
	
	ENDHLSL
	

	
		}
	}
}*/