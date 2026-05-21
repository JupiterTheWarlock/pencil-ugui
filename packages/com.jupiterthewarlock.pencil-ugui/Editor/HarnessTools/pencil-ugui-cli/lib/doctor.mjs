import { existsSync } from "node:fs";
import {
  ensureGeneratedDir,
  fetchJson,
  getSkillFilePath,
  isSkillInstalled,
  loadConfig,
  parseExporterCommand,
  printCheck,
  resolveProjectPath,
} from "./config.mjs";

export async function runDoctor(options) {
  let failed = false;
  let config;
  let configPath;

  try {
    ({ config, configPath } = loadConfig(options.projectRoot));
    printCheck("config", true, ".pencil-ugui/config.json");
  } catch (error) {
    printCheck("config", false, error.message);
    process.exitCode = 1;
    return;
  }

  const provider = options.provider || config.provider || "open-pencil";
  const providerOk = provider === "open-pencil";
  printCheck("provider", providerOk, provider);
  failed ||= !providerOk;

  const openPencilDir = resolveProjectPath(options.projectRoot, config.openPencilDir);
  const openPencilOk = existsSync(openPencilDir);
  printCheck("open-pencil dir", openPencilOk, config.openPencilDir);
  failed ||= !openPencilOk;

  const exporter = parseExporterCommand(
    options.projectRoot,
    config.exporterCommand ?? "node .pencil-ugui/tools/open-pencil-exporter/export.mjs",
  );
  const exporterOk = existsSync(exporter.scriptPath);
  printCheck("exporter script", exporterOk, config.exporterCommand);
  failed ||= !exporterOk;

  try {
    ensureGeneratedDir(options.projectRoot, config);
    printCheck("generated dir", true, config.generatedDir ?? ".pencil-ugui/generated");
  } catch (error) {
    printCheck("generated dir", false, error.message);
    failed = true;
  }

  for (const target of config.skillTargets ?? ["cursor"]) {
    const skillOk = isSkillInstalled(options.projectRoot, target, config);
    printCheck(`${target} skill`, skillOk, getSkillFilePath(options.projectRoot, target, config));
    failed ||= !skillOk;
  }

  const port = config.serverPort ?? 47123;
  try {
    const health = await fetchJson(`http://127.0.0.1:${port}/health`);
    printCheck("unity local server", health.ok === true, `http://127.0.0.1:${port}/health`);
  } catch (error) {
    printCheck("unity local server", false, error.message);
    failed = true;
  }

  if (failed) {
    process.exitCode = 1;
    console.log("\nDoctor found issues. Open Tools > Pencil UGUI > Setup... in Unity to fix configuration.");
    return;
  }

  console.log("\nDoctor passed.");
}
