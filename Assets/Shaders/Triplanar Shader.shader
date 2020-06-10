﻿Shader "Marching Cubes/Triplanar Shader" 
{
	Properties
	{
		XColor("X Color", Color) = (0,0,0,0)
		YColor("Y Color", Color) = (0,0,0,0)
		NegativeYColor("Negative Y Color", Color) = (0,0,0,0)
		ZColor("Z Color", Color) = (0,0,0,0)
		YColorAmount("Y Color Amount", float) = 0.4
	}

	SubShader
	{
		Tags { "RenderType" = "Opaque"}

		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows
		#pragma target 3.0

		fixed4 XColor;
		fixed4 YColor;
		fixed4 NegativeYColor;
		fixed4 ZColor;
		fixed YColorAmount;


		struct Input 
		{
			float3 worldNormal;
		};

		void surf (Input IN, inout SurfaceOutputStandard o)
		{
			fixed x = abs(IN.worldNormal.x);
			fixed yNormal = IN.worldNormal.y;
			fixed y = abs(yNormal) * YColorAmount;
			fixed z = abs(IN.worldNormal.z);

			fixed total = (x + y + z);
			x /= total;
			y /= total;
			z /= total;

			// X
			fixed3 col = (XColor * x);

			// Y
			if (yNormal < 0)
				col += NegativeYColor * y;
			else
				col += YColor * y;

			// Z
			col += (ZColor * z);

			o.Albedo = col;
		}
		ENDCG
	}
	FallBack "Diffuse"
}