Shader "QuantumTheory/Self-Illumin/DiffuseVCEmissive" {
	Properties {
		_MainTex ("Diffuse", 2D) = "white" {}
		_Illum ("Emission Mask (RGBA)", 2D) = "white" {}
		
		_EmissionLM ("LightMapper Emission Strength", Float) = 0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 250
		
		CGPROGRAM
		#pragma surface surf Lambert

		sampler2D _MainTex;
		sampler2D _Illum;
		
		float _EmissionLM;
		
		struct Input {
			float2 uv_MainTex;
			float4 color : COLOR;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			fixed4 mainTexture = tex2D (_MainTex, IN.uv_MainTex);
			fixed4 emissionMask = tex2D(_Illum, IN.uv_MainTex);
			
			// Mod2X blend with vertex colors:
			fixed3 blendedMainTex = mainTexture.rgb * IN.color.rgb * 2;
			
			float3 emission = emissionMask * blendedMainTex.rgb * IN.color.a;//_EmissionLM only used by lightmapper
			
			o.Albedo = blendedMainTex;
			o.Emission = emission;
			o.Alpha = mainTexture.a;
		}
		ENDCG
	}
	Fallback "QuantumTheory/Mobile/Self-Illumin/DiffuseEmissive"
}
