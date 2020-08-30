Shader "QuantumTheory/Architecture" {
	Properties {
		_MainTex ("Diffuse", 2D) = "white" {}
		_Reflection ("CubeMap Reflection", Cube) = "black" {}
		_ReflectionMask ("Masks - Cube(R), Spec(G), Gloss(B)", 2D) = "black" {}
		_NormalMap ("NormalMap", 2D) = "bump" {}
		
		_SpecularPower ("Specular Power", Float) = 1
		_Glossiness ("Glossiness", Range(0,1)) = 1
		
		_SpecColor ("Specular Color", Color) = (1,1,1,1)
		_ReflectionTint ("Reflection Tint", Color) = (1,1,1,1)
		
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 400
		
		CGPROGRAM
		#pragma surface surf BlinnPhong
		#pragma target 3.0

		sampler2D _MainTex;
		samplerCUBE _Reflection;
		sampler2D _ReflectionMask;
		sampler2D _NormalMap;
		
		float _SpecularPower;
		float _Glossiness;
		
		float4 _ReflectionTint;
		
		struct Input {
			float2 uv_MainTex;
			float4 color : COLOR;
			float3 worldRefl;
			INTERNAL_DATA
		};

		void surf (Input IN, inout SurfaceOutput o) {
			fixed4 diffuseColor = tex2D (_MainTex, IN.uv_MainTex);
			fixed3 masks = tex2D(_ReflectionMask, IN.uv_MainTex);
			
			o.Normal = UnpackNormal( tex2D(_NormalMap, IN.uv_MainTex));
			float3 worldRefl = WorldReflectionVector (IN, o.Normal);
			
			fixed4 reflectionColor = texCUBE(_Reflection, worldRefl) * _ReflectionTint;
						
			
			// Blend diffuse with vertex color:
			float3 albedo = (IN.color.rgb * diffuseColor.rgb) * 2;
			
			o.Albedo = albedo;// alternative reflection: reflectionColor.rgb * masks.r +
			o.Specular = _Glossiness * masks.b;
			
			o.Gloss = masks.g * _SpecularPower;
			o.Emission = reflectionColor.rgb * masks.r;// This would give correct reflections, but the original adds it to albedo instead.
			o.Alpha = diffuseColor.a;
		}
		ENDCG
	} 
	FallBack "QuantumTheory/Mobile/Architecture"
}
