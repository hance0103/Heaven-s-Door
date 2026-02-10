using System.Collections.Generic;
using UnityEngine;
using GamePlay;

namespace GamePlay.GridMap
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class RevealMaskController : MonoBehaviour
    {
        [SerializeField] private GridManager gridManager;

        // 추가로 같은 마스크를 공유할 렌더러(예: Outline SpriteRenderer)
        [SerializeField] private SpriteRenderer[] extraRenderers;

        // 셰이더에서 마스크 텍스처 프로퍼티 이름(실루엣/아웃라인 셰이더 둘 다 동일하게 _MaskTex 쓰는 걸 추천)
        [SerializeField] private string maskProperty = "_MaskTex";

        private Texture2D _maskTex;
        private Color32[] _pixels;

        private int _w;
        private int _h;

        private SpriteRenderer _sr;
        private MaterialPropertyBlock _mpb;

        private int _maskId;

        private static readonly int GridSizeId = Shader.PropertyToID("_GridSize");
        private static readonly int OriginId = Shader.PropertyToID("_Origin");
        private static readonly int CellWorldSizeId = Shader.PropertyToID("_CellWorldSize");

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            _mpb = new MaterialPropertyBlock();
            _maskId = Shader.PropertyToID(maskProperty);
        }

        private void Start()
        {
            if (gridManager == null) return;

            _w = gridManager.GridSize.x;
            _h = gridManager.GridSize.y;

            _maskTex = new Texture2D(_w, _h, TextureFormat.R8, false, true);
            _maskTex.filterMode = FilterMode.Point;
            _maskTex.wrapMode = TextureWrapMode.Clamp;

            _pixels = new Color32[_w * _h];
            for (int i = 0; i < _pixels.Length; i++)
                _pixels[i] = new Color32(0, 0, 0, 255);

            _maskTex.SetPixels32(_pixels);
            _maskTex.Apply(false);

            ApplyMaterialParamsAll();
            ApplyFromGridAll(); // 초기 점령(시작영역) 반영
        }

        private void ApplyMaterialParamsAll()
        {
            ApplyMaterialParams(_sr);

            if (extraRenderers == null) return;
            for (int i = 0; i < extraRenderers.Length; i++)
            {
                var r = extraRenderers[i];
                if (r == null) continue;
                ApplyMaterialParams(r);
            }
        }

        private void ApplyMaterialParams(SpriteRenderer r)
        {
            r.GetPropertyBlock(_mpb);

            _mpb.SetTexture(_maskId, _maskTex);
            _mpb.SetVector(GridSizeId, new Vector4(_w, _h, 0, 0));
            _mpb.SetVector(OriginId, new Vector4(gridManager.MapOrigin.x, gridManager.MapOrigin.y, 0, 0));
            _mpb.SetFloat(CellWorldSizeId, gridManager.CellWorldSize);

            r.SetPropertyBlock(_mpb);
        }

        // 점령(=Filled)된 셀을 흰색으로 찍기
        public void RevealCells(IReadOnlyList<Vector2Int> cells)
        {
            if (_maskTex == null) return;

            for (int i = 0; i < cells.Count; i++)
            {
                var c = cells[i];
                if (c.x < 0 || c.y < 0 || c.x >= _w || c.y >= _h) continue;

                var idx = c.y * _w + c.x;
                _pixels[idx] = new Color32(255, 255, 255, 255);
            }

            _maskTex.SetPixels32(_pixels);
            _maskTex.Apply(false);
        }

        // 전체를 다시 동기화(중요: Empty는 반드시 검정으로 리셋)
        public void ApplyFromGridAll()
        {
            if (_maskTex == null) return;

            for (int x = 0; x < _w; x++)
            {
                for (int y = 0; y < _h; y++)
                {
                    var idx = y * _w + x;

                    _pixels[idx] = (gridManager.GetCell(x, y) == SystemEnum.eSellState.Filled)
                        ? new Color32(255, 255, 255, 255)
                        : new Color32(0, 0, 0, 255);
                }
            }

            _maskTex.SetPixels32(_pixels);
            _maskTex.Apply(false);
        }
    }
}
