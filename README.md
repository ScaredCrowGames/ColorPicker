# Color Picker for Unity

A simple color picker tool for Unity.

![PaletteTextureExample](https://github.com/user-attachments/assets/55bb70b8-3d85-41a2-8f23-d8c3ae6d0c23)
---
## Table of Contents
- [Installation](#installation)
- [Palette Texture Generator](#palette-texture-generator)
- [Manual setup](#manual-setup)
- [Samples](#samples)
- [How to use](#how-to-use)

## Installation

1. Use the Package Manager:

Window > Package Manager > Add package from git url...
```link
https://github.com/scaredcrowgames/colorpicker.git?path=src
```

2. Or add this to your Unity project's `Packages/manifest.json`:

```json
"com.scaredcrowgames.colorpicker": "https://github.com/scaredcrowgames/colorpicker.git?path=src"
```
## Palette Texture Generator
Access from the top menu: Tools > ColorPickerGenerator

Features:
* Add, remove, reorder colors
* Import colors from image
* Save/load color presets as .json files
* Configure texture size and column layout
* Save and auto-setup generated texture in .png format

## Manual setup:
> [!TIP]
> You can use ready-to-use prefab from Samples (skip this section if so).
1. Generate (see above) or import your color palette texture
2. Create UI image and assign the texture
3. Create child image (raycast target should be disabled, rect middle-center, size is no matter)
4. Assign outline texture to child image
5. Attach ColorPicker component to any object and assign palette image and outline rect in Inspector.

## Samples
The package includes:

📁 Samples/ contains ColorPickerDemo.unity, palette and outline images, preset example and ready-to-use prefab

Import it from Package Manager > Samples

## How to use
The ColorPicker.cs (MonoBehaviour) allows to select colors from the palette at runtime.
Subscribe to ColorSelectionChanged
```csharp
colorPicker.ColorSelectionChanged += color => {
    Debug.Log("Selected color: " + color);
};
```
Or get current selected color directly:
```csharp
var currentSelectedColor = colorPicker.CurrentSelectedColor;
```
> [!NOTE]
> Default color before the very first selection is white-transparent

## Contact us
> [!TIP]
> All discussions, requests and bug reports can be left in the corresponding [Discord channel](https://discord.gg/kWvJeWHPZ9) or here in Discussions
