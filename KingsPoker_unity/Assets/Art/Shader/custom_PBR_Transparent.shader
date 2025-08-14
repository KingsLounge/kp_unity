Shader "Custom/PBR_Transparent"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
		_Cutoff("Alpha cutoff", Range(0,1)) = 0.5
	}
		SubShader
		{
			Tags { "RenderType" = "Opaque" "Queue" = "AlphaTest" "IgnoreProjector" = "True" "RenderType" = "TransparentCutout"  }
	LOD 150


//#pragma surface surf Lambert noforwardadd alphatest:_Cutoff
			 //Tags{ "Queue" = "AlphaTest" "RenderType" = "TransparentCutout" "IgnoreProjector" = "True" }
			// Blend SrcAlpha OneMinusSrcAlpha
			//Cull Off
			
			//Tags {"Queue" = "Transparent" "RenderType" = "Transparent" }
			LOD 200

			CGPROGRAM

			#pragma surface surf Standard fullforwardshadows alphatest:_Cutoff
			#pragma target 3.0

			sampler2D _MainTex;

			struct Input {
				float2 uv_MainTex;
			};

			half _Glossiness;
			half _Metallic;
			fixed4 _Color;

			void surf(Input IN, inout SurfaceOutputStandard o)
			{
				fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
				o.Albedo = c.rgb;
				o.Metallic = _Metallic;
				o.Smoothness = _Glossiness;
				o.Alpha = c.a;
			}
			ENDCG
		}
			FallBack "Transparent/Cutout/VertexLit"
}
