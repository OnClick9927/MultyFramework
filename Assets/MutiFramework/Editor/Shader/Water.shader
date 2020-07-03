Shader "Unlit/Water"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Density("_Density",float)=10
		_RotateSpeed("Rotate Speed",Range(0,10))=5
   }
    SubShader
    {
Tags{"Queue"="Transparent"}
blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
			float _Density;
			 float _RotateSpeed;


            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {

				i.uv+=0.005*sin(i.uv *3.14*_Density+_Time.y);
i.uv -= float2(0.5,0.5);
 if (length(i.uv) > 0.5) //由于移动，那么可能会采样到区域外的内容。将uv的模大于0.5的在原点采样
 
            {
 
                return fixed4(0, 0, 0, 0); //去掉多余的图片，否则四边会出现多余的内容
 
            }
 float angle = _RotateSpeed * _Time.x; //计算需要选择的角度，不要把_Time放到下面的式子中计算，否则会导致速度加快（当然通过

 
            i.uv = float2(i.uv.x * cos(angle) - i.uv.y * sin(angle), i.uv.x * sin(angle) + i.uv.y * cos(angle));
i.uv += float2(0.5, 0.5); 


                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}
