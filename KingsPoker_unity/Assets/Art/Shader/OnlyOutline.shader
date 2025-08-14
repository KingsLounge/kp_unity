Shader "Custom/OnlyOutline" {

	Properties {
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
//        ZWrite On
//        ZTest LEqual
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


		

	} 
		FallBack "Diffuse"
		//CustomEditor "CubeShaderGUI_Character"
}