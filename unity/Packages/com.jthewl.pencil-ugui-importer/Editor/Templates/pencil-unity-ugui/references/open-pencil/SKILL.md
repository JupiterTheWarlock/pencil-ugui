# Open Pencil Bridge Reference

This reference adds Unity UGUI bridge constraints for `open-pencil/open-pencil`.

## Source Artifacts

- Design file extension: `.fig`
- Primary sample: `samples/harness/open-pencil-settings-panel.fig`

## Useful CLI Patterns

From the configured `openPencilDir`:

```powershell
bun run open-pencil -- info "<fig>" --json
bun run open-pencil -- tree "<fig>" --depth 3 --json
bun run open-pencil -- node "<fig>" --id "<node-id>" --json
```

## Export To UI IR

From the Unity project root:

```powershell
openpencil-ugui export --input "<fig>" --output ".open-pencil-ugui/generated/panel.json"
```

## Unity Mapping Notes

- `FRAME` -> `GameObject + RectTransform`, add `Image` when there is a solid fill
- `RECTANGLE` -> `Image`
- `TEXT` -> `TextMeshProUGUI`
- Node names containing `Button` may receive a `Button` component
- Corner radius is preserved in UI IR but not rendered natively in MVP

## Stable Node Identity

Keep node ids stable across updates. The Unity importer uses node id suffixes in GameObject names to update existing objects.

## Known MVP Limits

- PNG export is not required for import
- Gradients, shadows, blur, and complex vectors are deferred
- Import target mode MVP supports `selection` on a Canvas only
