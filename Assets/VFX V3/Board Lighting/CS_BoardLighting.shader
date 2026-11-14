// Combat board surface. Base look matches NormalMapGeneric (sprite as base colour + emission, fully
// metallic so the board itself is not washed out by diffuse light). On top of that, a highlight mask
// decides what can catch light: inside the mask the surface is smoother, more embossed and reflects a
// brighter shine colour, so moving lights glint along ornaments, cracks and pearls only.
// Two optional emissive layers, both confined to the mask and both eased so nothing pops:
//   Flow  - slow organic glow drifting through the mask (two scrolling noise layers, breathing).
//   Band  - a soft sheen band that sweeps across the mask now and then.
Shader "Custom/CS_BoardLighting"
{
    Properties
    {
        [MainTexture] _MainTex ("Sprite", 2D) = "white" {}
        _Emission_Strength ("Base Emission", Range(0, 2)) = 1
        [Normal] _NormalTex ("Normal Map", 2D) = "bump" {}
        _Normal_Strength ("Normal Strength", Range(0, 4)) = 1

        [Header(Highlight Mask)]
        _ShineMask ("Highlight Mask (R)", 2D) = "black" {}
        _Mask_Power ("Mask Contrast", Range(0.25, 4)) = 1
        _Base_Smoothness ("Base Smoothness", Range(0, 1)) = 0.2
        _Shine_Smoothness ("Shine Smoothness", Range(0, 1)) = 0.75
        [HDR] _Shine_Color ("Shine Color", Color) = (1.6, 1.4, 1.0, 1)
        _Base_Normal ("Normal Outside Mask", Range(0, 1)) = 0.35
        _Shine_Normal ("Normal Inside Mask", Range(0, 4)) = 1.5
        _MatteMask ("Matte Mask (R): soft cloth etc. that should not reflect like metal", 2D) = "black" {}
        _Matte_Strength ("Matte Strength", Range(0, 1)) = 1

        [Header(Flow Glow)]
        _Flow_Tex ("Flow Noise", 2D) = "gray" {}
        [HDR] _Flow_Color ("Flow Color", Color) = (0, 0, 0, 1)
        _Flow_Tiling ("Flow Tiling", Float) = 2
        _Flow_Speed ("Flow Speed (xy layer A, zw layer B)", Vector) = (0.02, 0.01, -0.015, 0.012)
        _Flow_Threshold ("Flow Threshold", Range(0, 1)) = 0.45
        _Flow_Softness ("Flow Softness", Range(0.01, 1)) = 0.3
        _Breath_Period ("Breath Period (s)", Float) = 7
        _Breath_Depth ("Breath Depth", Range(0, 1)) = 0.5
        _Flow_Fill ("Flow Fill Outside Mask", Range(0, 1)) = 0
        _Chroma ("Flow Chromatic Split", Range(0, 0.1)) = 0
        _Flow_Blur ("Flow Blur (mip bias)", Range(0, 8)) = 0

        [Header(Breeze Ripple)]
        [HDR] _Wave_Color ("Ripple Color", Color) = (0, 0, 0, 1)
        _Wave_Dir ("Ripple Travel Direction (xy)", Vector) = (0.8, 0.6, 0, 0)
        _Wave_Scale ("Ripple Spacing (waves across board)", Float) = 1.6
        _Wave_Speed ("Ripple Speed", Float) = 0.12
        _Wave_Sharp ("Ripple Softness (higher = narrower)", Range(1, 8)) = 3
        _Wave_Streak ("Blade Streak Density", Float) = 22
        _Field ("Field Ellipse (uv cx, cy, rx, ry; rx 0 = whole board)", Vector) = (0.5, 0.5, 0, 0)
        _Field_Soft ("Field Edge Softness", Range(0.001, 0.2)) = 0.03

        [Header(Rainbow)]
        _Rainbow ("Rainbow Amount", Range(0, 1)) = 0
        _Rainbow_Scale ("Rainbow Spread", Float) = 0.8
        _Rainbow_Speed ("Rainbow Drift", Float) = 0.02

        [Header(Light Pools (driven by BoardLightRig))]
        _Pool0 ("Pool 0 (uv xy, radius, intensity)", Vector) = (0.5, 0.5, 0.2, 0)
        [HDR] _Pool0_Color ("Pool 0 Color", Color) = (1, 0.8, 0.5, 0)
        _Pool1 ("Pool 1 (uv xy, radius, intensity)", Vector) = (0.5, 0.5, 0.2, 0)
        [HDR] _Pool1_Color ("Pool 1 Color", Color) = (1, 0.8, 0.5, 0)
        _Pool2 ("Pool 2 (uv xy, radius, intensity)", Vector) = (0.5, 0.5, 0.2, 0)
        [HDR] _Pool2_Color ("Pool 2 Color", Color) = (1, 0.8, 0.5, 0)
        _Pool3 ("Pool 3 (uv xy, radius, intensity)", Vector) = (0.5, 0.5, 0.2, 0)
        [HDR] _Pool3_Color ("Pool 3 Color (alpha = fill outside mask)", Color) = (1, 0.8, 0.5, 0)
        _Pool_Wash ("Pool Wash Outside Mask", Range(0, 0.5)) = 0.06

        [Header(Sheen Band)]
        [HDR] _Band_Color ("Band Color", Color) = (0, 0, 0, 1)
        _Band_Dir ("Band Direction (xy)", Vector) = (0.7, -0.7, 0, 0)
        _Band_Width ("Band Width", Range(0.01, 1)) = 0.12
        _Band_Period ("Band Period (s)", Float) = 11
        _Band_Travel ("Band Travel Fraction", Range(0.05, 1)) = 0.45
        _Band_Fill ("Band Fill Outside Mask", Range(0, 1)) = 0
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _FORWARD_PLUS
            #pragma multi_compile_fragment _ _LIGHT_LAYERS
            #define _SURFACE_TYPE_TRANSPARENT 1

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_MainTex);   SAMPLER(sampler_MainTex);
            TEXTURE2D(_NormalTex); SAMPLER(sampler_NormalTex);
            TEXTURE2D(_ShineMask); SAMPLER(sampler_ShineMask);
            TEXTURE2D(_Flow_Tex);  SAMPLER(sampler_Flow_Tex);
            TEXTURE2D(_MatteMask); SAMPLER(sampler_MatteMask);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half _Emission_Strength, _Normal_Strength;
                half _Mask_Power, _Base_Smoothness, _Shine_Smoothness, _Base_Normal, _Shine_Normal, _Matte_Strength;
                half4 _Shine_Color;
                half4 _Flow_Color; float _Flow_Tiling; float4 _Flow_Speed; half _Flow_Threshold, _Flow_Softness;
                float _Breath_Period; half _Breath_Depth;
                half4 _Band_Color; float4 _Band_Dir; half _Band_Width; float _Band_Period; half _Band_Travel;
                float4 _Pool0, _Pool1, _Pool2, _Pool3; half4 _Pool0_Color, _Pool1_Color, _Pool2_Color, _Pool3_Color; half _Pool_Wash;
                half _Flow_Fill, _Chroma, _Band_Fill, _Flow_Blur;
                half4 _Wave_Color; float4 _Wave_Dir; float _Wave_Scale, _Wave_Speed; half _Wave_Sharp; float _Wave_Streak;
                half _Rainbow; float _Rainbow_Scale, _Rainbow_Speed;
                float4 _Field; half _Field_Soft;
            CBUFFER_END

            half FlowAt(float2 fuv, float t)
            {
                half fa = SAMPLE_TEXTURE2D_BIAS(_Flow_Tex, sampler_Flow_Tex, fuv + _Flow_Speed.xy * t, _Flow_Blur).r;
                half fb = SAMPLE_TEXTURE2D_BIAS(_Flow_Tex, sampler_Flow_Tex, fuv * 1.37 + _Flow_Speed.zw * t, _Flow_Blur).r;
                return smoothstep(_Flow_Threshold, _Flow_Threshold + _Flow_Softness, fa * fb * 2.0);
            }

            // soft round pool in board UV space (aspect corrected), smooth all the way to zero
            half Pool(float2 uv, float4 p)
            {
                float2 d = (uv - p.xy) * float2(16.0 / 9.0, 1.0);
                float r = max(p.z, 0.001);
                half f = saturate(1.0 - dot(d, d) / (r * r));
                return f * f * p.w;
            }

            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; half4 color : COLOR; };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                half4 color : COLOR;
            };

            Varyings Vert(Attributes v)
            {
                Varyings o;
                o.positionWS = TransformObjectToWorld(v.positionOS.xyz);
                o.positionCS = TransformWorldToHClip(o.positionWS);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }

            half4 Frag(Varyings i) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv) * i.color;
                half mask = pow(saturate(SAMPLE_TEXTURE2D(_ShineMask, sampler_ShineMask, i.uv).r), _Mask_Power);
                // matte areas (cloth) take light softly: no metal sheen, a flatter emboss, no light pools
                half matte = SAMPLE_TEXTURE2D(_MatteMask, sampler_MatteMask, i.uv).r * _Matte_Strength;

                // sprites have no reliable normals/tangents: the board faces the camera (-Z), +X right, +Y up
                half3 nTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_NormalTex, sampler_NormalTex, i.uv), _Normal_Strength * lerp(_Base_Normal, _Shine_Normal, mask) * (1.0 - 0.6 * matte));
                float3 T = TransformObjectToWorldDir(float3(1, 0, 0));
                float3 B = TransformObjectToWorldDir(float3(0, 1, 0));
                float3 N = TransformObjectToWorldDir(float3(0, 0, -1));
                float3 normalWS = normalize(nTS.x * T + nTS.y * B + nTS.z * N);

                InputData inputData = (InputData)0;
                inputData.positionWS = i.positionWS;
                inputData.positionCS = i.positionCS;
                inputData.normalWS = normalWS;
                inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(i.positionWS);
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(i.positionCS);
                inputData.bakedGI = SampleSH(normalWS);
                inputData.shadowMask = half4(1, 1, 1, 1);

                // emissive layers, eased and confined to the mask
                float t = _Time.y;
                half breath = 1.0 - _Breath_Depth * (0.5 + 0.5 * cos(t * 6.2831853 / max(_Breath_Period, 0.01)));
                float2 fuv = i.uv * _Flow_Tiling;
                // chromatic split: red and blue read the flow slightly offset, so edges fringe like prism light
                float2 co = float2(1.0, 0.55) * _Chroma;
                half3 flow3 = half3(FlowAt(fuv + co, t), FlowAt(fuv, t), FlowAt(fuv - co, t));
                // the open field (felt) the fill/breeze layers are confined to, so objects on the table stay out of it
                half field = 1.0;
                if (_Field.z > 0.0)
                {
                    float fr = length((i.uv - _Field.xy) / max(_Field.zw, 1e-4));
                    field = 1.0 - smoothstep(1.0 - _Field_Soft, 1.0 + _Field_Soft, fr);
                }
                half3 flowEm = _Flow_Color.rgb * flow3 * breath * lerp(mask, field, _Flow_Fill);

                // breeze ripple: soft waves travel across; a stretched noise breaks them into blade-like streaks
                float2 auv = i.uv * float2(16.0 / 9.0, 1.0);
                float2 wdir = normalize(_Wave_Dir.xy + float2(1e-4, 0));
                float2 wperp = float2(-wdir.y, wdir.x);
                half gust = pow(0.5 + 0.5 * sin((dot(auv, wdir) * _Wave_Scale - t * _Wave_Speed) * 6.2831853), _Wave_Sharp);
                half blades = SAMPLE_TEXTURE2D_BIAS(_Flow_Tex, sampler_Flow_Tex, float2(dot(auv, wperp) * _Wave_Streak, dot(auv, wdir) * 1.2 - t * _Wave_Speed * 0.35), _Flow_Blur * 0.5).r;
                half lum = dot(tex.rgb, half3(0.3, 0.59, 0.11));
                half3 waveEm = _Wave_Color.rgb * gust * smoothstep(0.35, 0.8, blades) * lerp(0.5, 1.0, saturate(lum * 2.5)) * field;

                float2 dir = normalize(_Band_Dir.xy + float2(1e-4, 0));
                float along = dot(i.uv - 0.5, dir) + 0.75;                  // ~0..1.5 across the board
                float phase = frac(t / max(_Band_Period, 0.01));             // band crosses during the first part
                float head = phase / max(_Band_Travel, 0.05) * 1.5;
                float d = (along - head) / max(_Band_Width, 0.001);
                half band = exp(-d * d) * smoothstep(0.0, 0.08, phase) * (1.0 - smoothstep(_Band_Travel * 0.85, _Band_Travel, phase));
                half3 bandEm = _Band_Color.rgb * band * lerp(mask, 1.0, _Band_Fill);

                // colour alpha = how much a pool also fills the unmasked surface (0 = ornaments only)
                half maskW = lerp(_Pool_Wash, 1.0, mask);
                half3 poolEm = _Pool0_Color.rgb * Pool(i.uv, _Pool0) * lerp(maskW, 1.0, _Pool0_Color.a)
                             + _Pool1_Color.rgb * Pool(i.uv, _Pool1) * lerp(maskW, 1.0, _Pool1_Color.a)
                             + _Pool2_Color.rgb * Pool(i.uv, _Pool2) * lerp(maskW, 1.0, _Pool2_Color.a)
                             + _Pool3_Color.rgb * Pool(i.uv, _Pool3) * lerp(maskW, 1.0, _Pool3_Color.a);
                bandEm += poolEm;
                bandEm *= 1.0 - matte;

                // soft rainbow drift over all the moving light
                float hue = frac(dot(i.uv, float2(0.55, 0.35)) * _Rainbow_Scale + t * _Rainbow_Speed);
                half3 rainbow = saturate(abs(frac(hue + half3(0.0, 2.0 / 3.0, 1.0 / 3.0)) * 6.0 - 3.0) - 1.0);
                half3 tint = lerp(half3(1, 1, 1), 0.45 + rainbow * 0.9, _Rainbow);
                flowEm *= tint; bandEm *= tint; waveEm *= tint;
                flowEm += waveEm;

                SurfaceData s = (SurfaceData)0;
                // keep the art's own colour so metal reflects its own gold/pearl tones; the mask only brightens it
                s.albedo = tex.rgb * lerp(half3(1, 1, 1), _Shine_Color.rgb, mask);
                s.metallic = 1;
                s.specular = 0;
                s.smoothness = lerp(_Base_Smoothness, _Shine_Smoothness, mask) * (1.0 - 0.85 * matte);
                s.occlusion = 1;
                s.emission = tex.rgb * saturate(_Emission_Strength * tex.a) + (flowEm + bandEm) * tex.a;
                s.alpha = tex.a;
                s.normalTS = nTS;

                half4 c = UniversalFragmentPBR(inputData, s);
                c.a = tex.a;
                return c;
            }
            ENDHLSL
        }
    }
    FallBack Off
}
