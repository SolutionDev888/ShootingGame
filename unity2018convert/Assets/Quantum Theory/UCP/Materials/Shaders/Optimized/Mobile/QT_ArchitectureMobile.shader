Shader "QuantumTheory/Mobile/Architecture" {
	Properties {
		_MainTex ("Diffuse", 2D) = "white" {}
		_Reflection ("CubeMap Reflection", Cube) = "black" {}
		_ReflectionMask ("Masks - Cube(R), Spec(G), Gloss(B)", 2D) = "black" {}
		
		_ReflectionTint ("Reflection Tint", Color) = (1,1,1,1)
		
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Lambert

		sampler2D _MainTex;
		samplerCUBE _Reflection;
		sampler2D _ReflectionMask;
		
		float4 _ReflectionTint;
		
		struct Input {
			float2 uv_MainTex;
			float4 color : COLOR;
			float3 worldRefl;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			fixed4 diffuseColor = tex2D (_MainTex, IN.uv_MainTex);
			fixed3 masks = tex2D(_ReflectionMask, IN.uv_MainTex);
			
			fixed4 reflectionColor = texCUBE(_Reflection, IN.worldRefl) * _ReflectionTint;
						
			
			// Blend diffuse with vertex color:
			float3 albedo = (IN.color.rgb * diffuseColor.rgb) * 2;
			
			o.Albedo = albedo;
			
			o.Emission = reflectionColor.rgb * masks.r;// This would give correct reflections, but it is not done this way in the original.
			o.Alpha = diffuseColor.a;
		}
		ENDCG
	} 
	FallBack "Mobile/VertexLit"
}