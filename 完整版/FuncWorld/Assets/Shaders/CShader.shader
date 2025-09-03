Shader "Custom/CShader" {
	Properties {
		_Color ("Color", Color) = (1,0,0,1)
		_MainTex ("Albedo (RGB)", 2D) = "红色" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Pass{
			CGPROGRAM
			struct my_struct{
				int a;
		    };
			float sum(float a,float b){
				return a+b;
			}
			fixed4 _Color;
			sampler2D _MainTex;
			struct my_vert_data{
				float4 pos : POSITION;
		    };

			#pragma vertex my_vert //定义顶点着色器入口函数
			float4 my_vert(float4 pos:POSITION):POSITION{
				//return UnityObjectToClipPos(pos);
				//return mul(UNITY_MATRIX_MVP,pos);
				return UnityObjectToClipPos(pos);
			}
			#pragma fragment my_frag
			//fixed4 my_frag(float2 uv : TEXCOORD0) : COLOR{
			fixed4 my_frag(float2 uv :TEXCOORD0) : COLOR{
				return fixed4(1.0,0.0,0.0,1.0);
				//return _Color; 
				//return tex2D(_MainTex,uv);
			}
			ENDCG
		}
	}
	FallBack "Diffuse"
}
