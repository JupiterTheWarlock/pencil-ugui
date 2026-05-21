# Agent Instructions

## Project Goal

This repository builds a bridge from AI canvas tools to Unity UGUI.

The core contract is UI IR JSON:

```text
canvas provider -> source exporter -> UI IR JSON -> Unity UGUI importer
```

Unity importer code must depend on UI IR, not directly on `open-pencil`, Pencil, MCP payloads, or canvas-specific file formats.

## Current MVP

The current primary provider is `open-pencil/open-pencil`.

Focus on the smallest reliable path:

1. Create or update an OpenPencil `.fig`.
2. Export it to UI IR JSON.
3. Import the UI IR into an existing Unity UGUI `Canvas`.
4. Re-import by stable node id without duplicating GameObjects.

Closed-source Pencil, `ZSeven-W/openpencil`, Unity MCP target discovery, and custom component handlers are future or optional provider/enhancement paths unless the task explicitly targets them.

## Architecture Rules

- Treat UI IR as the stable boundary between source canvas tools and Unity.
- Keep provider-specific behavior in provider/exporter code.
- Keep Unity importer behavior source-agnostic.
- Prefer project-local configuration under `.pencil-ugui/` for harness state.
- Prefer a Unity local server or CLI bridge for automation; Unity MCP should be an optional enhancement, not a required dependency.
- Skill installation should create a main `pencil-ugui` skill, with canvas provider references under `references/<provider>/`.
- Prefer official provider skills and documentation when available. Project reference skills should only add Unity bridge constraints, examples, and known local quirks.

## Coding Guidelines

- Make the smallest runnable change that satisfies the current task.
- Do not wrap logic into a method if it is only called once and extraction does not improve clarity.
- Avoid unnecessary defensive programming. Validate required contracts, then keep code clean.
- Follow SOLID, DRY, and SRP where they help the code stay simple.
- Preserve existing user changes. Never revert unrelated work unless explicitly asked.
- Prefer existing project patterns over new abstractions.

## Unity Package

The Unity package lives at:

```text
packages/com.jupiterthewarlock.pencil-ugui/
```

Important current files:

- `Editor/UiIrModels.cs`
- `Editor/PencilUgImporter.cs`
- `Editor/PencilUgImportMenu.cs`

The importer should create or update UGUI objects under an existing target `Canvas` or `Transform`. Generated objects should preserve source node ids so repeated imports update rather than duplicate.

## Tools And Samples

The OpenPencil exporter lives at:

```text
tools/open-pencil-exporter/
```

Useful sample paths:

```text
samples/harness/
samples/ui-ir/
```

Planning docs:

```text
docs/OPEN_PENCIL_IMPLEMENTATION_PLAN.md
docs/FEATURE_MATRIX.md
docs/HARNESS_ARCHITECTURE.md
```

Update these docs when changing the harness architecture or MVP scope.

## Harness Direction

The target user experience is:

1. Install the Unity package from a Git URL.
2. Open the package setup EditorWindow.
3. Configure local service port, provider paths, and agent skill installation.
4. Start an agent from the Unity project root.
5. The agent reads the installed skill, uses the provider reference, generates UI, exports UI IR, and imports it into UGUI.

Optional enhancements:

- Unity MCP tools for selection and target discovery.
- Local server endpoints for `/health`, `/targets`, `/import`, `/handlers`, and `/prompt-context`.
- Component handler interfaces that expose both import behavior and prompt-time constraints.

Keep the MVP centered on the first end-to-end `open-pencil/open-pencil -> UI IR -> UGUI` loop.
