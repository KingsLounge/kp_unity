Shader "Custom/OnlyRim" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)	
	_Shininess ("Shininess", Range (0.01, 1)) = 0.078125
	_RimColor ("Rim Color", Color) = (1.0,0.19,0.16,0.0)
    _RimPower ("Rim Power", Range(0.5,8.0)) = 0.5
}

SubShader {
	Tags { "Queue"="Geometry+20" "RenderType"="Opaque" }

//	Blend SrcAlpha OneMinusSrcAlpha
//	Blend OneMinusDstColor One
//	Blend One OneMinusSrcAlpha
	Blend SrcAlpha One
	LOD 300
	
	CGPROGRAM
	#pragma surface surf Lambert alphatest:_Cutoff
	#pragma target 3.0

//	sampler2D _MainTex;
	fixed4 _Color;
	half _Shininess;
	float4 _RimColor;
	float _RimPower;

	struct Input {
		float2 uv_MainTex;
		float3 viewDir;
	};

	
	void surf (Input IN, inout SurfaceOutput o) 
	{
		float3 rimLightValue = abs(dot (normalize(IN.viewDir), o.Normal));
		o.Albedo = _Color;

		o.Gloss = 0;

//		o.Alpha = _Color.a;

		o.Specular = _Shininess;

		half rim = 1.0 - saturate(rimLightValue);

//		_RimPower = 0.4;

		half power = pow (rim, _RimPower);
		o.Alpha = power;
		o.Emission = _RimColor.rgb * power;
	}
	ENDCG
}

Fallback "VertexLit"
}
