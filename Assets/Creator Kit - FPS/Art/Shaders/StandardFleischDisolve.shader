Shader "Custom/StandardFleischDissolve"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _NormalMap ("Normal", 2D) = "bump" {}
        _Occlusion ("Occlusion", 2D) = "white" {}
        _Specular ("Metallic/Smoothness", 2D) = "white" {}
        _EmissionMap ("Emission Map", 2D) = "white" {}
        _DissolveNoise("DissolveNoise", 2D) = "white" {}
        
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
		[HDR]_EmissionColor ("Emission Color", Color) = (1,1,1,1)

        [Space]
        [NoScaleOffset] _DisplacementNoise ("Displacement Noise", 3D) = "" {}
        _Displacement ("Displacement", Range(0, 1.0)) = 0.3
        _DisplacementIndex ("Displacement Index", int) = 0        
        _DisplacementScale ("Displacement Scale", Float) = 2
        _DisplacementSpeed ("Displacement Speed", Float) = 0.1
        
		_EdgeSize ("EdgeSize", Range(0,1)) = 0.2
		_Cutoff ("cutoff", Range(0,1)) = 0.0	
		[HDR]_EdgeColor1 ("Edge Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="AlphaTest" "Queue" = "Transparent+1" }

		Pass
		{
			ZWrite On
			ZClip On
			ColorMask 0
			
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

            sampler2D _DissolveNoise;
            float _Cutoff;
            float _EdgeSize;
            float4 _DissolveNoise_ST;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _DissolveNoise);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                float Noise = tex2D(_DissolveNoise, i.uv).r;
                Noise = lerp(0, 1, Noise);
                
                _Cutoff  = lerp(0, _Cutoff + _EdgeSize, _Cutoff);
            
                clip(Noise - _Cutoff);
                return float4(1,1,1,1);
            }
            ENDCG
		}

        ColorMask RGBA
        ZWrite On

        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows vertex:vert addshadow alpha:fade

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex, _NormalMap, _Occlusion, _Specular, _EmissionMap;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color, _EmissionColor;
        half _Displacement, _Scale, _DisplacementScale, _DisplacementSpeed;
        int _DisplacementIndex;
        sampler3D _DisplacementNoise;
		fixed Alpha;
		
		sampler2D _DissolveNoise;
		float _Cutoff;
		float _EdgeSize;
		half4 _EdgeColor1;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void vert (inout appdata_full v) {
            // o.localPos = v.vertex.xyz;
            // float3 offset = abs(_SinTime[3]);
            float3 offset = half3(0,_SinTime[1]*_DisplacementSpeed,0)*v.color.g;
            //float d = tex3Dlod(_DisplacementNoise, float4(offset+v.vertex.xyz*_DisplacementScale*v.color.b,0))[_DisplacementIndex];
			float d = tex3Dlod(_DisplacementNoise, float4(offset+(v.vertex.xyz*_DisplacementScale*v.color.b), 0))[_DisplacementIndex];
            float h = d;
			h = h * _Displacement * v.color.r;
            v.vertex.xyz += v.normal * h;
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float Noise = tex2D(_DissolveNoise, IN.uv_MainTex).r;
			Noise = lerp(0, 1, Noise);
			_Cutoff  = lerp(0, _Cutoff + _EdgeSize, _Cutoff);
			
            //Dissolve Effect
            half Edge = smoothstep(_Cutoff + _EdgeSize, _Cutoff, clamp(Noise, _EdgeSize, 1));
        
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
			o.Alpha = c.a;
            // Metallic and smoothness come from slider variables
            float4 occlusion = tex2D(_Occlusion, IN.uv_MainTex);
            float4 metalSmooth = tex2D(_Specular, IN.uv_MainTex);
            
           // o.Albedo = c.rgb;
            o.Emission = tex2D(_EmissionMap, IN.uv_MainTex) * _EmissionColor * tex3D(_DisplacementNoise, IN.worldPos+_SinTime[2]).r + _EdgeColor1 * Edge;
            o.Metallic = metalSmooth.r * _Metallic;
            o.Smoothness = metalSmooth.a * _Glossiness;
            o.Occlusion = occlusion.r;
            o.Normal = UnpackNormal (tex2D (_NormalMap, IN.uv_MainTex));
            
            clip(Noise - _Cutoff);
        }
        ENDCG
    }
    FallBack "Diffuse"
}
