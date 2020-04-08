Shader "Unlit/MapShader"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,.5,.5,1)
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
            
            #define UNITY_SHADER_NO_UPGRADE 1 

            struct appdata
            {
                float4 vertex : POSITION;
                float4 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };
            
            float4 _Color;
            float _WallThickness;

            v2f vert (appdata v)
            {
                v2f o;
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                
                float4 normal = mul(UNITY_MATRIX_MVP,float4(v.normal.xyz, 0.0));

                float rangeCenter = UNITY_NEAR_CLIP_VALUE + (1.0f - UNITY_NEAR_CLIP_VALUE) * 0.5f;
                o.vertex.xy += normalize(normal.xy) * _WallThickness * (1 - step(rangeCenter, o.vertex.z/o.vertex.w) * 2);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = _Color;

                return col;
            }
            ENDCG
        }
    }
}
