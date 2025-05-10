using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ColorPicker
{
    public class ColorPicker : MonoBehaviour, IPointerClickHandler
    {
        public event Action<Color> ColorSelectionChanged;

        public Color CurrentSelectedColor { get; private set; } = new (1, 1, 1, 0);

        [SerializeField] private Image _paletteImage;
        [SerializeField] private RectTransform _outline;
        [SerializeField, Range(0, 1)] private float _colorMatchTolerance = 0.01f;
        [SerializeField, Range(0, 1)] private float _colorAlphaTolerance = 0.1f;

        private Texture2D _texture;
        private RectTransform _paletteRectTransform;
        private Rect _paletteSpriteRect;

        private void Start()
        {
            if (_outline && CurrentSelectedColor.a == 0)
            {
                _outline.gameObject.SetActive(false);
            }

            if (!_paletteImage?.sprite)
            {
                Debug.LogError("Palette image or sprite not assigned.");
                enabled = false;

                return;
            }

            _texture = _paletteImage.sprite.texture;
            _paletteRectTransform = _paletteImage.rectTransform;
            _paletteSpriteRect = _paletteImage.sprite.rect;

            _paletteImage.alphaHitTestMinimumThreshold = _colorAlphaTolerance;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _paletteRectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out var localPoint))
            {
                return;
            }

            var rect = _paletteRectTransform.rect;
            var spriteSize = _paletteSpriteRect.size;
            var spriteRatio = spriteSize.x / spriteSize.y;
            var rectRatio = rect.width / rect.height;

            var drawWidth = rect.width;
            var drawHeight = rect.height;

            if (spriteRatio > rectRatio)
            {
                drawHeight = drawWidth / spriteRatio;
            }
            else
            {
                drawWidth = drawHeight * spriteRatio;
            }

            var offsetX = (rect.width - drawWidth) * 0.5f;
            var offsetY = (rect.height - drawHeight) * 0.5f;

            var pivot = _paletteRectTransform.pivot;
            var pixelX = localPoint.x + rect.width * pivot.x - offsetX;
            var pixelY = localPoint.y + rect.height * pivot.y - offsetY;


            if (pixelX < 0 || pixelY < 0 || pixelX > drawWidth || pixelY > drawHeight)
            {
                return;
            }

            var uvX = pixelX / drawWidth;
            var uvY = pixelY / drawHeight;

            var texX = (int)(_paletteSpriteRect.x + uvX * _paletteSpriteRect.width);
            var texY = (int)(_paletteSpriteRect.y + uvY * _paletteSpriteRect.height);

            if (!IsInTexture(texX, texY))
            {
                return;
            }

            var targetColor = _texture.GetPixel(texX, texY);

            if (targetColor.a < _colorAlphaTolerance)
            {
                return;
            }

            var left = texX;
            var right = texX;
            var top = texY;
            var bottom = texY;

            while (left > _paletteSpriteRect.x && ColorsMatch(_texture.GetPixel(left - 1, texY), targetColor)) left--;
            while (right < _paletteSpriteRect.xMax - 1 && ColorsMatch(_texture.GetPixel(right + 1, texY), targetColor)) right++;
            while (bottom > _paletteSpriteRect.y && ColorsMatch(_texture.GetPixel(texX, bottom - 1), targetColor)) bottom--;
            while (top < _paletteSpriteRect.yMax - 1 && ColorsMatch(_texture.GetPixel(texX, top + 1), targetColor)) top++;

            var centerX = (left + right + 1) / 2f;
            var centerY = (bottom + top + 1) / 2f;
            var cellW = right - left + 1;
            var cellH = top - bottom + 1;

            var normX = (centerX - _paletteSpriteRect.x) / _paletteSpriteRect.width;
            var normY = (centerY - _paletteSpriteRect.y) / _paletteSpriteRect.height;
            var normW = cellW / _paletteSpriteRect.width;
            var normH = cellH / _paletteSpriteRect.height;

            var outlineX = offsetX + normX * drawWidth - rect.width * 0.5f;
            var outlineY = offsetY + normY * drawHeight - rect.height * 0.5f;
            var outlineW = normW * drawWidth;
            var outlineH = normH * drawHeight;

            _outline.anchorMin = _outline.anchorMax = _outline.pivot = new (0.5f, 0.5f);
            _outline.anchoredPosition = new(outlineX, outlineY);
            _outline.sizeDelta = new(outlineW, outlineH);
            _outline.gameObject.SetActive(true);

            CurrentSelectedColor = targetColor;

            ColorSelectionChanged?.Invoke(CurrentSelectedColor);
        }

        private bool IsInTexture(int x, int y)
        {
            return x >= 0 && y >= 0 && x < _texture.width && y < _texture.height;
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