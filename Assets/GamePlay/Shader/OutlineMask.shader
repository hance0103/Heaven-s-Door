Shader "Custom/CapturedOutlineByGridMask"
{
    Properties
    {
        [PerRendererData]_MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Outline Color", Color) = (1,1,1,1)

        _MaskTex ("Mask (R8/Alpha8)", 2D) = "black" {}
        _GridSize ("Grid Size (W,H)", Vector) = (1,1,0,0)
        _Origin ("Grid Origin (World XY)", Vector) = (0,0,0,0)
        _CellWorldSize ("Cell World Size", Float) = 0.32

        _Thickness01 ("Thickness (0..0.5 of cell)", Range(0,0.5)) = 0.08
        _InvertMask ("Invert (0/1)", Range(0,1)) = 0
        _Threshold ("Mask Threshold", Range(0,1)) = 0.5
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "CanUseSpriteAtlas"="True"
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
            float4 _GridSize;       // (W,H,0,0)
            float4 _Origin;         // world XY
            float  _CellWorldSize;

            float _Thickness01;
            float _InvertMask;
            float _Threshold;

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

            float SampleMaskCell(int cx, int cy, float2 gridSize)
            {
                if (cx < 0 || cy < 0 || cx >= (int)gridSize.x || cy >= (int)gridSize.y) return 0.0;

                float2 uv = (float2(cx, cy) + 0.5) / gridSize;
                float m = tex2D(_MaskTex, uv).r;
                m = lerp(m, 1.0 - m, _InvertMask);
                return m;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 tex = tex2D(_MainTex, i.uv);
                float baseA = tex.a * i.color.a;
                if (baseA <= 0.001) return 0;

                float2 gridSize = _GridSize.xy;
                if (gridSize.x <= 0.5 || gridSize.y <= 0.5 || _CellWorldSize <= 0.00001) return 0;

                // cell 좌표: _Origin은 "셀(0,0) 중심" 기준
                float2 cell = (i.worldXY - _Origin.xy) / _CellWorldSize;

                // 중심 기준을 코너 기준 로컬(0..1)로 바꾸기 위해 +0.5
                float2 cellCorner = cell + 0.5;

                int cx = (int)floor(cellCorner.x);
                int cy = (int)floor(cellCorner.y);

                // 현재 셀 내부 로컬(0..1)
                float2 local = frac(cellCorner);

                float m = SampleMaskCell(cx, cy, gridSize);
                float filled = step(_Threshold, m);
                if (filled <= 0.0) return 0;

                float fl = step(_Threshold, SampleMaskCell(cx - 1, cy, gridSize));
                float fr = step(_Threshold, SampleMaskCell(cx + 1, cy, gridSize));
                float fd = step(_Threshold, SampleMaskCell(cx, cy - 1, gridSize));
                float fu = step(_Threshold, SampleMaskCell(cx, cy + 1, gridSize));

                float t = _Thickness01;

                float edgeL = (1.0 - fl) * step(local.x, t);
                float edgeR = (1.0 - fr) * step(1.0 - t, local.x);
                float edgeD = (1.0 - fd) * step(local.y, t);
                float edgeU = (1.0 - fu) * step(1.0 - t, local.y);

                float edge = saturate(edgeL + edgeR + edgeD + edgeU);
                if (edge <= 0.0) return 0;

                fixed4 outCol = i.color;
                outCol.a = baseA;
                return outCol;
            }
            ENDCG
        }
    }
}
