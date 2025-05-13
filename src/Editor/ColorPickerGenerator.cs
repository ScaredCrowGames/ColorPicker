#if UNITY_EDITOR && UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;

namespace Editor.ColorPicker
{
    public class ColorPickerGenerator : EditorWindow
    {
        private int _columns = 8;
        private int _textureSize = 512;
        private string _outputPath = "Assets/GeneratedPalette.png";
        private Texture2D _generatedTexture;
        private Vector2 _scroll;
        private List<Color> _colorList = new();
        private float _colorTolerance = 0.05f;

        [Serializable]
        class ColorPresetData
        {
            public string name;
            public int columns;
            public int textureSize;
            public List<Color> colors = new();
        }

        [MenuItem("Tools/ColorPickerGenerator")]
        public static void OpenWindow()
        {
            GetWindow<ColorPickerGenerator>("Color Picker Generator");
        }

        private void OnGUI()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            EditorGUILayout.LabelField("Configuration", EditorStyles.boldLabel);
            _columns = EditorGUILayout.IntSlider("Columns", _columns, 1, 128);
            _textureSize = EditorGUILayout.IntPopup("Texture Size", _textureSize, new[] { "128", "256", "512", "1024", "2048", "4096" }, new[] { 128, 256, 512, 1024, 2048, 4096 });

