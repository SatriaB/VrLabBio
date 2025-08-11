Shader "UI/MicroscopeRaw"
{
    Properties
    {
        [PerRendererData]_MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _Zoom ("Zoom", Float) = 1.0
        _Vignette ("Vignette", Range(0,1)) = 0.2
        _ChromAb ("Chromatic Aberration", Range(0,0.05)) = 0.0
        _BlurStrength ("Blur Strength", Range(0,1)) = 0
        _Brightness ("Brightness", Range(0,2)) = 1
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ZWrite ("ZWrite", Float) = 0
        _ZTest ("ZTest", Float) = 4
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        ZWrite Off
        ZTest [_ZTest]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask RGBA

        Pass
        {
            Name "UI"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
                float4 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _Zoom, _Vignette, _ChromAb, _BlurStrength, _Brightness;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.worldPos = v.vertex;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                #ifdef UNITY_UI_CLIP_RECT
                if (any(i.worldPos.xy < _ClipRect.xy) || any(i.worldPos.xy > _ClipRect.zw))
                    discard;
                #endif

                float2 center = float2(0.5, 0.5);
                float2 uv = i.uv;
                float2 toC = uv - center;

                // Zoom
                float zoom = max(_Zoom, 0.001);
                uv = center + toC / zoom;

                // Clamp keluar area
                if (uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1)
                    return 0;

                // Chromatic aberration
                float2 dir = normalize(toC + 1e-5);
                float2 off = dir * _ChromAb;

                fixed r = tex2D(_MainTex, uv + off).r;
                fixed g = tex2D(_MainTex, uv).g;
                fixed b = tex2D(_MainTex, uv - off).b;
                fixed4 col = fixed4(r, g, b, 1);

                // Blur aman di mobile (pakai screen pixel size)
                if (_BlurStrength > 0.001)
                {
                    float2 px = float2(1.0 / _ScreenParams.x, 1.0 / _ScreenParams.y) * 1.5;
                    fixed4 bl = (tex2D(_MainTex, uv + px) +
                        tex2D(_MainTex, uv - px) +
                        tex2D(_MainTex, uv + float2(px.x, -px.y)) +
                        tex2D(_MainTex, uv + float2(-px.x, px.y))) * 0.25;
                    col = lerp(col, bl, saturate(_BlurStrength));
                }

                // Vignette
                float d = length(toC) * 1.4142;
                float vig = 1 - smoothstep(0.7, 1.1, d);
                col.rgb *= lerp(1.0, vig, _Vignette);

                // Brightness & vertex color
                col.rgb *= _Brightness;
                col *= i.color;

                return col;
            }
            ENDCG
        }
    }
}