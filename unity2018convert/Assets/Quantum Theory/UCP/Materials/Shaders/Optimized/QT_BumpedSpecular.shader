Shader "QuantumTheory/BumpedSpecular" {
	Properties {
		_MainTex ("Diffuse(rgb), Specular Mask(a)", 2D) = "white" {}
		_NormalMap ("NormalMap", 2D) = "bump" {}
		
		_Shininess ("Specular Power", Float) = 1
		_Glossiness ("Glossiness", Range(0,1)) = 1
		
		_SpecColor ("Specular Color", Color) = (1,1,1,1)
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		
		LOD 400
		
		CGPROGRAM
		#pragma surface surf BlinnPhong
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _NormalMap;
		
		float _Shininess;
		float _Glossiness;
		
		
		struct Input {
			float2 uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			fixed4 diffuseColor = tex2D (_MainTex, IN.uv_MainTex);
			
			o.Normal = UnpackNormal( tex2D(_NormalMap, IN.uv_MainTex));
			
			o.Albedo = diffuseColor;
			o.Specular = _Glossiness;
			
			o.Gloss = _Shininess * diffuseColor.a;
			o.Alpha = diffuseColor.a;
		}
		ENDCG
	}
	fallback "Specular"
}