# Color Picker for Unity

A simple color picker tool for Unity.

![PaletteTextureExample](https://github.com/user-attachments/assets/55bb70b8-3d85-41a2-8f23-d8c3ae6d0c23)
---
## Table of Contents
- [Installation](#installation)
- [Palette Texture Generator](#palette-texture-generator)
- [How to use](#how-to-use)
- [Samples](#samples)

## Installation

Add this to your Unity project's `Packages/manifest.json`:

```json
"com.scaredcrowgames.colorpicker": "https://github.com/scaredcrowgames/color-picker.git"
```

### Or use the Package Manager:

Window > Package Manager > Add package from git url... > https://github.com/scaredcrowgames/color-picker.git

## Palette Texture Generator
Access from the top menu: Tools > ColorPickerGenerator

Features:
* Add, remove, reorder colors
* Import colors from image
* Save/load color presets as .json files
* Configure texture size and column layout
* Save and auto-setup generated texture in .png format

## How to use
You can use ready-to-use prefab from Samples (skip the section above if so).

The ColorPicker.cs (MonoBehaviour) allows to select colors from the palette at runtime.

### Manual setup:
1. Generate (see above) or import your color palette texture
2. Create UI image and assign the texture
3. Create child image (raycast target should be disabled, rect middle-center, size is no matter)
4. Assign outline texture to child image
5. Attach ColorPicker component to any object and assign palette image and outline rect in Inspector.

Optionally subscribe to ColorSelectionChanged

```csharp
colorPicker.ColorSelectionChanged += color => {
    Debug.Log("Selected color: " + color);
};
```
Or read current selected color directly:
```csharp
var currentSelectedColor = colorPicker.CurrentSelectedColor;
```
> [!NOTE]
> Default color before the very first selection is white-transparent

## Samples
The package includes a full demo:

📁 Samples~/ contains ColorPickerDemo.unity and ready-to-use prefab

Import it from Package Manager > Samples
