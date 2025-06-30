Shader "Unlit/PositionDisplayColor"
{
    Properties
    {
        _Scale("Scale", Float) = 0.01
    }
    
    SubShader
    {
        
         Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        Pass
        {
             Tags{"LightMode" = "UniversalForward"}
            HLSLPROGRAM
           
            #pragma vertex Vertex
            #pragma fragment Fragment
            #pragma instancing_options assumeuniformscaling procedural //Instancing
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Assets/_MyFolder/Examples/TextureSampleExample/Script/PointDataStruct.hlsl"

            struct appdata
            {
                float4 vertex   : POSITION;
               float3 normalOS : NORMAL;
            };
            
            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 positionWS : TEXCOORD1; // Position in world space
                float3 normalWS : TEXCOORD2;
                float4 color : COLOR0;
            };
            
            StructuredBuffer<PointData> _graphicsBufferPoints;
            float4 _BaseColor;
            float _Scale;

            v2f Vertex(appdata v, uint instanceID : SV_InstanceID)
            {
                v2f o;
                
                float3 position = _graphicsBufferPoints[instanceID].position.xyz;
                float3 scaledVertex = v.vertex.xyz * _Scale;
                o.positionWS = mul(unity_ObjectToWorld,  float4 ( scaledVertex+ position, 1.0) );
                o.pos = mul(UNITY_MATRIX_VP,  o.positionWS);
                VertexNormalInputs normInputs = GetVertexNormalInputs(v.normalOS);
                o.normalWS = normInputs.normalWS;
                o.color = _graphicsBufferPoints[instanceID].color;
                
                return o;
            }

            float4 Fragment(v2f i) : SV_Target
            {

                return i.color;//UniversalFragmentBlinnPhong(lightingInput, surfaceInput);
                
            }
            ENDHLSL
        }
    }
}