            EditorGUILayout.BeginHorizontal();
            _outputPath = EditorGUILayout.TextField("Output Path", _outputPath);
            if (GUILayout.Button("...", GUILayout.MaxWidth(30)))
            {
                var directory = Path.GetDirectoryName(_outputPath);

                if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
                {
                    directory = "Assets";
                }

                var selectedPath = EditorUtility.SaveFilePanel("Select Output Texture File", directory, Path.GetFileName(_outputPath), "png");

                if (!string.IsNullOrEmpty(selectedPath))
                {
                    _outputPath = selectedPath;
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(2);
            EditorGUILayout.LabelField("Colors", EditorStyles.boldLabel);
            DrawColorGrid();

            if (GUILayout.Button("Generate Texture"))
            {
                GenerateTexture(_colorList.ToArray(), _columns, _textureSize, _outputPath);
            }

            if (_generatedTexture != null)
            {
                GUILayout.Label("Preview:");
                var previewRect = GUILayoutUtility.GetRect(_textureSize, _textureSize, GUILayout.ExpandWidth(false));
                EditorGUI.DrawPreviewTexture(previewRect, _generatedTexture);
            }

            EditorGUILayout.Space(2);
            EditorGUILayout.LabelField("Import from image", EditorStyles.boldLabel);
            _colorTolerance = EditorGUILayout.Slider("Color Tolerance", _colorTolerance, 0f, 1f);

            if (GUILayout.Button("Select source image"))
            {
                var path = EditorUtility.OpenFilePanel("Select Palette Image", "Assets", "png");

                if (!string.IsNullOrEmpty(path))
                {
                    var texData = File.ReadAllBytes(path);
                    var tempTex = new Texture2D(2, 2);

                    tempTex.LoadImage(texData);

                    AutoFillColorsFromTexture(tempTex);
                }
            }
            EditorGUILayout.Space(2);

            EditorGUILayout.LabelField("Presets", EditorStyles.boldLabel);
            if (GUILayout.Button("Save Preset"))
            {
                var path = EditorUtility.SaveFilePanel("Save Color Preset", "Assets", "NewColorPreset", "json");

                if (!string.IsNullOrEmpty(path))
                {
                    SavePreset(path);
                }
            }

            if (GUILayout.Button("Load Preset"))
            {
                var path = EditorUtility.OpenFilePanel("Load Color Preset", "Assets", "json");

                if (!string.IsNullOrEmpty(path))
                {
                    LoadPreset(path);
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawColorGrid()
        {
            var itemSize = 50;
            var rowCount = Mathf.CeilToInt(_colorList.Count / (float)_columns);

            for (var row = 0; row < rowCount; row++)
            {
                EditorGUILayout.BeginHorizontal();

                for (var col = 0; col < _columns; col++)
                {
                    var index = row * _columns + col;

                    if (index >= _colorList.Count)
                    {
                        break;
                    }

                    EditorGUILayout.BeginVertical(GUILayout.Width(itemSize));
                    _colorList[index] = EditorGUILayout.ColorField(_colorList[index], GUILayout.Width(itemSize), GUILayout.Height(itemSize));

                    if (GUILayout.Button("X", GUILayout.Width(itemSize)))
                    {
                        _colorList.RemoveAt(index);
                        GUIUtility.ExitGUI();
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("+ Add Color"))
            {
                _colorList.Add(Color.white);
            }
        }

        private void GenerateTexture(Color[] palette, int columns, int texSize, string savePath)
        {
            var cellSize = texSize / columns;
            var texture = new Texture2D(texSize, texSize, TextureFormat.RGBA32, false);

            texture.filterMode = FilterMode.Point;

            for (var y = 0; y < texSize; y++)
            {
                for (var x = 0; x < texSize; x++)
                {
                    texture.SetPixel(x, y, new Color(0, 0, 0, 0));
                }
            }

            for (var i = 0; i < palette.Length; i++)
            {
                var col = i % columns;
                var row = i / columns;

                if ((row + 1) * cellSize > texSize)
                {
                    break;
                }

                var color = palette[i];

                for (var y = 0; y < cellSize; y++)
                {
                    for (var x = 0; x < cellSize; x++)
                    {
                        var px = col * cellSize + x;
                        var py = texSize - (row + 1) * cellSize + y;

                        if (px < texSize && py < texSize)
                        {
                            texture.SetPixel(px, py, color);
                        }
                    }
                }
            }

            texture.Apply();

            SaveTexture(texture, savePath);

            _generatedTexture = texture;
        }

        private void SaveTexture(Texture2D texture, string path)
        {
            var dir = Path.GetDirectoryName(path);

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var pngData = texture.EncodeToPNG();

            File.WriteAllBytes(path, pngData);
            AssetDatabase.ImportAsset(path);

            if (AssetImporter.GetAtPath(path) is TextureImporter importer)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.filterMode = FilterMode.Point;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.maxTextureSize = GetNearestValidTextureSize(_textureSize);
                importer.SaveAndReimport();
            }
        }

        private void SavePreset(string path)
        {
            var preset = new ColorPresetData
            {
                name = Path.GetFileNameWithoutExtension(path),
                colors = new List<Color>(_colorList),
                columns = _columns,
                textureSize = _textureSize
            };

            var json = JsonUtility.ToJson(preset, true);

            File.WriteAllText(path, json);
        }

        private void LoadPreset(string path)
        {
            if (!File.Exists(path))
            {
                return;
            }

            var json = File.ReadAllText(path);
            var preset = JsonUtility.FromJson<ColorPresetData>(json);

            if (preset?.colors != null)
            {
                _colorList = new (preset.colors);
                _columns = preset.columns;
                _textureSize = preset.textureSize;

                Repaint();
            }
        }

        private void AutoFillColorsFromTexture(Texture2D texture)
        {
            var cellSize = EstimateCellSizeByGrid(texture, out int columns, out int rows);
            var result = new List<Color>();

            for (var row = 0; row < rows; row++)
            {
                for (var col = 0; col < columns; col++)
                {
                    var x = col * cellSize + cellSize / 2;
                    var y = texture.height - (row * cellSize + cellSize / 2);

                    if (x >= texture.width || y < 0)
                    {
                        continue;
                    }

                    var color = texture.GetPixel(x, y);

                    if (!IsBorderColor(color))
                    {
                        result.Add(color);
                    }
                }
            }

            _colorList = result;
            _columns = columns;

            Repaint();
        }

        private int EstimateCellSizeByGrid(Texture2D texture, out int columns, out int rows)
        {
            var midY = texture.height / 2;
            var transitions = new List<int>();
            var lastColor = texture.GetPixel(0, midY);

            for (var x = 1; x < texture.width; x++)
            {
                var currentColor = texture.GetPixel(x, midY);

                if (!AreColorsSimilar(lastColor, currentColor))
                {
                    transitions.Add(x);
                    lastColor = currentColor;
                }
            }

            if (transitions.Count < 2)
            {
                columns = 1;
                rows = 1;

                return texture.width;
            }

            var avgSpacing = (transitions.Last() - transitions.First()) / (float)(transitions.Count - 1);
            var cellSize = Mathf.RoundToInt(avgSpacing);

            columns = texture.width / cellSize;
            rows = texture.height / cellSize;

            return cellSize;
        }


        private bool AreColorsSimilar(Color a, Color b)
        {
            return Mathf.Abs(a.r - b.r) < _colorTolerance &&
                   Mathf.Abs(a.g - b.g) < _colorTolerance &&
                   Mathf.Abs(a.b - b.b) < _colorTolerance;
        }

        private bool IsBorderColor(Color c)
        {
            return AreColorsSimilar(c, Color.white) || AreColorsSimilar(c, Color.clear);
        }

        private int GetNearestValidTextureSize(int size)
        {
            var validSizes = new[] { 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192 };
            var closest = validSizes[0];
            var minDiff = Mathf.Abs(size - closest);

            for (var i = 1; i < validSizes.Length; i++)
            {
                var diff = Mathf.Abs(size - validSizes[i]);

                if (diff < minDiff)
                {
                    closest = validSizes[i];
                    minDiff = diff;
                }
            }

            return closest;
        }
    }
}
#endif