Shader "QuantumTheory/Road" {
	Properties {
		_MainTex ("Diffuse", 2D) = "white" {}
		_NormalMap ("NormalMap", 2D) = "bump" {}
		_Scale ("Markings Scale", Float) = 0
		_Bias ("Markings Bias", Float) = 0
		
		_SpecularPower ("Specular Power", Float) = 1
		_Glossiness ("Glossiness", Range(0,1)) = 1
		
		_SpecColor ("Specular Color", Color) = (1,1,1,1)
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		
		LOD 300
		
		CGPROGRAM
		#pragma surface surf BlinnPhong

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
			o.Normal = UnpackNormal( tex2D(_NormalMap, IN.uv_MainTex));
			
			fixed4 diffuseColor = tex2D (_MainTex, IN.uv_MainTex);
//			float modifier = (diffuseColor.r * _Scale) + _Bias;// Basic Bias Scale
			float modifier = saturate((diffuseColor.r - _Bias) * _Scale + _Bias);
			diffuseColor += modifier * IN.color;
			
			o.Albedo = diffuseColor;
			o.Alpha = diffuseColor.a;
			
			o.Specular = _Glossiness;
			o.Gloss = _SpecularPower;
		}
		ENDCG
	} 
	FallBack "QuantumTheory/Mobile/Road"
}
