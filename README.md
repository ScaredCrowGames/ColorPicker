# Color Picker for Unity

A simple color picker tool for Unity.

![PaletteTextureExample](https://github.com/user-attachments/assets/55bb70b8-3d85-41a2-8f23-d8c3ae6d0c23)
---
## 📦 Installation (via Git URL)

Add this to your Unity project's `Packages/manifest.json`:

```json
"com.scaredcrowgames.colorpicker": "https://github.com/scaredcrowgames/color-picker.git"
```

### Or use the Package Manager:

Window > Package Manager > Add package from git url... > https://github.com/scaredcrowgames/color-picker.git

## 🛠 Editor Tool: Palette Texture Generator
Access from the top menu: Tools > ColorPickerGenerator

Features:
* Add, remove, reorder colors
* Import colors from image
* Save/load color presets as .json files
* Configure texture size and column layout
* Save and auto-setup generated texture in .png format

## 🎮 How to
The ColorPicker.cs (MonoBehaviour) allows to select colors from the palette at runtime.

Setup:
1. Assign a UI Image with your palette texture to _paletteImage
2. Assign a RectTransform for _outline to highlight the selection

Optionally subscribe to ColorSelectionChanged

```csharp
colorPicker.ColorSelectionChanged += color => {
    Debug.Log("Selected color: " + color);
};
```

## 🧪 Demo
The package includes a full demo:

📁 Samples~/ contains ColorPickerDemo.unity and ready-to-use prefab

Import it from Package Manager > Samples