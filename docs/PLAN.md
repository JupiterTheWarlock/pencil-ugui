# Pencil to Unity UGUI Importer Plan

## 1. Project Goal

Build a Unity Editor importer that converts UI panels designed in Pencil into Unity UGUI prefabs.

The first milestone should only support the intersection between:

- Features already handled by `ManakhovN/FigmaToUnityImporter`.
- Features exposed by Pencil's local canvas, `.pen` files, and MCP tools.

This keeps the first version small, testable, and close to a proven Figma-to-UGUI import path.

## 2. Reference Baseline

`FigmaToUnityImporter` is the initial reference implementation. Its useful design choices are:

- Import a design node tree into an existing Unity `Canvas`.
- Preserve node identity in generated GameObject names, using a stable id suffix such as `[nodeId]`.
- Let each node choose an action:
  - `Generate`: create native UGUI objects.
  - `Render`: import a raster image for nodes that are hard to recreate.
  - `Transform`: update only layout and transform.
  - `None`: skip the node.
  - `SvgRender`: optional path when Unity Vector Graphics is installed.
- Generate native UGUI for text, solid fills, simple hierarchy, masks, and transforms.
- Use raster fallback for visual content that does not map cleanly to UGUI.
- Use TextMeshPro for text rendering.
- Link design fonts to Unity TMP fonts through an asset-side mapping.

The important lesson is not to import every design feature natively. The importer should generate native UGUI only when the mapping is reliable.

## 3. Pencil Capability Assumptions

Initial Pencil integration should rely on capabilities that are already aligned with its AI/MCP workflow:

- Read the current design hierarchy through MCP, likely via `batch_get`.
- Inspect current editor state and selection.
- Capture layout information through `snapshot_layout`.
- Read design variables through `get_variables`.
- Render screenshots for visual verification.

The importer should not require Figma cloud APIs. Figma compatibility can remain a research reference, not the production bridge.

## 4. Proposed Architecture

Use a stable intermediate representation instead of binding Unity directly to Pencil's internal format.

Pipeline:

```text
Pencil canvas / .pen / MCP
  -> Pencil extractor
  -> UI IR JSON
  -> Unity Editor importer
  -> UGUI prefab under an existing Canvas
```

Main modules:

- `PencilExtractor`: reads Pencil nodes and exports a normalized UI document.
- `UiIr`: shared JSON schema for nodes, layout, style, text, images, and variables.
- `UnityImporter`: reads UI IR and generates or updates UGUI objects.
- `AssetImporter`: imports raster images, sprites, and font mappings.
- `ImportWindow`: Unity Editor window for selecting source JSON, target Canvas, scale, and node actions.

## 5. MVP Feature Scope

The first version should support only the safe intersection.

P0 native UGUI generation:

- Node hierarchy.
- Stable node id.
- Node name.
- Absolute bounds: `x`, `y`, `width`, `height`.
- Parent-child transform conversion.
- Basic constraints or anchor presets when Pencil exposes equivalent layout data.
- Frame or group to `GameObject + RectTransform`.
- Rectangle with solid fill to `Image`.
- Text to `TextMeshProUGUI`.
- Text content.
- Font size.
- Text color.
- Horizontal and vertical alignment.
- Clipping to `RectMask2D` or `Mask`.
- Raster image to `Sprite + Image`.
- Scale factor for high-resolution raster import.
- Incremental update by stable node id.

P0 fallback behavior:

- Any unsupported leaf visual node can be rasterized.
- Any unsupported parent visual node can be set to `Render`, with children skipped.
- Unsupported style details should be ignored unless they break layout.

## 6. Deferred Features

These should not block the first milestone:

- Complex vector paths.
- Boolean vector operations.
- Advanced gradients.
- Multiple fills beyond the simplest stable case.
- Shadows and blur effects.
- Stroke alignment edge cases.
- Prototype links and transitions.
- Animation.
- Responsive layout beyond simple anchors.
- Full design token synchronization.
- Full component or prefab variant system.

Some of these can move into P1 after the P0 import loop is stable.

