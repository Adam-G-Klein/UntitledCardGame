Shader "Signified/TooltipUIDocumentRenderTarget"
{
        Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _alpha ("Alpha", Range(0,1)) = 1
    }
    SubShader
    {
        Tags { "RenderType"= "Transparent" "Queue" = "Overlay" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha 
            // Uncomment below to support transparency... but then get occluded by the combat UI
            // ZWrite Off
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

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _alpha;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col;
                col = tex2D(_MainTex, i.uv);
                if(col.a == 0) discard;
                col = fixed4(col.r, col.g, col.b, _alpha);
                return col;
            }
            ENDCG
        }
    }
}
