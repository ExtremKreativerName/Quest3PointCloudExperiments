Shader "DepthVisualizer/SobelDepth"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _farColor("Far Color", Color) = (0, 0, 1, 1)
        _nearColor("Near Color", Color) = (1, 0, 0, 1)
        _sobelColor("sobel Color", Color) = (1,0,1,1)
    	
        _MidColPos("Middle Color Position",Range(0.2,0.99)) =0.89
    	
    	_DeltaX("step",Range(0,0.1)) =0.01
    	_DeltaY("step",Range(0,0.1)) =0.01
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

          

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _farColor,_nearColor, _sobelColor;
                float _MidColPos,_DeltaX,_DeltaY;
            CBUFFER_END

            //linear calculation from chat gpt needs to be checked
            // unity eydepth: return 1.0 / (zBufferParam.z * depth + zBufferParam.w);
            // unity lin01 depth: 1.0 / (zBufferParam.x * depth + zBufferParam.y);
            // zBufferParam: Used to linearize Z buffer values. x is (1-far/near), y is (far/near), z is (x/far) and w is (y/far).
           
          
            //from git page https://gist.github.com/mattatz/d14898c16008e3ad1abe
            float sobel (sampler2D tex, float2 uv) {
			float2 delta = float2(_DeltaX, _DeltaY);
			
			float4 hr = float4(0, 0, 0, 0);
			float4 vt = float4(0, 0, 0, 0);
			
			hr += tex2D(tex, (uv + float2(-1.0, -1.0) * delta)) *  1.0;
			hr += tex2D(tex, (uv + float2( 0.0, -1.0) * delta)) *  0.0;
			hr += tex2D(tex, (uv + float2( 1.0, -1.0) * delta)) * -1.0;
			hr += tex2D(tex, (uv + float2(-1.0,  0.0) * delta)) *  2.0;
			hr += tex2D(tex, (uv + float2( 0.0,  0.0) * delta)) *  0.0;
			hr += tex2D(tex, (uv + float2( 1.0,  0.0) * delta)) * -2.0;
			hr += tex2D(tex, (uv + float2(-1.0,  1.0) * delta)) *  1.0;
			hr += tex2D(tex, (uv + float2( 0.0,  1.0) * delta)) *  0.0;
			hr += tex2D(tex, (uv + float2( 1.0,  1.0) * delta)) * -1.0;
			
			vt += tex2D(tex, (uv + float2(-1.0, -1.0) * delta)) *  1.0;
			vt += tex2D(tex, (uv + float2( 0.0, -1.0) * delta)) *  2.0;
			vt += tex2D(tex, (uv + float2( 1.0, -1.0) * delta)) *  1.0;
			vt += tex2D(tex, (uv + float2(-1.0,  0.0) * delta)) *  0.0;
			vt += tex2D(tex, (uv + float2( 0.0,  0.0) * delta)) *  0.0;
			vt += tex2D(tex, (uv + float2( 1.0,  0.0) * delta)) *  0.0;
			vt += tex2D(tex, (uv + float2(-1.0,  1.0) * delta)) * -1.0;
			vt += tex2D(tex, (uv + float2( 0.0,  1.0) * delta)) * -2.0;
			vt += tex2D(tex, (uv + float2( 1.0,  1.0) * delta)) * -1.0;
			
			return sqrt(hr * hr + vt * vt);
		}

            float sobelDepth (sampler2D tex, float _DeltaX, float _DeltaY,float2 uv) {
			
			float2 delta = float2(_DeltaX, _DeltaY);
            	
			float hr = 0;
			float vt = 0;
			
			hr += tex2D(tex, (uv + float2(-1.0, -1.0) * delta)).r *  1.0;
			//hr += tex2D(tex, (uv + float2( 0.0, -1.0) * delta)).r *  0.0;
			hr += tex2D(tex, (uv + float2( 1.0, -1.0) * delta)).r * -1.0;
			hr += tex2D(tex, (uv + float2(-1.0,  0.0) * delta)) *  2.0;
			//hr += tex2D(tex, (uv + float2( 0.0,  0.0) * delta)) *  0.0;
			hr += tex2D(tex, (uv + float2( 1.0,  0.0) * delta)) * -2.0;
			hr += tex2D(tex, (uv + float2(-1.0,  1.0) * delta)) *  1.0;
			//hr += tex2D(tex, (uv + float2( 0.0,  1.0) * delta)) *  0.0;
			hr += tex2D(tex, (uv + float2( 1.0,  1.0) * delta)) * -1.0;
			
			vt += tex2D(tex, (uv + float2(-1.0, -1.0) * delta)) *  1.0;
			vt += tex2D(tex, (uv + float2( 0.0, -1.0) * delta)) *  2.0;
			vt += tex2D(tex, (uv + float2( 1.0, -1.0) * delta)) *  1.0;
			//vt += tex2D(tex, (uv + float2(-1.0,  0.0) * delta)) *  0.0;
			//vt += tex2D(tex, (uv + float2( 0.0,  0.0) * delta)) *  0.0;
			//vt += tex2D(tex, (uv + float2( 1.0,  0.0) * delta)) *  0.0;
			vt += tex2D(tex, (uv + float2(-1.0,  1.0) * delta)) * -1.0;
			vt += tex2D(tex, (uv + float2( 0.0,  1.0) * delta)) * -2.0;
			vt += tex2D(tex, (uv + float2( 1.0,  1.0) * delta)) * -1.0;
			
			return sqrt(hr * hr + vt * vt);
		}
            
            half4 FourColorGradient(float GradientMiddlePos, float GradientMiddlePosTwo, half4 ColorOne, half4 ColorTwo, half4 ColorThree, half4 ColorFour, float Pos)
    {
    
             //Gradient
            float is_top = step(GradientMiddlePos, Pos);
            float is_topTwo = step(GradientMiddlePosTwo, Pos);
   
            half4 col1 = lerp(lerp(ColorOne, ColorTwo, is_top), ColorThree, is_topTwo);
            half4 col2 = lerp(lerp(ColorTwo, ColorThree, is_top), ColorFour, is_topTwo);
    
             // calculate percentage of how much which color is used
            float percent = (Pos - is_top * GradientMiddlePos - is_topTwo * (GradientMiddlePosTwo - GradientMiddlePos)) /
                                      (is_top + (-2 * is_top + 1) * GradientMiddlePos
                                       + (-1 + is_topTwo) * (GradientMiddlePosTwo - GradientMiddlePos) * is_top
                                       - (GradientMiddlePosTwo - GradientMiddlePos) * is_topTwo);
              
    
            // linear interpolating between colors
            half4 finalColor = lerp(col1, col2, percent) ;
   
    
            return finalColor;
    }
            half4 ThreeColorGradient(float GradientMiddlePos, half4 ColorOne, half4 ColorTwo, half4 ColorThree, float Pos)
            {
	             //gradient
			    float is_top = step(GradientMiddlePos, Pos);
			   
			    half4 col1 = lerp(ColorOne, ColorTwo, is_top);
			    half4 col2 = lerp(ColorTwo, ColorThree, is_top);
			    
			   // calculate percentage of how much which color is used
			    float percent = (Pos - (is_top * GradientMiddlePos)) /
			                    (is_top + (-2 * is_top + 1) * GradientMiddlePos);
			   // linear interpolating between colors
			    return  lerp(col1, col2, percent) ;
            }

            v2f vert (appdata v)
            {
                v2f o;
                  o.vertex = TransformObjectToHClip(v.vertex.xyz);
               
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
               
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
            	
                float depth = tex2D(_MainTex, i.uv).r;
               float sobelEdge = sobel(_MainTex,i.uv);
				//half4 color = lerp(_nearColor,_farColor,depth );
            	//half4 color = _farColor * depth *0.1;
            	half4 color = ThreeColorGradient(_MidColPos,half4(1,0,0,1),half4(0,0.5,0,1), half4(0,0,1,1),depth);
                // Combine the color gradient with the Sobel edge detection
                 return lerp(color, _sobelColor, sobelEdge);
            	//return color;
            }
            ENDHLSL
        }
    }
}
