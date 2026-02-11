using System.Collections.Generic;
using UnityEngine;
using GamePlay;

namespace GamePlay.GridMap
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class RevealMaskController : MonoBehaviour
    {
        [SerializeField] private GridManager gridManager;
        [SerializeField] private string maskProperty = "_MaskTex";

        private Texture2D _maskTex;
        private Color32[] _pixels;

        private int _w;
        private int _h;

        private SpriteRenderer _sr;
        private MaterialPropertyBlock _mpb;

        private static readonly int GridSizeId = Shader.PropertyToID("_GridSize");
        private static readonly int OriginId = Shader.PropertyToID("_Origin");
        private static readonly int CellWorldSizeId = Shader.PropertyToID("_CellWorldSize");

        private static readonly Color32 Black = new Color32(0, 0, 0, 255);
        private static readonly Color32 White = new Color32(255, 255, 255, 255);

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            _mpb = new MaterialPropertyBlock();
        }

        private void Start()
        {
            if (gridManager == null) return;

            _w = gridManager.CellWidth;
            _h = gridManager.CellHeight;

            _maskTex = new Texture2D(_w, _h, TextureFormat.R8, false, true);
            _maskTex.filterMode = FilterMode.Point;
            _maskTex.wrapMode = TextureWrapMode.Clamp;

            _pixels = new Color32[_w * _h];
            for (int i = 0; i < _pixels.Length; i++)
                _pixels[i] = Black;

            _maskTex.SetPixels32(_pixels);
            _maskTex.Apply(false);

            ApplyMaterialParams();
            ApplyFromGridAll();
        }

        private void ApplyMaterialParams()
        {
            _sr.GetPropertyBlock(_mpb);

            _mpb.SetTexture(maskProperty, _maskTex);
            _mpb.SetVector(GridSizeId, new Vector4(_w, _h, 0, 0));
            _mpb.SetVector(OriginId, new Vector4(gridManager.Origin.x, gridManager.Origin.y, 0, 0));
            _mpb.SetFloat(CellWorldSizeId, gridManager.CellWorldSize);

            _sr.SetPropertyBlock(_mpb);
        }

        public void RevealCells(IReadOnlyList<Vector2Int> cells)
        {
            if (_maskTex == null || cells == null) return;

            bool changed = false;

            for (int i = 0; i < cells.Count; i++)
            {
                var c = cells[i];
                if (c.x < 0 || c.y < 0 || c.x >= _w || c.y >= _h) continue;

                int idx = c.y * _w + c.x;
                if (_pixels[idx].r == 255) continue;

                _pixels[idx] = White;
                changed = true;
            }

            if (!changed) return;

            _maskTex.SetPixels32(_pixels);
            _maskTex.Apply(false);
        }

        public void ApplyFromGridAll()
        {
            if (_maskTex == null) return;

            for (int x = 0; x < _w; x++)
            for (int y = 0; y < _h; y++)
            {
                if (gridManager.GetCell(x, y) == SystemEnum.eSellState.Filled)
                {
                    int idx = y * _w + x;
                    _pixels[idx] = White;
                }
            }

            _maskTex.SetPixels32(_pixels);
            _maskTex.Apply(false);
        }
    }
}