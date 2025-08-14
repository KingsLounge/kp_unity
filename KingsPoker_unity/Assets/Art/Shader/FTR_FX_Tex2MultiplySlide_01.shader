// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "FTR/FX/Tex2MultiplySlide_01" {
 	Properties {
		_MainTex   ("Main", 2D) = "white" {}
		_MainColor ("MainColor", Color) = (1,1,1,1)          
		_MainU ("Main U speed", Float) = 0.0
		_MainV ("Main V speed", Float) = 0.0
		_MultiplyTex  ("Multiply", 2D) = "white" {}
		_MultiplyColor ("MultiplyColor", Color) = (1,1,1,1)          
		_MultiplyU  ("Multiply U speed", Float) = 0.0
		_MultiplyV  ("Multiply V speed", Float) = 0.0
	}

	CGINCLUDE

		#include "UnityCG.cginc"
	
		sampler2D _MainTex;
		sampler2D _MultiplyTex;
		float _MainU;
		float _MainV;
		float _MultiplyU;
		float _MultiplyV;
		half4 _MainTex_ST;
		half4 _MultiplyTex_ST;
		fixed4 _MainColor;				
		fixed4 _MultiplyColor;				
						
		struct v2f {
			half4 pos : SV_POSITION;
			half2 uv  : TEXCOORD0;
			half2 uv2 : TEXCOORD1;
		};
	
		v2f vert(appdata_full v)
		{
			v2f o;			
			o.pos    = UnityObjectToClipPos (v.vertex);	
			o.uv.xy  = TRANSFORM_TEX(v.texcoord.xy,_MainTex) + frac(float2(_MainU, _MainV) * _Time);
			o.uv2.xy = TRANSFORM_TEX(v.texcoord.xy,_MultiplyTex)  + frac(float2(_MultiplyU, _MultiplyV) * _Time);
			return o; 
		}
		
		fixed4 frag( v2f i ) : COLOR
		{	
			fixed4 mainColor  = tex2D (_MainTex, i.uv)*_MainColor;
			fixed4 subColor   = tex2D (_MultiplyTex, i.uv2)*_MultiplyColor;
			mainColor	  = mainColor * subColor;			
			return mainColor;
		}

	ENDCG

	SubShader {
    	Tags {"RenderType" = "Transparent" "Queue" = "Transparent" "Reflection" = "RenderReflectionTransparentBlend" }
		Cull Off
		ZWrite Off
        Blend SrcAlpha One
        //Blend SrcAlpha OneMinusSrcAlpha
				
		Pass {
	
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest 
			
			ENDCG
		}		
	} 
	FallBack "Diffuse"
}

