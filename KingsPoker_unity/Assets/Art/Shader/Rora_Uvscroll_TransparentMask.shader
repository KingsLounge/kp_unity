// Simplified Diffuse shader. Differences from regular Diffuse one:
// - no Main Color
// - fully supports only 1 directional light. Other lights can affect it, but it will be per-vertex/SH.

Shader "Rora/Transparent/Rora_Uvscroll_TransparentMask" {
	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_IllumMask("IlumMask", 2D) = "black" {}
		_UvscrollMask("Uvscroll", 2D) = "black" {}
		_OverColor("Over Color", Color) = (0,0,0,0)
		_IllumColor("Ilum Color", Color) = (1,1,1,1)
			_IllumPower1("Ilum Power", Range(0,4.0)) = 1.0
		_IllumPower("Rim Power", Range(0,2.0)) = 1.0
			_Cutoff("Alpha cutoff", Range(0,1)) = 0.5
		_Speed("Speed", Float) = 0.2
	}
		SubShader{
			Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"  }
			LOD 150

		CGPROGRAM
		#pragma surface surf Lambert noforwardadd alphatest:_Cutoff

		sampler2D _MainTex;
		sampler2D _IllumMask;
		sampler2D _UvscrollMask;
		fixed4 _OverColor;
		fixed4 _IllumColor;
		half _IllumPower;
		half _IllumPower1;
		float _Speed;

		struct Input {
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutput o) {
			half TimeSpeed = _Speed;
			fixed4 c = tex2D(_MainTex, fixed2(IN.uv_MainTex.x, IN.uv_MainTex.y - _Time.y*TimeSpeed));
			fixed4 maskC = tex2D(_IllumMask, fixed2(IN.uv_MainTex.x, IN.uv_MainTex.y - _Time.y*TimeSpeed));
			fixed4 maskD = tex2D(_UvscrollMask, IN.uv_MainTex);

			o.Albedo = c.rgb + _OverColor.rgb;

			fixed3 d = (c.rgb * _IllumColor.rgb) * maskC.r * _IllumPower*_IllumPower1;


			 o.Emission = d * maskD.r;
			o.Alpha = c.a;
		}
		ENDCG
		}

			Fallback "Mobile/VertexLit"
}

