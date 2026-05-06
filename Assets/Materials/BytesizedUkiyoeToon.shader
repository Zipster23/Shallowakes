Shader "Bytesized/UkiyoeToon"
{
    Properties
    {
        // --- Terrain splatmap inputs (Unity fills these automatically) ---
        [HideInInspector] _Control("Splatmap", 2D) = "red" {}
        [HideInInspector] _Splat0("Layer 0", 2D) = "white" {}
        [HideInInspector] _Splat1("Layer 1", 2D) = "white" {}
        [HideInInspector] _Splat2("Layer 2", 2D) = "white" {}
        [HideInInspector] _Splat3("Layer 3", 2D) = "white" {}
        [HideInInspector] _Normal0("Normal 0", 2D) = "bump" {}
        [HideInInspector] _Normal1("Normal 1", 2D) = "bump" {}
        [HideInInspector] _Normal2("Normal 2", 2D) = "bump" {}
        [HideInInspector] _Normal3("Normal 3", 2D) = "bump" {}

        // Tiling per splat layer (Unity terrain editor sets these)
        [HideInInspector] _Splat0_ST("Splat0 ST", Vector) = (1,1,0,0)
        [HideInInspector] _Splat1_ST("Splat1 ST", Vector) = (1,1,0,0)
        [HideInInspector] _Splat2_ST("Splat2 ST", Vector) = (1,1,0,0)
        [HideInInspector] _Splat3_ST("Splat3 ST", Vector) = (1,1,0,0)

        // --- Ukiyo-e toon settings ---
        [HDR] _AmbientColor("Ambient Color", Color) = (0.4,0.4,0.4,1)
        _BandCount("Diffuse Band Count", Range(1, 8)) = 3
        _Smoothness("Band Smoothness", Range(0, 0.5)) = 0.025

        // Tri-tone palette
        [HDR] _ShadowColor("Shadow Color", Color) = (0.12, 0.18, 0.35, 1)
        [HDR] _MidColor("Mid Color", Color) = (0.75, 0.62, 0.38, 1)
        [HDR] _LitColor("Lit Color", Color) = (0.96, 0.92, 0.82, 1)
    }

    SubShader
    {
        Tags
        {
            "Queue"           = "Geometry-100"
            "RenderType"      = "Opaque"
            "TerrainCompatible" = "True"
        }

        Pass
        {
            Name "FORWARD"
            Tags { "LightMode" = "ForwardBase" }

            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            #pragma target 3.0

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            // Splatmap
            sampler2D _Control;
            float4    _Control_ST;

            sampler2D _Splat0, _Splat1, _Splat2, _Splat3;
            float4    _Splat0_ST, _Splat1_ST, _Splat2_ST, _Splat3_ST;

            // Toon
            float4 _AmbientColor;
            float4 _ShadowColor;
            float4 _MidColor;
            float4 _LitColor;
            float  _BandCount;
            float  _Smoothness;

            struct appdata
            {
                float4 vertex  : POSITION;
                float3 normal  : NORMAL;
                float2 uv      : TEXCOORD0;  // terrain UV (0..1 over whole terrain)
            };

            struct v2f
            {
                float4 pos         : SV_POSITION;
                float2 uvControl   : TEXCOORD0;
                float2 uvSplat0    : TEXCOORD1;
                float2 uvSplat1    : TEXCOORD2;
                float2 uvSplat2    : TEXCOORD3;
                float2 uvSplat3    : TEXCOORD4;
                float3 worldNormal : TEXCOORD5;
                SHADOW_COORDS(6)
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos       = UnityObjectToClipPos(v.vertex);
                // Control map uses the raw terrain UV
                o.uvControl = TRANSFORM_TEX(v.uv, _Control);
                // Each splat layer can have its own tiling
                o.uvSplat0  = TRANSFORM_TEX(v.uv, _Splat0);
                o.uvSplat1  = TRANSFORM_TEX(v.uv, _Splat1);
                o.uvSplat2  = TRANSFORM_TEX(v.uv, _Splat2);
                o.uvSplat3  = TRANSFORM_TEX(v.uv, _Splat3);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                TRANSFER_SHADOW(o)
                return o;
            }

            // Stepped toon band -- returns 0..1
            float ToonBand(float NdotL)
            {
                float raw  = floor(NdotL * _BandCount) / _BandCount;
                float soft = smoothstep(raw - _Smoothness, raw + _Smoothness, NdotL)
                             * raw;
                return saturate(soft);
            }

            // Map a 0..1 band value to the tri-tone palette
            float4 PaletteColor(float band)
            {
                // 0 = shadow, 0.5 = mid, 1 = lit
                float4 a = lerp(_ShadowColor, _MidColor, saturate(band * 2.0));
                float4 b = lerp(_MidColor,   _LitColor,  saturate(band * 2.0 - 1.0));
                return band < 0.5 ? a : b;
            }

            float4 frag(v2f i) : SV_Target
            {
                // --- Splatmap blend ---
                float4 ctrl = tex2D(_Control, i.uvControl);
                // ctrl.rgba = weights for layers 0-3 (sum = 1)

                float4 col  = ctrl.r * tex2D(_Splat0, i.uvSplat0)
                            + ctrl.g * tex2D(_Splat1, i.uvSplat1)
                            + ctrl.b * tex2D(_Splat2, i.uvSplat2)
                            + ctrl.a * tex2D(_Splat3, i.uvSplat3);

                // --- Toon lighting ---
                float3 normal  = normalize(i.worldNormal);
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float  shadow  = SHADOW_ATTENUATION(i);
                float  NdotL   = saturate(dot(normal, lightDir)) * shadow;

                float  band    = ToonBand(NdotL);
                float4 toon    = PaletteColor(band);

                // Multiply splatmap albedo by toon shading + ambient
                float4 result  = col * (toon + _AmbientColor);
                result.a = 1;
                return result;
            }
            ENDCG
        }

        Pass
        {
            Name "SHADOW"
            Tags { "LightMode" = "ForwardAdd" }
            Blend One One
            ZWrite Off

            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma multi_compile_fwdadd_fullshadows
            #pragma target 3.0

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            sampler2D _Control;
            float4    _Control_ST;
            sampler2D _Splat0, _Splat1, _Splat2, _Splat3;
            float4    _Splat0_ST, _Splat1_ST, _Splat2_ST, _Splat3_ST;

            float4 _ShadowColor;
            float4 _LitColor;
            float  _BandCount;
            float  _Smoothness;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos         : SV_POSITION;
                float2 uvControl   : TEXCOORD0;
                float2 uvSplat0    : TEXCOORD1;
                float2 uvSplat1    : TEXCOORD2;
                float2 uvSplat2    : TEXCOORD3;
                float2 uvSplat3    : TEXCOORD4;
                float3 worldNormal : TEXCOORD5;
                SHADOW_COORDS(6)
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos       = UnityObjectToClipPos(v.vertex);
                o.uvControl = TRANSFORM_TEX(v.uv, _Control);
                o.uvSplat0  = TRANSFORM_TEX(v.uv, _Splat0);
                o.uvSplat1  = TRANSFORM_TEX(v.uv, _Splat1);
                o.uvSplat2  = TRANSFORM_TEX(v.uv, _Splat2);
                o.uvSplat3  = TRANSFORM_TEX(v.uv, _Splat3);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                TRANSFER_SHADOW(o)
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float4 ctrl = tex2D(_Control, i.uvControl);
                float4 col  = ctrl.r * tex2D(_Splat0, i.uvSplat0)
                            + ctrl.g * tex2D(_Splat1, i.uvSplat1)
                            + ctrl.b * tex2D(_Splat2, i.uvSplat2)
                            + ctrl.a * tex2D(_Splat3, i.uvSplat3);

                float3 normal   = normalize(i.worldNormal);
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float  shadow   = SHADOW_ATTENUATION(i);
                float  NdotL    = saturate(dot(normal, lightDir)) * shadow;
                float  band     = saturate(floor(NdotL * _BandCount) / _BandCount);

                return col * lerp(_ShadowColor, _LitColor, band) * 0.25;
            }
            ENDCG
        }

        UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }

    // Tells the terrain editor this is a terrain shader
    Fallback "Nature/Terrain/Diffuse"
}