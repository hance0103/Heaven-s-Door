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
            float4 _GridSize;       // (W, H, 0, 0)
            float4 _Origin;         // (worldX, worldY, 0, 0)
            float  _CellWorldSize;

            fixed4 _SilhouetteColor;
            float  _SilhouetteMul;

            float  _InvertMask;
            float  _MaskedOutTransparent;

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

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 main = tex2D(_MainTex, i.uv) * i.color;
                if (main.a <= 0.001) return 0;

                float W = _GridSize.x;
                float H = _GridSize.y;

                // 안전장치: 세팅이 안 되면 그냥 실루엣(=마스크 0) 처리
                if (W <= 0.5 || H <= 0.5 || _CellWorldSize <= 0.00001)
                {
                    fixed4 fallback = main;
                    fallback.rgb = lerp(main.rgb, _SilhouetteColor.rgb, _SilhouetteMul);
                    return fallback;
                }

                // world -> cell
                float2 cell = (i.worldXY - _Origin.xy) / _CellWorldSize;

                // cell -> mask uv (텍스처 픽셀 중앙을 샘플)
                float2 maskUV = (cell + 0.5) / float2(W, H);

                // 범위 밖은 마스크 0(가려짐)로 취급
                float m = 0.0;
                if (maskUV.x >= 0.0 && maskUV.x <= 1.0 && maskUV.y >= 0.0 && maskUV.y <= 1.0)
                {
                    m = tex2D(_MaskTex, maskUV).r; // 0..1
                }

                // 마스크 반전 옵션
                m = lerp(m, 1.0 - m, _InvertMask);

                // masked-out(=m=0)일 때의 색
                fixed4 maskedOut = main;

                // 실루엣 강도: 0이면 원본, 1이면 실루엣색
                maskedOut.rgb = lerp(main.rgb, _SilhouetteColor.rgb, _SilhouetteMul);

                // 필요하면 아예 투명
                maskedOut = lerp(maskedOut, fixed4(0,0,0,0), _MaskedOutTransparent);

                // m=0 -> maskedOut, m=1 -> main
                return lerp(maskedOut, main, m);
            }
            ENDCG
        }
    }
}
