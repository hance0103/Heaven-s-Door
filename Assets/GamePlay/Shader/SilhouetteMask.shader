Shader "Custom/SilhouetteCutByGridMask"
{
    Properties
    {
        [PerRendererData]_MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Silhouette Color", Color) = (0,0,0,1)

        _MaskTex ("Mask (R8/Alpha8)", 2D) = "black" {}
        _GridSize ("Grid Size (W,H)", Vector) = (1,1,0,0)
        _Origin ("Grid Origin (World XY)", Vector) = (0,0,0,0)
        _CellWorldSize ("Cell World Size", Float) = 0.32

        _InvertMask ("Invert (0/1)", Range(0,1)) = 0
        _Threshold ("Threshold", Range(0,1)) = 0.5
        _Softness ("Softness", Range(0,0.5)) = 0
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
            float4 _GridSize;
            float4 _Origin;
            float  _CellWorldSize;

            float _InvertMask;
            float _Threshold;
            float _Softness;

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
                fixed4 tex = tex2D(_MainTex, i.uv);

                float baseA = tex.a * i.color.a;
                if (baseA <= 0.001) return 0;

                float W = _GridSize.x;
                float H = _GridSize.y;

                // 세팅이 안 되면 전체 실루엣
                if (W <= 0.5 || H <= 0.5 || _CellWorldSize <= 0.00001)
                {
                    fixed4 outCol = i.color;
                    outCol.a = baseA;
                    return outCol;
                }

                float2 cell = (i.worldXY - _Origin.xy) / _CellWorldSize;
                float2 maskUV = (cell + 0.5) / float2(W, H);

                float m = 0.0;
                if (maskUV.x >= 0.0 && maskUV.x <= 1.0 && maskUV.y >= 0.0 && maskUV.y <= 1.0)
                    m = tex2D(_MaskTex, maskUV).r; // 0..1

                m = lerp(m, 1.0 - m, _InvertMask);

                // m=1(점령)일수록 실루엣이 사라지게
                // Softness는 나중에 블러/그라데이션 마스크를 쓰고 싶을 때를 대비한 옵션
                float t0 = _Threshold - _Softness;
                float t1 = _Threshold + _Softness;
                float reveal = (_Softness > 0.0001) ? smoothstep(t0, t1, m) : step(_Threshold, m);

                float a = baseA * (1.0 - reveal);

                fixed4 outCol = i.color;
                outCol.a = a;
                return outCol;
            }
            ENDCG
        }
    }
}
