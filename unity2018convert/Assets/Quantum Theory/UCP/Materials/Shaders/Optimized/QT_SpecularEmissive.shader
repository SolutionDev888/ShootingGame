Shader "QuantumTheory/Self-Illumin/SpecularEmissive" {
	Properties {
		_MainTex ("Diffuse (rgb) Specular Mask(a)", 2D) = "white" {}
		_Illum ("Emission Mask (rgba)", 2D) = "white" {}
		
		_EmissionLM ("Emission Strength", Float) = 1
		_SpecularPower ("Specular Power", Float) = 1
		_Glossiness ("Glossiness", Range(0,1)) = 1
		
		_SpecColor ("Specular Color", Color) = (1,1,1,1)// Automatically used by BlinnPhong
		
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		
		LOD 300
		
		CGPROGRAM
		#pragma surface surf BlinnPhong

		sampler2D _MainTex;
		sampler2D _Illum;
		
		float _EmissionLM;
		float _SpecularPower;
		float _Glossiness;
		
		struct Input {
			float2 uv_MainTex;
			float4 color : COLOR;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			fixed4 mainTexture = tex2D (_MainTex, IN.uv_MainTex);
			fixed4 emissionMask = tex2D(_Illum, IN.uv_MainTex);
			
			// Mod2X blend with vertex colors:
			fixed4 blendedMainTex = mainTexture * fixed4(IN.color.rgb,0) * 2;
			
			o.Albedo = blendedMainTex;
			o.Emission = emissionMask * blendedMainTex;//_EmissionLM only used by lightmapper.
			
			o.Specular = _Glossiness;
			o.Gloss = _SpecularPower * mainTexture.a;
			
			o.Alpha = 1.0;
		}
		ENDCG
	} 
	FallBack "QuantumTheory/Mobile/Self-Illumin/DiffuseEmissive"
}
