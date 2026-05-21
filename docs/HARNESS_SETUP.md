# Harness Setup Status

## Closed-source Pencil

Installed:

```powershell
npm install -g "@pencil.dev/cli"
```

Verified:

```powershell
pencil version
pencil status
pencil --help
```

Current status:

- CLI version: `0.2.6`.
- CLI command is available.
- Authentication is not configured yet.
- `pencil status` reports `Not authenticated`.

Next requirement:

- Run `pencil login`, or provide `PENCIL_CLI_KEY` in the shell environment.
- After authentication, the first smoke test command should generate a `.pen` and exported image.

Candidate smoke test:

```powershell
pencil --out samples\pencil\smoke.pen --prompt "Create a simple mobile game settings panel" --export samples\pencil\smoke.png --export-scale 2
```

## `open-pencil/open-pencil`

Local path:

```text
references/open-pencil-complete
```

Notes:

- Full checkout on Windows is blocked by an invalid path in the repository: `.github/instructions/*.instructions.md`.
- Valid source directories were restored manually from Git.
- `bun install` mostly completed, but reported several package link failures.
- The CLI still runs and prints help.
- The MCP package builds successfully when run directly from `packages/mcp`.

Verified:

```powershell
bun run open-pencil -- --help
cd packages\mcp
bun run build
```

Current HTTP MCP server:

```text
http://127.0.0.1:7600/mcp
```

Startup command:

```powershell
$env:PORT='7600'; $env:WS_PORT='7601'; bun packages/mcp/src/index.ts
```

Endpoint probe:

- `GET /mcp` returns `406`, which is expected for a streamable MCP endpoint without a proper MCP request.

Important harness note:

- This MCP server is an OpenPencil app bridge. Some tools may require a running OpenPencil app or document connection over its WebSocket bridge.

## `ZSeven-W/openpencil`

Local path:

```text
references/zseven-openpencil
```

Setup:

```powershell
git submodule update --init --recursive
bun install
bun run mcp:compile
bun run cli:compile
```

Notes:

- The repository requires the `packages/agent-native` submodule.
- Initial install failed until the submodule was initialized.
- Bun cache had one `paper` integrity failure; clearing Bun cache fixed it.
- Optional Zig native build is skipped because Zig is not installed. Basic MCP/CLI still compiled.

Verified:

```powershell
node apps/cli/dist/openpencil-cli.cjs --help
bun run mcp:compile
bun run cli:compile
```

Current HTTP MCP server:

```text
http://127.0.0.1:3100/mcp
```

Startup command:

```powershell
bun run mcp:dev -- --http --port 3100
```

Endpoint probe:

- `GET /mcp` returns `400`, which is expected for a streamable MCP endpoint without a proper MCP request.

## Next Smoke Test

Use the same target brief for all three harnesses:

```text
Create a simple mobile game settings panel with a title, background card, music toggle, sound toggle, language dropdown, close button, and confirm button.
```

For each harness, capture:

- Can the agent create the design without manual drawing?
- Can the harness expose structured nodes?
- Can one node be updated without regenerating the whole design?
- Can it export an image for visual verification?
- Can it save a local design file that our importer can read?
- Are tool failures recoverable and understandable?
