Shader "Custom/Brain"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Metallic ("Metallic Smoothness", 2D) = "black" {}
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _Occlusion ("Occlusion", 2D) = "white" {}
        
        _VeinColor("Vein Pulse Color", Color) = (1,1,1,1)
        _BrainColor("Brain Pulse Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _Metallic;
        sampler2D _NormalMap;
        sampler2D _Occlusion;
        sampler2D _VeinMap;
        
        float4 _VeinColor;
        float4 _BrainColor;
        
        struct Input
        {
            float2 uv_MainTex;
        };

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
            float4 occlusion = tex2D(_Occlusion, IN.uv_MainTex);
            float4 metalSmooth = tex2D(_Metallic, IN.uv_MainTex);
            
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = metalSmooth.r;
            o.Smoothness = metalSmooth.a;
            o.Alpha = c.a;
            o.Occlusion = occlusion.r;
            o.Normal = UnpackNormal (tex2D (_NormalMap, IN.uv_MainTex));
            
            //x is brain, y is y
            const float2 range = 0.04;
            
            float timeFract = frac(_Time.y);
            
            float uvMask = saturate(sin(smoothstep(timeFract - range, timeFract + range, IN.uv_MainTex.y) * 3.14));
            float brainPulse = saturate(sin(frac(_Time.y * 0.2) * 3.14));
            
            //this is the "true" wrapping, the one we do will miss some uv range at start & end of the [0..1] range
            //but it's not visible for the small band we do, so we avoid too much computation. This is left here for reference
            
            //float3 wrapUvs = float3(IN.uv_MainTex.y, IN.uv_MainTex.y + 1.0f, IN.uv_MainTex.y - 1.0f);        
            
            //float band1 = smoothstep(timeFract - range, timeFract + range, wrapUvs.x);
            //float band2 = smoothstep(timeFract - range, timeFract + range, wrapUvs.y);
            //float band3 = smoothstep(timeFract - range, timeFract + range, wrapUvs.z);            
            //float uvMask = saturate(sin(band1 * 3.14) + sin(band2 * 3.14) + sin(band3 * 3.14));
            
            o.Emission = (1.0f - metalSmooth.g) * _BrainColor * brainPulse + (1.0f - metalSmooth.b) * _VeinColor * uvMask;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
