import { existsSync, mkdirSync, readFileSync } from "node:fs";
import { dirname, isAbsolute, join, resolve } from "node:path";
import { fileURLToPath } from "node:url";

const CONFIG_DIR = ".pencil-ugui";
const CONFIG_FILE = "config.json";

export function loadConfig(projectRoot) {
  const configPath = join(projectRoot, CONFIG_DIR, CONFIG_FILE);
  if (!existsSync(configPath)) {
    throw new Error(`Config not found: ${configPath}`);
  }

  const config = JSON.parse(readFileSync(configPath, "utf8"));
  return { config, configPath };
}

export function resolveProjectPath(projectRoot, pathValue) {
  if (!pathValue) {
    return projectRoot;
  }

  return isAbsolute(pathValue) ? pathValue : resolve(projectRoot, pathValue);
}

export function ensureGeneratedDir(projectRoot, config) {
  const generatedDir = resolveProjectPath(projectRoot, config.generatedDir ?? ".pencil-ugui/generated");
  mkdirSync(generatedDir, { recursive: true });
  return generatedDir;
}

export function parseExporterCommand(projectRoot, exporterCommand) {
  const trimmed = exporterCommand.trim();
  const parts = trimmed.split(/\s+/);
  const command = parts[0];
  const argsPrefix = parts.slice(1);
  return {
    command,
    argsPrefix,
    scriptPath: argsPrefix[0] ? resolveProjectPath(projectRoot, argsPrefix[0]) : "",
  };
}

export function isCursorSkillInstalled(projectRoot) {
  return isSkillInstalled(projectRoot, "cursor");
}

export function isCodexSkillInstalled(projectRoot) {
  return isSkillInstalled(projectRoot, "codex");
}

export function isSkillInstalled(projectRoot, target, config) {
  const skillPath = getSkillFilePath(projectRoot, target, config);
  return skillPath ? existsSync(skillPath) : false;
}

export function getSkillFilePath(projectRoot, target, config) {
  const root = getSkillDestinationRoot(projectRoot, target, config);
  return root ? join(root, "pencil-ugui", "SKILL.md") : "";
}

export function getSkillDestinationRoot(projectRoot, target, config) {
  switch (target?.trim().toLowerCase()) {
    case "cursor":
      return join(projectRoot, ".cursor", "skills");
    case "codex":
      return join(projectRoot, ".codex", "skills");
    case "claude":
      return join(projectRoot, ".claude", "skills");
    case "qoder":
      return join(projectRoot, ".qoder", "skills");
    case "custom":
      return config?.customSkillDir
        ? resolveProjectPath(projectRoot, config.customSkillDir)
        : "";
    default:
      return "";
  }
}

export async function fetchJson(url, init) {
  const response = await fetch(url, init);
  const text = await response.text();
  let payload;
  try {
    payload = text ? JSON.parse(text) : {};
  } catch {
    payload = { raw: text };
  }

  if (!response.ok) {
    throw new Error(payload.error ?? payload.message ?? `Request failed: ${response.status} ${text}`);
  }

  return payload;
}

export function printCheck(name, ok, detail) {
  console.log(`${ok ? "[ok]" : "[fail]"} ${name}${detail ? `: ${detail}` : ""}`);
}
