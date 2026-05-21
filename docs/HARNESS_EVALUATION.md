# Open-Source Harness Evaluation

## Test Brief

Create a simple mobile game settings panel with a title, background card, music toggle, sound toggle, language dropdown, close button, and confirm button.

## Artifacts

- `samples/harness/zseven-settings-panel.ops`
- `samples/harness/zseven-settings-panel.op`
- `samples/harness/zseven-root-update.json`
- `samples/harness/open-pencil-seed.pen`
- `samples/harness/open-pencil-create-settings.js`
- `samples/harness/open-pencil-update-confirm.js`
- `samples/harness/open-pencil-settings-panel.fig`
- `samples/harness/open-pencil-settings-panel.svg`

## `open-pencil/open-pencil`

Local path: `references/open-pencil-complete`

Commands run:

```powershell
bun run open-pencil -- --help
cmd /c "bun run open-pencil -- eval ""D:\Users\JtheWL\pencil-unity-ugui\samples\harness\open-pencil-seed.pen"" --stdin --output ""D:\Users\JtheWL\pencil-unity-ugui\samples\harness\open-pencil-settings-panel.fig"" --json < ""D:\Users\JtheWL\pencil-unity-ugui\samples\harness\open-pencil-create-settings.js"""
bun run open-pencil -- info "D:\Users\JtheWL\pencil-unity-ugui\samples\harness\open-pencil-settings-panel.fig" --json
bun run open-pencil -- tree "D:\Users\JtheWL\pencil-unity-ugui\samples\harness\open-pencil-settings-panel.fig" --depth 3 --json
cmd /c "bun run open-pencil -- eval ""D:\Users\JtheWL\pencil-unity-ugui\samples\harness\open-pencil-settings-panel.fig"" --stdin --write --json < ""D:\Users\JtheWL\pencil-unity-ugui\samples\harness\open-pencil-update-confirm.js"""
bun run open-pencil -- export "D:\Users\JtheWL\pencil-unity-ugui\samples\harness\open-pencil-settings-panel.fig" --format svg --output "D:\Users\JtheWL\pencil-unity-ugui\samples\harness\open-pencil-settings-panel.svg"
```

Result:

- Headless creation works through `eval` against a seed `.pen` and writes a local `.fig`.
- Structured nodes are readable through `info` and `tree`; the generated file reported 1 page, 12 nodes, and the expected `FRAME`, `TEXT`, and `RECTANGLE` mix.
- Single-node update works through `eval` and `figma.getNodeById`; `Confirm Button` was updated in place.
- SVG export works from the local `.fig`.
- PNG export failed on Windows because CanvasKit tried to read `\D:\...\canvaskit.wasm`, even though the wasm file exists at the non-prefixed path.
- MCP remains less suitable for this immediate local harness unless the OpenPencil app bridge is running.

Assessment:

- Best current fit for Figma-like hierarchy and local structured inspection.
- Good candidate for the feature matrix and static importer research.
- Needs a Windows PNG export fix or SVG-based visual checks for now.

## `ZSeven-W/openpencil`

Local path: `references/zseven-openpencil`

Commands run:

```powershell
node apps/cli/dist/openpencil-cli.cjs --help
node apps/cli/dist/openpencil-cli.cjs open --file "D:\Users\JtheWL\pencil-unity-ugui\samples\harness\zseven-settings-panel.op" --pretty
node apps/cli/dist/openpencil-cli.cjs design "@D:\Users\JtheWL\pencil-unity-ugui\samples\harness\zseven-settings-panel.ops" --file "D:\Users\JtheWL\pencil-unity-ugui\samples\harness\zseven-settings-panel.op" --canvas-width 375 --pretty
node apps/cli/dist/openpencil-cli.cjs get --file "D:\Users\JtheWL\pencil-unity-ugui\samples\harness\zseven-settings-panel.op" --depth 3 --pretty
node apps/cli/dist/openpencil-cli.cjs layout --file "D:\Users\JtheWL\pencil-unity-ugui\samples\harness\zseven-settings-panel.op" --depth 3 --pretty
node apps/cli/dist/openpencil-cli.cjs update T1DgqEQOSaxFsctquh_0I "@D:\Users\JtheWL\pencil-unity-ugui\samples\harness\zseven-root-update.json" --file "D:\Users\JtheWL\pencil-unity-ugui\samples\harness\zseven-settings-panel.op" --pretty
```

Result:

- CLI help works.
- `open` creates a new local `.op` file.
- `design` works after keeping the DSL operation on one line. The first multiline attempt failed with a clear parse error, which was recoverable.
- `get` exposes a structured node tree, and `layout` exposes hierarchical bounds.
- Root node update works by ID.
- Nested nodes created inside one `batch_design` operation did not receive stable IDs in the saved `.op`; this blocks reliable single-child updates unless the agent supplies IDs or nodes are inserted separately.
- `read-nodes --file` still tries to reach a running OpenPencil instance, so it is not a pure file-mode read path.
- No offline CLI image export was verified; screenshot support appears tied to live/debug tooling.

Assessment:

- Fastest local file-format path because `.op` is plain JSON.
- Good fallback for a source-agnostic UI IR extractor.
- Needs stricter ID generation for nested nodes and an offline visual export path before it can be the primary harness.

## Current Ranking

1. `open-pencil/open-pencil`: stronger Figma-like source model and working local `.fig`/SVG flow.
2. `ZSeven-W/openpencil`: simpler `.op` JSON and good file-mode creation/read basics, but nested IDs and export are current blockers.

## Next Steps

- Build `docs/FEATURE_MATRIX.md` using `open-pencil/open-pencil` first.
- For ZSeven, rerun the same panel with explicit IDs on every node to confirm whether stable child updates become reliable.
- Investigate the `open-pencil/open-pencil` CanvasKit path handling on Windows if PNG export is required.
