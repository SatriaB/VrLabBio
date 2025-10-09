Shader "Custom/WobbleCapsuleLiquidTiltFixed"
{
    Properties
    {
        _MainTex("Liquid Texture", 2D) = "white" {}
        _Color("Liquid Color", Color) = (0,0.5,1,0.6)
        _Fill("Fill Amount", Range(0,1)) = 0.5
        _WobbleSpeed("Wobble Speed", Range(0,10)) = 2
        _WobbleStrength("Wobble Strength", Range(0,0.1)) = 0.05
        _Radius("Capsule Radius", Float) = 0.5
        _HalfHeight("Capsule Half Height", Float) = 1.0
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
            float _Radius;
            float _HalfHeight;
            float _SurfaceThickness;
            float4 _TiltNormal; // expected in object/local space

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
                o.localPos = v.vertex.xyz; // object space position
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            // Signed distance to a vertical capsule centered at origin (object-space)
            float sdCapsuleY(float3 p, float halfHeight, float radius)
            {
                float2 d = float2(length(p.xz), abs(p.y) - halfHeight);
                return min(max(d.x, d.y), 0.0) + length(max(d, 0.0)) - radius;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Make fill range include spherical caps so "1" reaches the dome
                float fullExtent = _HalfHeight + _Radius;
                float fillHeight = lerp(-fullExtent, fullExtent, saturate(_Fill));

                // Wobble (object-space / world-time driven)
                float wobble = sin(i.worldPos.x * 4 + _Time.y * _WobbleSpeed) * _WobbleStrength
                             + cos(i.worldPos.z * 4 + _Time.y * _WobbleSpeed) * _WobbleStrength;

                // Use tilt normal in OBJECT space (passed from script)
                float3 n = normalize(_TiltNormal.xyz);

                // plane value (projection of local position onto tilt normal)
                float surfaceVal = dot(i.localPos, n);
                float surfaceFill = fillHeight + wobble;

                // check inside capsule
                float dist = sdCapsuleY(i.localPos, _HalfHeight, _Radius);

                if (dist <= 0)
                {
                    // Surface band (highlight)
                    if (abs(surfaceVal - surfaceFill) < _SurfaceThickness)
                    {
                        fixed4 surf = tex2D(_MainTex, i.uv) * _Color;
                        surf.rgb *= 1.15; // slight brighten on rim
                        return surf;
                    }

                    // Body below plane -> filled
                    if (surfaceVal < surfaceFill)
                    {
                        fixed4 tex = tex2D(_MainTex, i.uv);
                        return tex * _Color;
                    }
                }

                return fixed4(0,0,0,0); // transparent
            }
            ENDCG
        }
    }
}
