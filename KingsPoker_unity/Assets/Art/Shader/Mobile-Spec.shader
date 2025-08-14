Shader "Rora/Specular" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_OverColor ("Over Color", Color) = (0,0,0,0)
	_SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
	_Shininess ("Shininess", Range (0.01, 1)) = 0.078125
	_MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
	_RimColor ("Rim Color", Color) = (0.26,0.19,0.16,0.0)
    _RimPower ("Rim Power", Range(0.5,8.0)) = 3.0	
}

SubShader {
	Tags { "Queue"="Geometry +20" "RenderType"="Opaque" }
	LOD 300
	
CGPROGRAM
#pragma surface surf BlinnPhong

sampler2D _MainTex;
fixed4 _Color;
fixed4 _OverColor;
half _Shininess;
float4 _RimColor;
float _RimPower;

struct Input {
	float2 uv_MainTex;
	float3 viewDir;
};

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
	fixed3 color = fixed3(0.5, 0.5, 0.5);
	o.Albedo = tex.rgb * _Color.rgb + _OverColor.rgb ;
	o.Alpha = _Color.a;
	o.Gloss = tex.a;
	o.Specular = _Shininess;	
	half rim = 1.0 - saturate(dot (normalize(IN.viewDir), o.Normal));
	o.Emission = _RimColor.rgb * pow(rim, _RimPower) + (_SpecColor.rgb * tex.a) / _Shininess;	
}
ENDCG
}

Fallback "VertexLit"
}
