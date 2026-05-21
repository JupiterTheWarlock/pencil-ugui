# Pencil Unity UGUI Harness Architecture

## Purpose

This document captures the target harness for making `pencil-ugui` work as a bridge between AI canvas tools and Unity UGUI.

The desired user experience is:

1. Install the Unity package from a Git URL.
2. Open a Unity Editor window from the package.
3. Configure local ports, tool paths, and agent skill installation targets.
4. Start an agent from the Unity project root.
5. Ask the agent to create or update UI.
6. The agent uses the installed skill, calls the configured canvas CLI, exports UI IR, and imports the result into the selected Unity UGUI target.

The first supported canvas provider is `open-pencil/open-pencil`. Other canvas tools can be added later by writing source-specific exporters that produce the same UI IR.

## Core Contract: UI IR

UI IR is the stable contract of the whole project.

```text
canvas source
  -> source adapter / exporter
  -> UI IR JSON
  -> Unity UGUI importer
```

The Unity importer should not depend on `open-pencil` internals, `.fig` details, closed-source Pencil APIs, or any specific canvas MCP shape. It should only consume UI IR.

This means the following sources are all theoretically valid:

- `open-pencil/open-pencil`
- closed-source Pencil
- `ZSeven-W/openpencil`
- Figma-like local sources
- hand-written UI IR
- future AI canvas tools

Each source needs its own provider adapter. The MVP only implements the `open-pencil/open-pencil` provider.

## Required Harness

The required harness should not depend on Unity MCP. Unity MCP is useful, but the core path must work without it.

Required flow:

```text
Unity plugin setup
  -> install project agent skill
  -> agent reads skill from project root
  -> agent calls open-pencil CLI to create or update .fig
  -> agent calls exporter to write UI IR JSON
  -> agent calls Unity local server or CLI bridge to import JSON
  -> Unity imports under a target UGUI Canvas or Transform
```

The harness should have these pieces:

- Unity Editor setup window.
- Project-local configuration file.
- Agent skill installer.
- Canvas provider references.
- OpenPencil exporter.
- Unity import endpoint.
- Doctor command for setup validation.

The plugin should write stable project-local files so the agent does not need to guess where the package was installed.

Recommended project files:

```text
.pencil-ugui/
  config.json
  generated/
  prompts/
  logs/
```

Recommended agent files:

```text
.cursor/skills/pencil-ugui/
.codex/skills/pencil-ugui/
.claude/
.qoder/
```

Exact agent installation formats can differ by agent platform, but the installed instructions should point back to `.pencil-ugui/config.json` as the source of project-specific runtime configuration.

## Skill Model

The main project skill should be `pencil-ugui`.

This skill is the orchestrator. It should explain how to:

1. Read `.pencil-ugui/config.json`.
2. Validate the harness with `doctor`.
3. Choose a canvas provider.
4. Use that provider's reference skill or documentation.
5. Generate or update the source canvas file.
6. Export UI IR.
7. Import UI IR into Unity.
8. Read project handler prompts before generating component-aware UI.

Canvas-specific knowledge should live under references:

```text
pencil-ugui/
  SKILL.md
  references/
    open-pencil/
      SKILL.md
      docs.md
      examples/
    openpencil/
      SKILL.md
      docs.md
      examples/
    pencil/
      SKILL.md
      docs.md
      examples/
```

The main skill should prefer official provider skills and documentation when available. Project references should only add bridge-specific guidance:

- Unity UGUI layout constraints.
- Stable naming and node id conventions.
- Exporter requirements.
- Known local CLI quirks.
- Examples that are specific to this bridge.

## Provider Adapter Model

Each canvas source should be represented as a provider.

Example provider fields:

```json
{
  "id": "open-pencil",
  "displayName": "Open Pencil",
  "designFileExtension": ".fig",
  "referenceSkill": "references/open-pencil/SKILL.md",
  "doctorCommand": "pencil-ugui doctor --provider open-pencil",
  "exportCommand": "pencil-ugui export --provider open-pencil --input <fig> --output <json>"
}
```

The provider owns source-specific behavior. The importer owns only UI IR behavior.

Provider responsibilities:

- Validate required tools.
- Tell the agent how to create and update a design file.
- Export source data to UI IR.
- Preserve stable node ids when possible.
- Report source limitations clearly.

Unity importer responsibilities:

- Resolve the target Canvas or Transform.
- Create or update GameObjects by UI IR node id.
- Apply supported RectTransform, Image, TextMeshPro, and component data.
- Preserve unsupported data when useful, but avoid depending on source-specific fields.

## Unity Local Server

The Unity plugin should provide a local Editor service. The service is the default automation bridge between the agent and the Unity Editor.

Suggested endpoints:

```text
GET  /health
GET  /config
GET  /targets/selection
GET  /targets/canvases
GET  /handlers
GET  /prompt-context
POST /import
```

`POST /import` should accept a UI IR path and a target descriptor.

Target descriptors should support:

```text
selection
scene:/Canvas/Panel
prefab:Assets/UI/MainMenu.prefab:/Canvas/Root
asset:Assets/UI/MainMenu.prefab
```

The first implementation can support only `selection` and selected GameObjects whose transform is a `RectTransform`. Prefab and explicit path targets can be added after the core loop is stable.

## Optional Unity MCP Enhancement

Unity MCP is optional harness enhancement, not a required dependency.

Its main value is target discovery:

- Read the current selected scene object.
- Find the nearest or child `Canvas`.
- Locate a target under a prefab.
- Return a stable target descriptor to the CLI or local server.

MCP custom tools should wrap the same Unity local server operations where possible. This keeps the system usable when MCP is unavailable.

