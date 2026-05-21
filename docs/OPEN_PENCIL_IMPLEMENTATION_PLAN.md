# OpenPencil to Unity UGUI Implementation Plan

## 1. Decision

Use `open-pencil/open-pencil` as the primary harness for the first working `AI canvas -> Unity UGUI` pipeline.

The closed-source Pencil CLI path is abandoned and should not be included in future harness work, feature comparisons, or implementation planning.

`ZSeven-W/openpencil` remains useful as a secondary reference for plain JSON document ideas, but it is not the primary path for the MVP because its current file-mode workflow has weaker guarantees around repeated runs and nested node identity.

## 2. Goal

Build the smallest reliable pipeline that can:

1. Generate or update a UI design through an AI canvas harness.
2. Read a structured node tree from the generated design.
3. Convert that tree into a stable UI IR JSON document.
4. Import the UI IR into Unity as UGUI objects under an existing `Canvas`.
5. Re-run the import and update existing GameObjects by stable node id instead of duplicating them.

Visual export is useful for verification, but it is not part of the core data path. SVG export is sufficient for early visual checks. PNG export should not block the MVP.

## 3. Current Harness Assessment

`open-pencil/open-pencil` currently supports the important MVP requirements:

- Headless document creation through `eval` against a seed `.pen`.
- Local `.fig` output that can be saved with the test artifacts.
- Structured inspection through `info` and `tree`.
- Stable node ids for generated nodes in the local `.fig`.
- In-place node update through `figma.getNodeById`.
- SVG export for visual inspection.

Known limitation:

- PNG export fails on Windows because CanvasKit resolves the wasm path with an invalid leading slash. This is not a blocker for the importer because the importer should consume structured node data, not screenshots.

## 4. MVP Scope

The first implementation should support only the design features that are already visible in the settings panel harness and map cleanly to UGUI:

- Frame hierarchy.
- Stable node id.
- Node name.
- Absolute bounds.
- Parent-child layout conversion.
- Rectangles with solid fills.
- Text nodes with content, font size, and fill color.
- Basic rounded rectangle metadata, even if Unity handling is initially deferred.
- Incremental update by node id.

Deferred:

- PNG rendering.
- Complex vector paths.
- Gradients.
- Shadows and blur.
- Components and variants.
- Full responsive layout.
- Font asset matching beyond a simple default TMP font.

## 5. Target Pipeline

```text
open-pencil/open-pencil harness
  -> local .fig file
  -> OpenPencil tree/info/node inspection
  -> UI IR JSON
  -> Unity Editor importer
  -> UGUI prefab or scene hierarchy under Canvas
```

The UI IR should be the stable contract between OpenPencil and Unity. Unity code should not depend directly on the OpenPencil `.fig` file format.

## 6. Implementation Milestones

### Milestone 0: Harness Lock-In

Output:

- Keep `docs/HARNESS_EVALUATION.md` as the historical harness result.
- Treat `open-pencil/open-pencil` as the primary source in new planning docs.
- Do not add new closed-source Pencil tasks.

Success criteria:

- The settings panel harness can be regenerated locally.
- `info`, `tree`, and single-node update continue to pass.
- SVG export remains available for manual verification.

### Milestone 1: Feature Matrix

Create `docs/FEATURE_MATRIX.md`.

Compare:

- OpenPencil node fields from the generated settings panel.
- UI IR fields required by the importer.
- Unity UGUI target components.
- Import strategy: `Generate`, `Transform`, `Render`, or `None`.

Success criteria:

- Each MVP field has a clear source and Unity target.
- Unsupported fields are explicitly marked as deferred or ignored.
- PNG export is listed as optional verification, not a required importer feature.

### Milestone 2: Static UI IR Importer

Create a Unity package under:

```text
packages/com.jupiterthewarlock.pencil-ugui/
```

Start with a hand-written sample UI IR file before adding OpenPencil extraction.

Success criteria:

- Import creates a root panel under a selected `Canvas`.
- Frames become `GameObject + RectTransform`.
- Rectangles become `Image`.
- Text nodes become `TextMeshProUGUI`.
- GameObject names preserve node ids, for example `Confirm Button [0:15]`.
- Re-running import updates existing objects by node id.

### Milestone 3: OpenPencil to UI IR Exporter

Add a small exporter under:

```text
tools/open-pencil-exporter/
```

The exporter should read the `.fig` produced by the harness and write UI IR JSON.

Initial implementation can call the existing OpenPencil CLI commands and parse their JSON output. Direct `.fig` parsing can wait until the CLI path proves insufficient.

Success criteria:

- Export preserves hierarchy, ids, names, types, bounds, fills, and text.
- Exported UI IR imports into Unity without hand editing.
- The settings panel becomes the first full end-to-end sample.

### Milestone 4: End-to-End Harness Test

Add a repeatable local test flow:

```text
generate OpenPencil .fig
  -> export UI IR
  -> import UI IR in Unity
  -> verify generated hierarchy
```

Success criteria:

- The test can run from clean artifacts.
- The generated UI IR is deterministic enough for review.
- The Unity hierarchy contains expected objects for title, toggles, dropdown, close button, and confirm button.
- A second import updates the same objects rather than duplicating them.

## 7. First Vertical Slice

Use the existing settings panel brief:

```text
Create a simple mobile game settings panel with a title, background card, music toggle, sound toggle, language dropdown, close button, and confirm button.
```

Expected first end-to-end artifacts:

- `samples/harness/open-pencil-settings-panel.fig`
- `samples/harness/open-pencil-settings-panel.svg`
- `samples/ui-ir/settings-panel.json`
- Unity scene or prefab generated from `settings-panel.json`

The first slice is complete when the generated Unity hierarchy is recognizable and re-importable, even if visual fidelity is not perfect.

## 8. Risks and Mitigations

Risk: OpenPencil CLI tree output may not expose every style field needed by the importer.

Mitigation: Add targeted `node` queries for specific ids before parsing `.fig` directly.

Risk: Text bounds in the harness may be approximate.

Mitigation: Import text with explicit RectTransform bounds first, then tune TMP sizing after the basic hierarchy is stable.

Risk: Rounded corners do not map directly to default UGUI `Image`.

Mitigation: Preserve corner radius in UI IR, but initially import as a normal solid `Image`.

Risk: Visual export remains SVG-only on Windows.

Mitigation: Use SVG for manual review. Keep PNG support out of the MVP unless visual diff automation becomes necessary.

## 9. Immediate Next Step

Create `docs/FEATURE_MATRIX.md` from the current OpenPencil settings panel output, then implement the static UI IR importer before adding the OpenPencil exporter.