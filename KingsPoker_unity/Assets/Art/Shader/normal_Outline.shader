Shader "Custom/normal_outline" {

	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}

		_Glossiness("Smoothness", Range(0.0, 1.0)) = 0.5
		_SpecColor("Specular", Color) = (0.2,0.2,0.2)
		_SpecGlossMap("Specular", 2D) = "white" {}

		_BumpScale("NormalScale", Float) = 1.0
		_BumpMap("Normal Map", 2D) = "bump" {}

		_OcclusionStrength("OcclusionStrength", Range(0.0, 1.0)) = 1.0
		_OcclusionMap("Occlusion", 2D) = "white" {}

		_OutlineColor ("Outline Color", Color) = (0.8, 0.2, 0.2, 1.0)
        _OutlineDist ("Shift", Range(-1, 1)) = 0.02
	}

	SubShader 
	{
		 /// first pass
        Tags { "IgnoreProjector"="True" "RenderType"="TransparentCutout" }
        LOD 200
        Blend SrcAlpha OneMinusSrcAlpha
        Lighting On
        ZWrite On
        ZTest LEqual
        Cull Front

        CGPROGRAM
 
        #pragma surface surf StandardSpecular fullforwardshadows addshadow alphatest:_Cutoff vertex:vert
        #pragma target 3.0
        #include "UnityCG.cginc"
 
 
        float4 _OutlineColor;
        float _OutlineDist;
 
     
        struct Input {
            float2 uv_MainTex;
        };
     
        void vert (inout appdata_full v) {

//			_Dist = 0.04;

            v.vertex.xyz += float3(v.normal.xyz)*_OutlineDist;    
        }
     
        void surf (Input i, inout SurfaceOutputStandardSpecular o) {

//			_OutlineColor.rgb = 0;
//			_OutlineColor.r = 1;

            o.Emission = _OutlineColor.rgb;  // main albedo color
            o.Specular =0;
            o.Smoothness = 0;
            o.Alpha = 1;
            ///////////////
        }
        ENDCG
        //// end first pass


		// second pass.
		Tags { "IgnoreProjector"="True" "RenderType"="Opaque"}
        Blend SrcAlpha OneMinusSrcAlpha
        Lighting On
        ZWrite On
//        Cull Off
		Cull Back

		
		CGPROGRAM

		#pragma shader_feature _OCCLUSIONMAP
		#pragma shader_feature _SPECGLOSSMAP
		#pragma shader_feature _NORMALMAP

		// Physically based Standard lighting model, and enable shadows on all light types
//		        #pragma surface surf StandardSpecular fullforwardshadows addshadow alphatest:_Cutoff
		#pragma surface surf StandardSpecular fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0
		#include "UnityCG.cginc"
//		#include "UnityStandardUtils.cginc"
//		#include "UnityStandardCore.cginc"

		fixed4 _Color;
		sampler2D _MainTex;

#ifdef _NORMALMAP
		sampler2D _BumpMap;
		half	  _BumpScale;
#endif

#ifdef _SPECGLOSSMAP
		sampler2D _SpecGlossMap;
#else
		half _Glossiness;
#endif


#ifdef _OCCLUSIONMAP
		sampler2D	_OcclusionMap;
		half		_OcclusionStrength;
#endif

		struct Input {
			float2 uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutputStandardSpecular o) {

			fixed4 c = tex2D (_MainTex, IN.uv_MainTex.xy) * _Color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;

#ifdef _NORMALMAP
			float3 normalTangent = UnpackScaleNormal( tex2D(_BumpMap, IN.uv_MainTex.xy), _BumpScale);
			o.Normal = normalTangent;
#endif
			
#ifdef _SPECGLOSSMAP
			half4 specGloss = tex2D(_SpecGlossMap, IN.uv_MainTex.xy);
			o.Specular = specGloss.rgb;
			o.Smoothness = specGloss.a;
#else
			o.Specular = _SpecColor;
			o.Smoothness = _Glossiness;
#endif

#ifdef _OCCLUSIONMAP
			half occ = tex2D(_OcclusionMap, IN.uv_MainTex.xy).g;
			o.Occlusion = LerpOneTo (occ, _OcclusionStrength);
#endif
		}
		ENDCG

	} 
		FallBack "Diffuse"
		CustomEditor "CubeShaderGUI_Character"
}