Recommended MCP tool shape:

```text
pencil_ugui_get_selection_target
pencil_ugui_list_canvases
pencil_ugui_import_ir
pencil_ugui_get_prompt_context
```

These tools should call the plugin service rather than duplicating importer logic.

## Component Handlers

Custom component support should be implemented through handler interfaces in the Unity plugin.

Handlers allow project teams to describe how generated UI should attach common custom components, configure properties, and preserve references.

Conceptual interface:

```csharp
public interface IPencilUgComponentHandler
{
    string ComponentKey { get; }
    string PromptMarkdown { get; }
    bool CanHandle(UiIrNode node);
    void Apply(GameObject gameObject, UiIrNode node, ImportContext context);
}
```

The handler registry should expose two things:

- Import-time behavior for UI IR nodes.
- Prompt-time constraints for agents.

The prompt-time side is important. Agents must know which custom components exist before generating the UI IR. The local server or CLI should expose this with:

```text
pencil-ugui prompt-context
pencil-ugui list-handlers
```

This allows project-specific component information to be injected into the agent's prompt without hardcoding it into the base skill.

## UI IR Component Metadata

UI IR should include optional component metadata when a generated node should receive a custom or built-in Unity component.

Example:

```json
{
  "id": "0:15",
  "name": "Confirm Button",
  "type": "rectangle",
  "component": {
    "key": "UnityEngine.UI.Button",
    "properties": {
      "interactable": true
    }
  }
}
```

The importer should keep the first version small:

- Native `Button` detection can continue to work from naming while the component field is being introduced.
- Custom component application should be explicit through `component.key`.
- Complex reference wiring should wait until handler requirements are proven by real project usage.

## Editor Window

The package should provide a setup window.

Suggested sections:

- Harness status.
- OpenPencil CLI path.
- Exporter command path.
- Unity local server port.
- Agent skill installation targets.
- Default provider.
- Default import target mode.
- Handler prompt preview.
- Install or update skill button.
- Run doctor button.

The setup window should write project-local configuration and instructions. It should not require users to manually copy files from the package.

## CLI Commands

The CLI should be thin and predictable.

Suggested commands:

```text
pencil-ugui doctor
pencil-ugui generate
pencil-ugui export
pencil-ugui import
pencil-ugui prompt-context
pencil-ugui list-handlers
```

Early versions can keep `generate` provider-specific and focus on `export` and `import`.

`doctor` should validate:

- Unity local server is reachable.
- OpenPencil CLI is reachable.
- Config file exists.
- Agent skill is installed.
- Output directories exist.
- Current provider is supported.

## Implementation Order

Recommended order:

1. Keep UI IR as the only Unity importer contract.
2. Add Unity local server with health and import endpoints.
3. Add project config under `.pencil-ugui/config.json`.
4. Add setup Editor window and skill installer.
5. Create `pencil-ugui` skill with `open-pencil` reference.
6. Add CLI `doctor`, `export`, `import`, and `prompt-context`.
7. Run end-to-end from an agent opened at a Unity project root.
8. Add optional Unity MCP tools for target discovery.
9. Add component handler registry and prompt injection.
10. Add additional canvas providers only after `open-pencil/open-pencil` is reliable.

## Non-Goals For MVP

The MVP should not attempt to solve every design-to-Unity problem.

Deferred:

- Full visual fidelity.
- PNG export automation.
- Complex vector and effect rendering.
- Complete responsive layout.
- Automatic prefab variant systems.
- Full custom component reference wiring.
- Multiple canvas providers at once.

The MVP is successful when an agent can create a recognizable UI panel through `open-pencil/open-pencil`, export UI IR, and import it into UGUI repeatedly without duplicating objects.

## Current Implementation Status

Implemented in this repository:

- Unity UI IR importer under `packages/com.jupiterthewarlock.pencil-ugui/`
- Project-local harness config at `.pencil-ugui/config.json`
- Unity setup window at `Tools > Pencil UGUI > Setup...`
- Unity Editor local server with `/health`, `/config`, `/targets/selection`, and `/import`
- In-Unity doctor (no manual CLI install required from the setup window)
- Automatic bootstrap of harness tools into `.pencil-ugui/tools/`
- Cursor, Codex, Claude, Qoder, and custom-folder skill installation
- Thin CLI at `tools/pencil-ugui-cli/` with `doctor`, `export`, `import`, and `prompt-context`

Still deferred:

- Prefab and explicit scene path import targets
- Unity MCP target discovery tools
- Component handler registry and `/handlers`
- Additional canvas providers beyond `open-pencil`

## Quick Start

1. Install the Unity package from Git into a Unity project.
2. Open `Tools > Pencil UGUI > Setup...`.
3. Set `openPencilDir` to your local `open-pencil/open-pencil` checkout.
4. Click `Save Config`, then `Start Server`.
5. Select agent skill targets and click `Install / Update Skill`.
6. Click `Run Doctor` in the setup window. The window bootstraps harness tools into `.pencil-ugui/tools/` automatically.
7. Export a `.fig` file to UI IR:

```powershell
node .pencil-ugui/tools/pencil-ugui-cli/bin/pencil-ugui.mjs export `
  --input path/to/design.fig `
  --output .pencil-ugui/generated/panel.json
```

8. Select a `Canvas` in the Unity Hierarchy, then import:

```powershell
node .pencil-ugui/tools/pencil-ugui-cli/bin/pencil-ugui.mjs import `
  --ui-ir .pencil-ugui/generated/panel.json `
  --target selection
```

Manual fallback remains available through `Tools > Pencil UGUI > Import UI IR...`.
