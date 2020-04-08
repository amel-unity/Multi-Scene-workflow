Shader "Custom/Fleisch"
{

    //Vertex Color Masks:
    // R: Displacement Mask
    // G: Movement Mask
    // B: Displacement Noise Frequency Modifier
    // A: Albedo Ramp Mix (mixes between two colour ramps)
    Properties
    {
        [NoScaleOffset] _DisplacementNoise ("Displacement Noise", 3D) = "" {}
        [NoScaleOffset] _DisplacementRamp ("Displacement Ramp", 2D) = "white" {}
        _EdgeLength ("Edge length", Range(2,50)) = 15
        _Displacement ("Displacement", Range(0, 1.0)) = 0.3
        _DisplacementIndex ("Displacement Index", int) = 0        
        _DisplacementScale ("Displacement Scale", Float) = 2
        _DisplacementSpeed ("Displacement Speed", Float) = 0.1
        
        [Space]
        [NoScaleOffset] _VolumeNoise ("Albedo Noise", 3D) = "" {}
        [NoScaleOffset] _AlbedoRamp1 ("Albdeo Ramp1", 2D) = "white" {}
        [NoScaleOffset] _AlbedoRamp2 ("Albdeo Ramp2", 2D) = "white" {}
        _OctaveIndex ("OctaveIndex", Vector) = (0,1,2,3)
        
        _Scale ("Albedo Scale", Float) = 2
        _Color ("Albedo Tint", Color) = (1,1,1,1)
        [Space]
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows vertex:vert addshadow //tessellate:tessEdge

        
        #pragma target 4.6
        #include "Tessellation.cginc"

        sampler2D _AlbedoRamp1, _AlbedoRamp2;
        sampler3D _VolumeNoise;
        sampler3D _DisplacementNoise;
        sampler2D _DisplacementRamp;

        struct Input
        {
            half4 color : COLOR;
            half3 worldPos : TEXCOORD0;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        half _EdgeLength, _Displacement, _Scale, _DisplacementScale, _DisplacementSpeed;
        int4 _OctaveIndex;
        int _AlbedoRamp1Index, _AlbedoRamp2Index;
        int _DisplacementIndex;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)


        float4 tessEdge (appdata_full v0, appdata_full v1, appdata_full v2)
        {
            return UnityEdgeLengthBasedTess (v0.vertex, v1.vertex, v2.vertex, _EdgeLength);
        }

        void vert (inout appdata_full v) {
            // o.localPos = v.vertex.xyz;
            // float3 offset = abs(_SinTime[3]);
            float3 offset = half3(0,_SinTime[1]*_DisplacementSpeed,0)*v.color.g;
            //float d = tex3Dlod(_DisplacementNoise, float4(offset+v.vertex.xyz*_DisplacementScale*v.color.b,0))[_DisplacementIndex];
			float d = tex3Dlod(_DisplacementNoise, float4(offset+(v.vertex.xyz*_DisplacementScale*v.color.b), 0))[_DisplacementIndex];
            float h = tex2Dlod(_DisplacementRamp, float4(d,d,0,0));
			h = h * _Displacement * (v.color.r);
            v.vertex.xyz += v.normal * h;
        }


        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            _OctaveIndex = clamp(0,3,_OctaveIndex);
            fixed oct1 = tex3D (_VolumeNoise, IN.worldPos * _Scale)[_OctaveIndex.x];
            fixed oct2 = tex3D (_VolumeNoise, IN.worldPos * _Scale * 4)[_OctaveIndex.y];
            fixed oct3 = tex3D (_VolumeNoise, IN.worldPos * _Scale * 16)[_OctaveIndex.z];
            fixed oct4 = tex3D (_VolumeNoise, IN.worldPos * _Scale * 256)[_OctaveIndex.w];
            fixed N = (oct1 + oct2*0.5 + oct3*0.25 + oct4*0.125) * 0.5333333333;
            
            fixed4 a = tex2D(_AlbedoRamp1, N);
            fixed4 b = tex2D(_AlbedoRamp2, N);
            fixed4 c = lerp(a, b, IN.color.a);



            o.Albedo = c;
            // o.Emission = N;
            o.Metallic = _Metallic;


            o.Smoothness = _Glossiness * c.a;
            o.Alpha = _Color.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
