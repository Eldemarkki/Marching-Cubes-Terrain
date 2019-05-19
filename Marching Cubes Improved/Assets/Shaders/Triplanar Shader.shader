﻿Shader "Marching Cubes/Triplanar Shader" {
	Properties{
		XColor("X Color", Color) = (0,0,0,0)
		YColor("Y Color", Color) = (0,0,0,0)
		NegativeYColor("Negative Y Color", Color) = (0,0,0,0)
		ZColor("Z Color", Color) = (0,0,0,0)
		YColorAmount("Y Color Amount", float) = 0.4
	}

		SubShader{

			Tags { "RenderType" = "Opaque"}

			Pass {
				CGPROGRAM
				#pragma target 3.0
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"

				fixed4 XColor;
				fixed4 YColor;
				fixed4 NegativeYColor;
				fixed4 ZColor;
				fixed YColorAmount;

				struct appdata {
					fixed3 normal : NORMAL;
					float4 vertex : POSITION;
				};

				struct v2f {
					half4 color : COLOR;
					float4 vertex : SV_POSITION;
				};

				v2f vert(appdata v)
				{
					fixed x = abs(v.normal.x);
					fixed yNormal = v.normal.y;
					fixed y = abs(yNormal) * YColorAmount;
					fixed z = abs(v.normal.z);

					fixed total = (x + y + z);
					x /= total;
					y /= total;
					z /= total;

					// X
					fixed4 col = (XColor * x);

					// Y
					if (yNormal < 0)
						col += NegativeYColor * y;
					else
						col += YColor * y;

					// Z
					col += (ZColor * z);

					v2f o;

					o.color = col.rgba;
					o.vertex = UnityObjectToClipPos(v.vertex);

					return o;
				}



				fixed4 frag(v2f i) : SV_TARGET{
					return i.color;
				}

				ENDCG
			}
	}

}