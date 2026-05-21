import { resolve } from "node:path";
import { fetchJson, loadConfig, resolveProjectPath } from "./config.mjs";

export async function runImport(options) {
  if (!options.uiIrPath) {
    throw new Error("--ui-ir is required for import.");
  }

  const { config } = loadConfig(options.projectRoot);
  const port = config.serverPort ?? 47123;
  const uiIrPath = resolveProjectPath(options.projectRoot, options.uiIrPath);
  const target = options.target || config.defaultTargetMode || "selection";

  const payload = await fetchJson(`http://127.0.0.1:${port}/import`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({
      uiIrPath,
      target,
    }),
  });

  if (payload.ok === false) {
    throw new Error(payload.message ?? payload.error ?? "Import failed.");
  }

  console.log(payload.message ?? "Import completed.");
  if (payload.canvasPath) {
    console.log(`Canvas: ${payload.canvasPath}`);
  }
}
