Shader "Custom/WobbleLiquidUniversal"
{
    Properties
    {
        _MainTex("Liquid Texture", 2D) = "white" {}
        _Color("Liquid Color", Color) = (0,0.5,1,0.6)
        _Fill("Fill Amount", Range(0,1)) = 0.5
        _WobbleSpeed("Wobble Speed", Range(0,10)) = 2
        _WobbleStrength("Wobble Strength", Range(0,0.1)) = 0.05
        _SurfaceThickness("Surface Thickness", Range(0,0.1)) = 0.02
        _TiltNormal("Surface Tilt Normal (object-space)", Vector) = (0,1,0,0)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _Fill;
            float _WobbleSpeed;
            float _WobbleStrength;
            float _SurfaceThickness;
            float4 _TiltNormal;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 localPos : TEXCOORD1;
                float2 uv : TEXCOORD2;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.localPos = v.vertex.xyz; // object space
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Normalize fill into object’s local bounds
                float3 minB = unity_ObjectToWorld._m30_m31_m32; // fallback (not true bounds)
                // Instead: assume fill maps from mesh bounds in Y:
                float minY = -0.5; // normalized space
                float maxY = 0.5;
                float fillHeight = lerp(minY, maxY, _Fill);

                // wobble
                float wobble = sin(i.worldPos.x * 4 + _Time.y * _WobbleSpeed) * _WobbleStrength
                             + cos(i.worldPos.z * 4 + _Time.y * _WobbleSpeed) * _WobbleStrength;

                float3 n = normalize(_TiltNormal.xyz);
                float surfaceVal = dot(i.localPos, n);
                float surfaceFill = fillHeight + wobble;

                // surface band highlight
                if (abs(surfaceVal - surfaceFill) < _SurfaceThickness)
                {
                    fixed4 surf = tex2D(_MainTex, i.uv) * _Color;
                    surf.rgb *= 1.2;
                    return surf;
                }

                // below plane → filled
                if (surfaceVal < surfaceFill)
                {
                    fixed4 tex = tex2D(_MainTex, i.uv);
                    return tex * _Color;
                }

                return fixed4(0,0,0,0);
            }
            ENDCG
        }
    }
}
