using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ColorPicker
{
    public class ColorPicker : MonoBehaviour, IPointerClickHandler
    {
        private static readonly Color DefaultColor = new(1, 1, 1, 0);
        private static readonly Vector2 CenterPivot = new(0.5f, 0.5f);

        public event Action<Color> ColorSelectionChanged;
        public Color CurrentSelectedColor { get; private set; } = DefaultColor;

        [SerializeField] private Image _paletteImage;
        [SerializeField] private RectTransform _outline;
        [SerializeField, Range(0, 1)] private float _colorMatchTolerance = 0.01f;
        [SerializeField, Range(0, 1)] private float _colorAlphaTolerance = 0.1f;

        private Texture2D _texture;
        private RectTransform _rectTransform;
        private Rect _spriteRect;

        private void Start()
        {
            if (!_paletteImage?.sprite)
            {
                Debug.LogError("Palette image or sprite not assigned.");
                enabled = false;

                return;
            }

            _texture = _paletteImage.sprite.texture;
            _spriteRect = _paletteImage.sprite.rect;
            _rectTransform = _paletteImage.rectTransform;

            _paletteImage.alphaHitTestMinimumThreshold = _colorAlphaTolerance;

            if (_outline)
            {
                _outline.gameObject.SetActive(false);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _rectTransform, eventData.position, eventData.pressEventCamera, out var localPoint))
            {
                return;
            }

            var uv = _paletteImage.preserveAspect
                ? GetUVWithPreserveAspect(localPoint)
                : GetUVWithoutPreserveAspect(localPoint);

            if (uv == null)
            {
                return;
            }

            var texCoords = GetTextureCoords(uv.Value);

            if (!IsInTexture(texCoords))
            {
                return;
            }

            var color = _texture.GetPixel(texCoords.x, texCoords.y);

            if (color.a < _colorAlphaTolerance)
            {
                return;
            }

            var bounds = FloodFillBounds(texCoords, color);

            DrawOutline(bounds, uvSpace: uv.Value);

            CurrentSelectedColor = color;
            ColorSelectionChanged?.Invoke(CurrentSelectedColor);
        }

        private Vector2? GetUVWithPreserveAspect(Vector2 localPoint)
        {
            var rect = _rectTransform.rect;
            var pivot = _rectTransform.pivot;
            var spriteSize = _spriteRect.size;
            var spriteRatio = spriteSize.x / spriteSize.y;
            var rectRatio = rect.width / rect.height;

            float drawWidth = rect.width, drawHeight = rect.height;

            if (spriteRatio > rectRatio)
            {
                drawHeight = drawWidth / spriteRatio;
            }
            else
            {
                drawWidth = drawHeight * spriteRatio;
            }

            var offset = new Vector2((rect.width - drawWidth) / 2, (rect.height - drawHeight) / 2);
            var pixel = new Vector2(localPoint.x + rect.width * pivot.x - offset.x, localPoint.y + rect.height * pivot.y - offset.y);

            if (pixel.x < 0 || pixel.y < 0 || pixel.x > drawWidth || pixel.y > drawHeight)
            {
                return null;
            }

            return new Vector2(pixel.x / drawWidth, pixel.y / drawHeight);
        }

        private Vector2? GetUVWithoutPreserveAspect(Vector2 localPoint)
        {
            var rect = _rectTransform.rect;
            var pivot = _rectTransform.pivot;
            var pixel = new Vector2(localPoint.x + rect.width * pivot.x, localPoint.y + rect.height * pivot.y);

            if (pixel.x < 0 || pixel.y < 0 || pixel.x > rect.width || pixel.y > rect.height)
            {
                return null;
            }

            return new Vector2(pixel.x / rect.width, pixel.y / rect.height);
        }

        private Vector4 FloodFillBounds(Vector2Int texCoords, Color targetColor)
        {
            int left = texCoords.x, right = texCoords.x, top = texCoords.y, bottom = texCoords.y;

            while (left > _spriteRect.x && ColorsMatch(_texture.GetPixel(left - 1, texCoords.y), targetColor)) left--;
            while (right < _spriteRect.xMax - 1 && ColorsMatch(_texture.GetPixel(right + 1, texCoords.y), targetColor)) right++;
            while (bottom > _spriteRect.y && ColorsMatch(_texture.GetPixel(texCoords.x, bottom - 1), targetColor)) bottom--;
            while (top < _spriteRect.yMax - 1 && ColorsMatch(_texture.GetPixel(texCoords.x, top + 1), targetColor)) top++;

            var centerX = (left + right + 1) / 2f;
            var centerY = (bottom + top + 1) / 2f;

            var normX = (centerX - _spriteRect.x) / _spriteRect.width;
            var normY = (centerY - _spriteRect.y) / _spriteRect.height;

            var normW = Mathf.Max(1f / _spriteRect.width, (right - left + 1) / _spriteRect.width);
            var normH = Mathf.Max(1f / _spriteRect.height, (top - bottom + 1) / _spriteRect.height);

            return new Vector4(normX, normY, normW, normH);
        }

        private void DrawOutline(Vector4 bounds, Vector2 uvSpace)
        {
            var rect = _rectTransform.rect;
            var (normX, normY, normW, normH) = (bounds.x, bounds.y, bounds.z, bounds.w);
            var width = _paletteImage.preserveAspect ? rect.width : rect.width;
            var height = _paletteImage.preserveAspect ? rect.height : rect.height;

            if (_paletteImage.preserveAspect)
            {
                var spriteSize = _spriteRect.size;
                var spriteRatio = spriteSize.x / spriteSize.y;
                var rectRatio = rect.width / rect.height;

                if (spriteRatio > rectRatio)
                {
                    height = width / spriteRatio;
                }
                else
                {
                    width = height * spriteRatio;
                }
            }

            float offsetX = (_paletteImage.preserveAspect ? (rect.width - width) / 2 : 0);
            float offsetY = (_paletteImage.preserveAspect ? (rect.height - height) / 2 : 0);

            float outlineX = offsetX + normX * width - rect.width * 0.5f;
            float outlineY = offsetY + normY * height - rect.height * 0.5f;
            float outlineW = normW * width;
            float outlineH = normH * height;

            _outline.anchorMin = _outline.anchorMax = _outline.pivot = CenterPivot;
            _outline.anchoredPosition = new Vector2(outlineX, outlineY);
            _outline.sizeDelta = new Vector2(outlineW, outlineH);
            _outline.gameObject.SetActive(true);
        }

        private Vector2Int GetTextureCoords(Vector2 uv)
        {
            var texX = (int)(_spriteRect.x + uv.x * _spriteRect.width);
            var texY = (int)(_spriteRect.y + uv.y * _spriteRect.height);

            return new Vector2Int(texX, texY);
        }

        private bool IsInTexture(Vector2Int coords)
        {
            return coords.x >= 0 && coords.y >= 0 && coords.x < _texture.width && coords.y < _texture.height;
        }

        private bool ColorsMatch(Color a, Color b)
        {
            return Mathf.Abs(a.r - b.r) < _colorMatchTolerance &&
                   Mathf.Abs(a.g - b.g) < _colorMatchTolerance &&
                   Mathf.Abs(a.b - b.b) < _colorMatchTolerance &&
                   Mathf.Abs(a.a - b.a) < _colorMatchTolerance;
        }
    }
}