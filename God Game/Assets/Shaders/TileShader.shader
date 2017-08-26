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
			};
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.color = _Color;
				if (v.texcoord.x == 1 || v.texcoord.y == 1 || v.texcoord.x == 0 || v.texcoord.y == 0) {
					o.color = float4(0, 0, 0, 1);
				}
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target { return i.color; }
			ENDCG
		}
	}
}
