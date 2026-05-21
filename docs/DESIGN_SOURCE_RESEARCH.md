# Design Source Research

## Context

The project goal is to import AI-designed UI panels into Unity UGUI. The current importer strategy is:

```text
Design canvas
  -> normalized UI IR
  -> Unity Editor importer
  -> UGUI prefab
```

The main source decision is whether the design canvas should be the closed-source Pencil product or one of the open-source OpenPencil alternatives.

The most important selection dimension is not only file readability. The agent-facing harness matters more for this project: tool quality, CLI coverage, skill quality, MCP coverage, feedback loop speed, and whether an AI agent can reliably create and refine a UI panel on the canvas.

Current harness priority:

```text
CLI + skill > MCP
```

Closed-source Pencil is paused for now because its CLI requires external account authorization before local testing.

## Candidates

### Closed-source Pencil

Strengths:

- Already positioned around local `.pen` files and MCP.
- MCP tool surface appears aligned with this project: `batch_get`, `snapshot_layout`, `get_variables`, screenshots, and canvas mutation.
- Likely the fastest path if the file schema and MCP output are stable enough.
- Lower need to maintain a forked design editor.

Weaknesses:

- Source is unavailable, so importer behavior must be inferred from `.pen` samples and MCP responses.
- If the `.pen` schema or MCP API changes, debugging depends on external behavior instead of source-level investigation.
- Harder to patch missing export features directly.

### `open-pencil/open-pencil`

Strengths:

- Strongest fit if Figma compatibility is important.
- Reads and writes `.fig` and `.pen` files.
- Uses a Figma-like scene graph with node types and fields close to Figma:
  - `FRAME`, `GROUP`, `RECTANGLE`, `TEXT`, `IMAGE`, `COMPONENT`, `INSTANCE`.
  - fills, strokes, effects, constraints, auto layout, variables, masks, and text fields.
- Provides CLI and MCP packages.
- Supports export, linting, token extraction, Figma Plugin API style scripting, and file conversion.
- Better source-level context for building an importer that mirrors `FigmaToUnityImporter`.

Weaknesses:

- Current status says active development and not production ready.
- The repository has Windows checkout friction because one path contains `*`, so local source access on Windows needs sparse checkout, WSL, or archive filtering.
- Rich Figma compatibility may be broader than needed for the first Unity UGUI milestone.

### `ZSeven-W/openpencil`

Strengths:

- Easiest file format to consume directly: `.op` files are JSON and Git-friendly.
- Type model is simple and already close to a Unity UI IR:
  - `frame`, `group`, `rectangle`, `text`, `image`, `path`, `ref`.
  - `x`, `y`, `width`, `height`, `fill`, `clipContent`, `layout`, `gap`, `padding`, `textAlign`, `fontSize`.
- MCP docs and source are clear.
- `pen-core` exposes pure document operations and layout helpers.
- `pen-figma` can import `.fig` into its `PenDocument`, which gives a useful bridge for comparison.
- Very good fit for an AI-native design-to-code workflow.

Weaknesses:

- Its core source file extension is `.op`, not the closed Pencil `.pen`.
- It is less Figma-compatible than `open-pencil/open-pencil` by design, focusing more on AI-native design workflows.
- If the long-term goal is compatibility with the real Pencil product, this may become a parallel ecosystem rather than a direct replacement.

## Recommendation

For the current pre-research stage, keep the importer architecture source-agnostic and implement the first Unity side against a normalized UI IR.

Recommended priority:

1. Build the Unity importer against hand-written UI IR first.
2. Test `open-pencil/open-pencil` first among open-source options, because it has stronger public traction and a Figma-compatible scene graph.
3. Test `ZSeven-W/openpencil` second, using its `.op` JSON and clear TypeScript types as the fastest fallback path if the higher-priority harness blocks progress.
4. Resume closed-source Pencil only if local authorization becomes frictionless enough for repeated automated tests.
5. Choose the first candidate that can reliably generate a small UI panel and expose enough data for the P0 importer.

This means `ZSeven-W/openpencil` remains useful for rapid prototyping, but it should not outrank a stronger agent harness.

## Decision Rule

Use closed-source Pencil as the main product path if a small test panel can reliably provide:

- stable node ids,
- hierarchy,
- bounds,
- node types,
- text content and text style,
- solid fills,
- image references or image export,
- clipping/mask flags,
- variables or resolved style values.

If any of those are blocked or opaque, use an open-source adapter first.

Harness quality should be judged with the same test panel across all three tools:

- Can the agent create the panel without manual drawing?
- Can the agent inspect the result as structured nodes?
- Can the agent update one node without rewriting the whole document?
- Can the agent get a visual screenshot or layout snapshot for verification?
- Can the generated design be saved to a local file that the importer can read?
- Are tool errors understandable enough to recover from?

## Practical Path

The preferred product path is:

```text
Closed-source Pencil MCP/.pen
  -> UI IR
  -> Unity UGUI importer
```

The best open-source compatibility path is:

```text
open-pencil/open-pencil scene graph or .pen/.fig tooling
  -> UI IR
  -> Unity UGUI importer
```

The fastest fallback prototype path is:

```text
ZSeven OpenPencil .op JSON
  -> UI IR
  -> Unity UGUI importer
```

Use the fallback only if the higher-priority harnesses do not provide enough reliable context.

## Next Experiment

Create the same simple panel in each source:

- root frame,
- rectangle background,
- title text,
- image placeholder,
- button frame with text,
- clipped child frame.

Then compare the extracted fields against `FigmaToUnityImporter`'s effective feature set:

- transform,
- constraints/layout,
- fills,
- text,
- masks,
- raster fallback.

The result should become `docs/FEATURE_MATRIX.md`.