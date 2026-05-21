using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace OpenPencilUGUI
{
    public static class PencilUgSkillInstaller
    {
        const string SkillFolderName = "pencil-ugui";

        public static SkillInstallResult Install(PencilUgConfig config)
        {
            var templateRoot = GetTemplateRoot();
            if (!Directory.Exists(templateRoot))
            {
                return SkillInstallResult.Fail($"Skill template folder not found: {templateRoot}");
            }

            var installedTargets = new System.Collections.Generic.List<string>();
            foreach (var target in GetRequiredSkillTargets(config))
            {
                var destinationRoot = GetDestinationRoot(config, target);
                if (string.IsNullOrWhiteSpace(destinationRoot))
                {
                    continue;
                }

                CopyDirectory(templateRoot, Path.Combine(destinationRoot, SkillFolderName));
                installedTargets.Add(target);
            }

            if (installedTargets.Count == 0)
            {
                return SkillInstallResult.Fail("No skill targets selected.");
            }

            return SkillInstallResult.Ok(installedTargets.ToArray());
        }

        public static string[] GetRequiredSkillTargets(PencilUgConfig config)
        {
            if (config.skillTargets == null || config.skillTargets.Length == 0)
            {
                return new[] { "cursor" };
            }

            return config.skillTargets;
        }

        public static bool IsSkillInstalled(PencilUgConfig config, string target)
        {
            return File.Exists(GetSkillFilePath(config, target));
        }

        public static bool IsCursorSkillInstalled()
        {
            var config = PencilUgConfigStore.LoadOrCreateDefault();
            return IsSkillInstalled(config, "cursor");
        }

        public static string GetSkillFileRelativePath(PencilUgConfig config, string target)
        {
            return PencilUgHarnessPaths.ToProjectRelativePath(GetSkillFilePath(config, target));
        }

        public static string GetSkillFilePath(PencilUgConfig config, string target)
        {
            var destinationRoot = GetDestinationRoot(config, target);
            return string.IsNullOrWhiteSpace(destinationRoot)
                ? string.Empty
                : Path.Combine(destinationRoot, SkillFolderName, "SKILL.md");
        }

        static string GetTemplateRoot()
        {
            var guids = AssetDatabase.FindAssets("PencilUgSkillInstaller t:Script");
            if (guids.Length == 0)
            {
                return string.Empty;
            }

            var scriptPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            var editorDir = Path.GetDirectoryName(scriptPath);
            return Path.Combine(editorDir, "Templates", SkillFolderName);
        }

        static string GetDestinationRoot(PencilUgConfig config, string target)
        {
            switch (target?.Trim().ToLowerInvariant())
            {
                case "cursor":
                    return Path.Combine(PencilUgHarnessPaths.ProjectRoot, ".cursor", "skills");
                case "codex":
                    return Path.Combine(PencilUgHarnessPaths.ProjectRoot, ".codex", "skills");
                case "claude":
                    return Path.Combine(PencilUgHarnessPaths.ProjectRoot, ".claude", "skills");
                case "qoder":
                    return Path.Combine(PencilUgHarnessPaths.ProjectRoot, ".qoder", "skills");
                case "custom":
                    return string.IsNullOrWhiteSpace(config.customSkillDir)
                        ? null
                        : PencilUgHarnessPaths.ResolveProjectPath(config.customSkillDir);
                default:
                    return null;
            }
        }

        static void CopyDirectory(string sourceDir, string destinationDir)
        {
            Directory.CreateDirectory(destinationDir);

            foreach (var file in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
            {
                var relativePath = file.Substring(sourceDir.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                var destinationPath = Path.Combine(destinationDir, relativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));
                File.Copy(file, destinationPath, true);
            }
        }
    }

    [Serializable]
    public class SkillInstallResult
    {
        public bool ok;
        public string message;
        public string[] targets;

        public static SkillInstallResult Ok(string[] targets)
        {
            return new SkillInstallResult
            {
                ok = true,
                message = "Skill installed.",
                targets = targets
            };
        }

        public static SkillInstallResult Fail(string message)
        {
            return new SkillInstallResult
            {
                ok = false,
                message = message
            };
        }
    }
}
