// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/custom_mask_color_01" {
 	Properties {
		_MainTex		("main", 2D) = "white" {}
		_MaskTex		("Mask r:color pow g:alpha pow ", 2D) = "white" {}
		_MaskColor		("Mask Color", Color) = (1,1,1,1)          
		_MaskColorPow	("Mask Color Power", Float) = 1		
	}

	CGINCLUDE

		#include "UnityCG.cginc"
	
		sampler2D	_MainTex;
		sampler2D	_MaskTex;
		fixed4		_MaskColor;
		float		_MaskColorPow;
		half4 _MainTex_ST;
		half4 _MaskTex_ST;
						
		struct v2f {
			half4 pos : SV_POSITION;
			half2 uv  : TEXCOORD0;						
		};
	
		v2f vert(appdata_full v)
		{
			v2f o;			
			o.pos    = UnityObjectToClipPos (v.vertex);	
			o.uv     = TRANSFORM_TEX(v.texcoord.xy, _MainTex);
			
			return o; 
		}
		
		fixed4 frag( v2f i ) : COLOR 
		{	
			fixed4 mainColor = tex2D (_MainTex, i.uv);
			fixed4 maskColor = tex2D (_MaskTex, i.uv);

			fixed4 resultColor = mainColor*_MaskColorPow *_MaskColor * maskColor.r;
			resultColor.a	   = maskColor.w;
			return resultColor;
		}

	ENDCG

	SubShader {
    	Tags {"RenderType" = "Transparent" "Queue" = "Transparent-2" "Reflection" = "RenderReflectionTransparentBlend" }
		Cull Off
		ZWrite On
		//Blend SrcAlpha One
	    Blend SrcAlpha OneMinusSrcAlpha
				
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

