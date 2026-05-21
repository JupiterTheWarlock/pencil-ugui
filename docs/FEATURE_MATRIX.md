# OpenPencil to Unity UGUI Feature Matrix

## Scope

This matrix targets the first `AI canvas -> Unity UGUI` vertical slice using `open-pencil/open-pencil` as the primary harness.

The closed-source Pencil CLI path is intentionally excluded.

PNG export is also excluded from the MVP data path. The importer should consume structured node data and use SVG only as an optional visual check.

## Source Harness

Primary source:

```text
references/open-pencil-complete
```

Current sample artifacts:

```text
samples/harness/open-pencil-settings-panel.fig
samples/harness/open-pencil-settings-panel.svg
```

Useful commands:

```powershell
bun run open-pencil -- info "D:\Users\JtheWL\pencil-unity-ugui\samples\harness\open-pencil-settings-panel.fig" --json
bun run open-pencil -- tree "D:\Users\JtheWL\pencil-unity-ugui\samples\harness\open-pencil-settings-panel.fig" --depth 3 --json
bun run open-pencil -- node "D:\Users\JtheWL\pencil-unity-ugui\samples\harness\open-pencil-settings-panel.fig" --id "0:15" --json
```

## Matrix


| Capability                 | OpenPencil Source                           | UI IR Field             | Unity Target                       | Strategy         | MVP Status | Notes                                                            |
| -------------------------- | ------------------------------------------- | ----------------------- | ---------------------------------- | ---------------- | ---------- | ---------------------------------------------------------------- |
| Document root              | `.fig` page or top-level frame              | `documentId`, root node | Existing `Canvas` child            | `Generate`       | P0         | Import under a selected Canvas instead of creating a Canvas.     |
| Node hierarchy             | `tree --json` children                      | `children`              | Transform hierarchy                | `Generate`       | P0         | This is the core importer path.                                  |
| Stable node id             | `id` from tree/node output                  | `id`                    | GameObject name suffix             | `Generate`       | P0         | Use names like `Confirm Button [0:15]` for incremental updates.  |
| Node name                  | `name`                                      | `name`                  | GameObject name prefix             | `Generate`       | P0         | Preserve readable designer names.                                |
| Node type                  | `type` such as `FRAME`, `RECTANGLE`, `TEXT` | `type`                  | Component choice                   | `Generate`       | P0         | Normalize to `frame`, `rectangle`, `text`, `image`.              |
| Absolute bounds            | `x`, `y`, `width`, `height`                 | `bounds`                | `RectTransform`                    | `Generate`       | P0         | Convert top-left design coordinates to Unity anchored positions. |
| Parent-relative layout     | Tree nesting plus bounds                    | `bounds` per node       | Child `RectTransform`              | `Generate`       | P0         | Keep the first pass simple with fixed anchors.                   |
| Frame                      | `FRAME`                                     | `type: "frame"`         | `GameObject + RectTransform`       | `Generate`       | P0         | Add `Image` only when the frame has a solid fill.                |
| Rectangle                  | `RECTANGLE`                                 | `type: "rectangle"`     | `Image`                            | `Generate`       | P0         | Solid-fill rectangles are native UGUI images.                    |
| Text                       | `TEXT`                                      | `text.characters`       | `TextMeshProUGUI`                  | `Generate`       | P0         | Use TMP with a default font asset initially.                     |
| Text size                  | `fontSize` from node details                | `text.fontSize`         | TMP font size                      | `Generate`       | P0         | If `tree` omits it, query individual nodes.                      |
| Text color                 | Text `fills`                                | `text.color` or `fills` | TMP color                          | `Generate`       | P0         | Support first solid fill only.                                   |
| Solid fill                 | `fills: [{ type: "SOLID" }]`                | `fills`                 | `Image.color` or TMP color         | `Generate`       | P0         | First fill only for MVP.                                         |
| Corner radius              | `cornerRadius`                              | `cornerRadius`          | Deferred or custom sprite/material | `None` initially | P1         | Preserve in IR but do not block native import.                   |
| Clipping                   | Frame clipping fields if exposed            | `clipsContent`          | `RectMask2D`                       | `Generate`       | P1         | Add after basic hierarchy is stable.                             |
| Images                     | Image-like nodes if exposed                 | `image.assetPath`       | `Image + Sprite`                   | `Generate`       | P1         | Not present in the current settings panel sample.                |
| Unsupported leaf visuals   | Node data plus optional export              | `render` metadata       | `Image + Sprite`                   | `Render`         | P2         | Requires a reliable raster path later.                           |
| Unsupported parent visuals | Node subtree                                | `render` metadata       | Single rasterized `Image`          | `Render`         | P2         | Children should be skipped when parent is rendered.              |
| Gradients                  | Gradient fills if exposed                   | `fills`                 | Deferred                           | `None`           | Deferred   | Ignore until both source and Unity mapping are clear.            |
| Shadows and blur           | Effect fields if exposed                    | `effects`               | Deferred                           | `None`           | Deferred   | Not needed for first slice.                                      |
| Vector paths               | Vector/path fields                          | `vector`                | Deferred or SVG/vector package     | `None`           | Deferred   | Keep outside MVP.                                                |
| Components/variants        | Component metadata if exposed               | `component`             | Prefab variants later              | `None`           | Deferred   | Not required for the settings panel.                             |
| Variables/tokens           | `variables` command                         | `variables`             | Import settings or theme assets    | `None`           | Deferred   | Useful later, not part of first importer loop.                   |
| SVG visual check           | `export --format svg`                       | N/A                     | External review artifact           | Verification     | Optional   | Useful for manual comparison.                                    |
| PNG visual check           | `export --format png`                       | N/A                     | External review artifact           | Verification     | Excluded   | Current Windows CanvasKit issue should not block MVP.            |


## Initial UI IR Fields

The first importer should accept this minimal shape:

```json
{
  "version": 1,
  "source": "open-pencil",
  "documentId": "open-pencil-settings-panel.fig",
  "nodes": [
    {
      "id": "0:15",
      "name": "Confirm Button",
      "type": "rectangle",
      "bounds": { "x": 177, "y": 376, "width": 118, "height": 56 },
      "fills": [
        { "type": "SOLID", "color": { "r": 0.125, "g": 0.325, "b": 0.937, "a": 1 } }
      ],
      "children": []
    }
  ]
}
```

## Import Strategy Rules

- `FRAME` maps to `GameObject + RectTransform`.
- `FRAME` with a supported solid fill may also receive an `Image`.
- `RECTANGLE` with a supported solid fill maps to `Image`.
- `TEXT` maps to `TextMeshProUGUI`.
- Unsupported visual fields are preserved in UI IR when cheap, but ignored by the MVP importer.
- Unsupported complex visual nodes should later use `Render`, but the first importer should not depend on raster export.
- Every generated GameObject should include the source node id in its name so re-import can update instead of duplicate.

## Next Implementation Step

Implement a static UI IR importer using a hand-written `samples/ui-ir/settings-panel.json` before adding the OpenPencil exporter.