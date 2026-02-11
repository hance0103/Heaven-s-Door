Shader "Custom/SpriteRevealMaskGrid"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _MaskTex ("Reveal Mask (R8)", 2D) = "black" {}
        _GridSize ("Grid Size (W,H)", Vector) = (1,1,0,0)
        _Origin ("Grid Origin (World XY)", Vector) = (0,0,0,0)
        _CellWorldSize ("Cell World Size", Float) = 0.32

        _SilhouetteColor ("Silhouette Color", Color) = (0,0,0,1)
        _SilhouetteMul ("Silhouette Strength", Range(0,1)) = 1

        _InvertMask ("Invert Mask (0/1)", Range(0,1)) = 0
        _MaskedOutTransparent ("Masked-out Transparent (0/1)", Range(0,1)) = 0

        _OutlineColor ("Captured Outline Color", Color) = (1,1,1,1)
        _OutlineWorld ("Outline Thickness (World)", Float) = 0.02
        _OutlineEnable ("Outline Enable (0/1)", Range(0,1)) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "CanUseSpriteAtlas"="True"
            "PreviewType"="Plane"
        }

        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;

            sampler2D _MaskTex;
            float4 _GridSize;
            float4 _Origin;
            float  _CellWorldSize;

            fixed4 _SilhouetteColor;
            float  _SilhouetteMul;

            float  _InvertMask;
            float  _MaskedOutTransparent;

            fixed4 _OutlineColor;
            float  _OutlineWorld;
            float  _OutlineEnable;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                fixed4 color  : COLOR;
            };

            struct v2f
            {
                float4 pos     : SV_POSITION;
                float2 uv      : TEXCOORD0;
                fixed4 color   : COLOR;
                float2 worldXY : TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * _Color;

                float4 w = mul(unity_ObjectToWorld, v.vertex);
                o.worldXY = w.xy;
                return o;
            }

            float SampleMask(int2 ci, float W, float H)
            {
                if (ci.x < 0 || ci.y < 0 || ci.x >= (int)W || ci.y >= (int)H)
                    return 0.0;

                float2 uv = (float2(ci) + 0.5) / float2(W, H);
                return tex2D(_MaskTex, uv).r;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 main = tex2D(_MainTex, i.uv) * i.color;
                if (main.a <= 0.001) return 0;

                float W = _GridSize.x;
                float H = _GridSize.y;

                if (W <= 0.5 || H <= 0.5 || _CellWorldSize <= 1e-6)
                {
                    fixed4 fallback = main;
                    fallback.rgb = lerp(main.rgb, _SilhouetteColor.rgb, _SilhouetteMul);
                    return fallback;
                }

                float2 cellF = (i.worldXY - _Origin.xy) / _CellWorldSize;

                int2 ci = (int2)floor(cellF);
                float2 f = frac(cellF);

                float m = SampleMask(ci, W, H);
                m = lerp(m, 1.0 - m, _InvertMask);

                fixed4 maskedOut = main;
                maskedOut.rgb = lerp(main.rgb, _SilhouetteColor.rgb, _SilhouetteMul);
                maskedOut = lerp(maskedOut, fixed4(0,0,0,0), _MaskedOutTransparent);

                fixed4 col = lerp(maskedOut, main, m);

                if (_OutlineEnable > 0.5 && m > 0.5)
                {
                    float ml = SampleMask(ci + int2(-1, 0), W, H);
                    float mr = SampleMask(ci + int2( 1, 0), W, H);
                    float md = SampleMask(ci + int2( 0,-1), W, H);
                    float mu = SampleMask(ci + int2( 0, 1), W, H);

                    ml = lerp(ml, 1.0 - ml, _InvertMask);
                    mr = lerp(mr, 1.0 - mr, _InvertMask);
                    md = lerp(md, 1.0 - md, _InvertMask);
                    mu = lerp(mu, 1.0 - mu, _InvertMask);

                    float t = saturate(_OutlineWorld / _CellWorldSize);

                    float o = 0.0;
                    if (ml < 0.5 && f.x < t) o = 1.0;
                    if (mr < 0.5 && f.x > 1.0 - t) o = 1.0;
                    if (md < 0.5 && f.y < t) o = 1.0;
                    if (mu < 0.5 && f.y > 1.0 - t) o = 1.0;

                    col.rgb = lerp(col.rgb, _OutlineColor.rgb, o * _OutlineColor.a);
                    col.a = max(col.a, o * _OutlineColor.a);
                }

                return col;
            }
            ENDCG
        }
    }
}
