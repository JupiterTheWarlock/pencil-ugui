import { loadConfig } from "./config.mjs";

export async function runPromptContext(options) {
  const { config, configPath } = loadConfig(options.projectRoot);
  const provider = options.provider || config.provider || "open-pencil";

  const context = {
    configPath: ".pencil-ugui/config.json",
    provider,
    openPencilDir: config.openPencilDir,
    exporterCommand: config.exporterCommand,
    generatedDir: config.generatedDir ?? ".pencil-ugui/generated",
    defaultTargetMode: config.defaultTargetMode ?? "selection",
    unityServerPort: config.serverPort ?? 47123,
    skillTargets: config.skillTargets ?? ["cursor"],
    customSkillDir: config.customSkillDir ?? "",
    handlers: [],
    notes: [
      "Import only UI IR JSON into Unity. Do not depend on .fig format inside Unity.",
      "MVP import target mode supports selection on a Canvas.",
      "Custom component handlers are not registered yet.",
    ],
    providerReference: `.cursor/skills/pencil-ugui/references/${provider}/SKILL.md`,
  };

  console.log(JSON.stringify(context, null, 2));
}
