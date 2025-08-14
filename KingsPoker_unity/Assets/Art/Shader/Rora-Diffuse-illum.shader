// Simplified Diffuse shader. Differences from regular Diffuse one:
// - no Main Color
// - fully supports only 1 directional light. Other lights can affect it, but it will be per-vertex/SH.

Shader "Rora/Diffuse_Illum" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_IllumMask ("IlumMask", 2D) = "black" {}
	_OverColor ("Over Color", Color) = (0,0,0,0)
	_IllumColor ("Ilum Color", Color) = (1,1,1,1)
		_IllumPower1("Ilum Power", Range(0,4.0)) = 1.0
	_IllumPower ("Rim Power", Range(0,2.0)) = 1.0	
}    
SubShader {
	Tags { "RenderType"="Opaque" }
	LOD 150

CGPROGRAM
#pragma surface surf Lambert noforwardadd

sampler2D _MainTex;
sampler2D _IllumMask;
fixed4 _OverColor;
fixed4 _IllumColor;
half _IllumPower;
half _IllumPower1;

struct Input {
	float2 uv_MainTex;
};

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
	fixed4 maskC = tex2D(_IllumMask, IN.uv_MainTex);

	o.Albedo = c.rgb + _OverColor.rgb;
     o.Emission = (c.rgb * _IllumColor.rgb) * maskC.r * _IllumPower*_IllumPower1;
	o.Alpha = c.a;
}
ENDCG
}

Fallback "Mobile/VertexLit"
}
