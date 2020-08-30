Shader "QuantumTheory/Mobile/Road" {
	Properties {
		_MainTex ("Diffuse", 2D) = "white" {}
		_Scale ("Markings Scale", Float) = 0
		_Bias ("Markings Bias", Float) = 0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Lambert

		sampler2D _MainTex;
		sampler2D _NormalMap;
		
		float _Scale;
		float _Bias;
		
		float _SpecularPower;
		float _Glossiness;
		
		struct Input {
			float2 uv_MainTex;
			float4 color : COLOR;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			fixed4 diffuseColor = tex2D (_MainTex, IN.uv_MainTex);
//			float modifier = (diffuseColor.r * _Scale) + _Bias;// Basic Bias Scale
			float modifier = saturate((diffuseColor.r - _Bias) * _Scale + _Bias);
			diffuseColor += modifier * IN.color;
			
			o.Albedo = diffuseColor;
			o.Alpha = diffuseColor.a;
		}
		ENDCG
	} 
	FallBack "Mobile/VertexLit"
}
