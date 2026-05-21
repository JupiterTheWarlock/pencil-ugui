import { spawnSync } from "node:child_process";
import { resolve } from "node:path";
import { loadConfig, parseExporterCommand, resolveProjectPath } from "./config.mjs";

export async function runExport(options) {
  if (!options.input || !options.output) {
    throw new Error("--input and --output are required for export.");
  }

  const { config } = loadConfig(options.projectRoot);
  const provider = options.provider || config.provider || "open-pencil";
  if (provider !== "open-pencil") {
    throw new Error(`Unsupported provider: ${provider}`);
  }

  const exporter = parseExporterCommand(options.projectRoot, config.exporterCommand ?? "node tools/open-pencil-exporter/export.mjs");
  const inputPath = resolveProjectPath(options.projectRoot, options.input);
  const outputPath = resolveProjectPath(options.projectRoot, options.output);
  const openPencilDir = resolveProjectPath(options.projectRoot, config.openPencilDir);

  const args = [
    ...exporter.argsPrefix,
    "--input",
    inputPath,
    "--output",
    outputPath,
    "--open-pencil-dir",
    openPencilDir,
  ];

  const result = spawnSync(exporter.command, args, {
    cwd: options.projectRoot,
    encoding: "utf8",
    shell: process.platform === "win32",
  });

  if (result.status !== 0) {
    throw new Error(result.stderr || result.stdout || "Export failed.");
  }

  if (result.stdout?.trim()) {
    console.log(result.stdout.trim());
  }

  console.log(`Exported UI IR to ${resolve(outputPath)}`);
}
