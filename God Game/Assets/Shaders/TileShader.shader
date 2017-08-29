Shader "Unlit/TileShader"
{
	Properties
	{
		_Color ("Main Color", Color) = (1,1,1,1)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
		
			fixed4 _Color;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
				float4 internal : COLOR1;
			};
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.color = _Color;
				o.internal = float4(1, 1, 1, 1);
				if (v.texcoord.x == 1 || v.texcoord.y == 1 || v.texcoord.x == 0 || v.texcoord.y == 0) { 
					 o.internal = float4(0, 0, 0, 0); 
				} 
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target {
				float internal = step(1 - i.internal.r, 0.9);
				return i.color * internal;
			}
			ENDCG
		}
	}
}
