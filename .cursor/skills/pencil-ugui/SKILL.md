# Pencil UGUI

Use this skill to bridge AI canvas design tools to Unity UGUI through UI IR JSON.

## Runtime Configuration

Always read project configuration from:

```text
.pencil-ugui/config.json
```

Do not guess package install paths. The Unity plugin writes stable project-local paths there.

## Workflow

1. Read `.pencil-ugui/config.json`.
2. Run `pencil-ugui doctor` from the Unity project root.
3. Use the configured canvas provider reference under `references/<provider>/`.
4. Create or update the provider design file.
5. Export UI IR JSON with `pencil-ugui export`.
6. Import UI IR into Unity with `pencil-ugui import` while a GameObject with a RectTransform is selected in the Hierarchy.

## Unity Import Rules

- UI IR is the only contract consumed by the Unity importer.
- Import under the selected RectTransform GameObject; do not create a new Canvas unless the user asks.
- Preserve stable node ids so repeated imports update instead of duplicate.
- GameObject names should keep readable names plus node id suffix, for example `Confirm Button [0:15]`.

## Provider Reference

For the default OpenPencil provider, read:

```text
references/open-pencil/SKILL.md
```

Provider references should only add bridge-specific guidance. Prefer official provider documentation when available.

## Prompt Context

Before generating component-aware UI, run:

```text
pencil-ugui prompt-context
```

Use the returned provider, target mode, and handler notes in the generation prompt.