## 7. Initial UI IR Shape

The first JSON schema can stay deliberately small:

```json
{
  "version": 1,
  "source": "pencil",
  "documentId": "local-file-or-session-id",
  "nodes": [
    {
      "id": "node-id",
      "name": "Button",
      "type": "frame|group|rectangle|text|image",
      "bounds": { "x": 0, "y": 0, "width": 160, "height": 48 },
      "constraints": { "horizontal": "LEFT", "vertical": "TOP" },
      "clipsContent": false,
      "fills": [
        { "type": "SOLID", "color": { "r": 1, "g": 1, "b": 1, "a": 1 } }
      ],
      "text": {
        "characters": "Start",
        "fontFamily": "Inter",
        "fontSize": 18,
        "alignHorizontal": "CENTER",
        "alignVertical": "CENTER"
      },
      "image": {
        "assetPath": "Assets/PencilImporter/Renders/button.png"
      },
      "children": []
    }
  ]
}
```

This is close enough to `FigmaToUnityImporter`'s node model to reuse the same mapping ideas without copying its Figma API dependency.

## 8. Milestones

### Milestone 0: Research Matrix

Create a feature matrix comparing:

- Figma node fields used by `FigmaToUnityImporter`.
- Pencil node fields exposed by `.pen` or MCP.
- Unity UGUI target component.
- Import strategy: `Generate`, `Render`, `Transform`, or `None`.

Output: `docs/FEATURE_MATRIX.md`.

### Milestone 1: Static IR Importer

Implement a Unity package that imports a hand-written UI IR JSON file into UGUI.

Success criteria:

- Creates a root panel under a selected Canvas.
- Generates nested RectTransforms.
- Generates solid rectangles and TMP text.
- Preserves node ids in GameObject names.
- Re-running import updates existing objects instead of duplicating them.

### Milestone 2: Pencil Export Path

Add a small extractor that reads Pencil node data and writes UI IR JSON.

Success criteria:

- Export selected Pencil panel.
- Preserve hierarchy, ids, bounds, text, fills, and image references.
- Import the exported JSON into Unity without manual editing.

### Milestone 3: Raster Fallback

Add image rendering support for unsupported visual nodes.

Success criteria:

- Unsupported leaf nodes become sprites.
- Unsupported parent nodes can be imported as one rasterized image.
- Children of rasterized parent nodes are skipped.

### Milestone 4: Layout and Polish

Improve constraints, masks, font mapping, and basic gradients only where Pencil and UGUI both support the feature clearly.

Success criteria:

- Anchor mapping behaves consistently.
- Clipped frames map to masks.
- TMP font mapping is configurable.
- Import log reports unsupported features.

## 9. Repository Layout

Planned structure:

```text
pencil-ugui/
  docs/
    PLAN.md
    FEATURE_MATRIX.md
  unity/
    Packages/
      com.jupiterthewarlock.pencil-ugui/
  samples/
    ui-ir/
    pencil/
  tools/
    pencil-exporter/
  FigmaToUnityImporter/
```

`FigmaToUnityImporter/` is a local nested reference repository and should not be treated as source code owned by this project.

## 10. Key Risks

- Pencil's node schema may not expose every field required for faithful layout.
- Raster fallback quality depends on reliable screenshot or render export from Pencil.
- UGUI cannot represent all design effects natively.
- Font matching between design and Unity will require project-specific mapping.
- Incremental updates need stable node ids from Pencil.

The MVP should explicitly accept partial visual fidelity in exchange for a robust import loop.

## 11. Immediate Next Step

Build `docs/FEATURE_MATRIX.md` by reading:

- `FigmaToUnityImporter/Assets/FigmaImporter/Editor/Node.cs`
- `FigmaToUnityImporter/Assets/FigmaImporter/Editor/FigmaNodeGenerator.cs`
- `FigmaToUnityImporter/Assets/FigmaImporter/Editor/TransformUtils.cs`
- Pencil `.pen` samples or MCP output from a small test panel.

After the matrix is complete, implement Milestone 1 before touching Pencil extraction.
