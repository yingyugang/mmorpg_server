// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/UnitBase" {
Properties {
		_MainTex ("Base Texture ", 2D) = "white" {} 
        _Color ("Color", Color) = (1, 1, 1 ,1)
    } 
    SubShader {
        Pass { 
        	Lighting Off 
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata_t {
                half4 vertex : POSITION;
                half4 color : COLOR;
               	half2 texcoord : TEXCOORD0;
            };
            
            struct v2f {
                half4 vertex : POSITION;
                half4 color : COLOR;
                half2 texcoord : TEXCOORD0;
            };
            
            uniform sampler2D _MainTex;
            uniform half4 _Color;  
            v2f vert (appdata_t v)
            {
                v2f o1;
                o1.vertex = UnityObjectToClipPos(v.vertex);
                o1.color = half4(0,0,0,1);
                o1.texcoord = v.texcoord;
                return o1;
            }
            
            half4 frag (v2f i) : COLOR
            {
				half4 baseColor = tex2D(_MainTex, i.texcoord);
            	baseColor = lerp(baseColor,_Color ,1-baseColor.a);
            	return baseColor;
            }
			ENDCG
		}
	} 
	FallBack "Diffuse"
}
