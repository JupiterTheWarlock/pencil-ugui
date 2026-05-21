using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace OpenPencilUGUI
{
    [Serializable]
    public class PencilUgConfig
    {
        public int version = 1;
        public string provider = "open-pencil";
        public string openPencilDir = "";
        public string exporterCommand = "node .pencil-ugui/tools/open-pencil-exporter/export.mjs";
        public int serverPort = 47123;
        public string generatedDir = ".pencil-ugui/generated";
        public string defaultTargetMode = "selection";
        public string[] skillTargets = { "cursor" };
        public string customSkillDir = "";
    }

    public static class PencilUgConfigStore
    {
        public static PencilUgConfig LoadOrCreateDefault()
        {
            PencilUgHarnessBootstrap.EnsureProjectTools();
            MigrateLegacyConfigFileIfNeeded();

            if (!File.Exists(PencilUgHarnessPaths.ConfigFilePath))
            {
                var config = CreateDefault();
                Save(config);
                return config;
            }

            return ReadFromDisk();
        }

        static void MigrateLegacyConfigFileIfNeeded()
        {
            if (File.Exists(PencilUgHarnessPaths.ConfigFilePath))
            {
                return;
            }

            var legacyConfigPath = Path.Combine(
                PencilUgHarnessPaths.ProjectRoot,
                PencilUgHarnessPaths.LegacyConfigDirName,
                PencilUgHarnessPaths.ConfigFileName);
            if (!File.Exists(legacyConfigPath))
            {
                return;
            }

            Directory.CreateDirectory(PencilUgHarnessPaths.ConfigDirectory);
            File.Copy(legacyConfigPath, PencilUgHarnessPaths.ConfigFilePath, true);
        }

        public static PencilUgConfig ReadFromDisk()
        {
            MigrateLegacyConfigFileIfNeeded();

            if (!File.Exists(PencilUgHarnessPaths.ConfigFilePath))
            {
                return CreateDefault();
            }

            try
            {
                var json = File.ReadAllText(PencilUgHarnessPaths.ConfigFilePath);
                var config = JsonUtility.FromJson<PencilUgConfig>(json);
                if (config == null)
                {
                    return CreateDefault();
                }

                if (config.skillTargets == null || config.skillTargets.Length == 0)
                {
                    config.skillTargets = new[] { "cursor" };
                }

                var migrated = MigrateConfig(config);
                if (migrated)
                {
                    Save(config);
                }

                return config;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Failed to read harness config, using defaults: {exception.Message}");
                return CreateDefault();
            }
        }

        static bool MigrateConfig(PencilUgConfig config)
        {
            var migrated = false;

            if (ReplaceLegacyPath(ref config.exporterCommand))
            {
                migrated = true;
            }

            if (ReplaceLegacyPath(ref config.generatedDir))
            {
                migrated = true;
            }

            if (string.IsNullOrWhiteSpace(config.exporterCommand)
                || (config.exporterCommand.Contains("tools/open-pencil-exporter/export.mjs")
                    && !config.exporterCommand.Contains(".pencil-ugui/tools/")))
            {
                config.exporterCommand = "node .pencil-ugui/tools/open-pencil-exporter/export.mjs";
                migrated = true;
            }

            return migrated;
        }

        static bool ReplaceLegacyPath(ref string value)
        {
            if (string.IsNullOrWhiteSpace(value) || !value.Contains(PencilUgHarnessPaths.LegacyConfigDirName))
            {
                return false;
            }

            value = value.Replace(PencilUgHarnessPaths.LegacyConfigDirName, PencilUgHarnessPaths.ConfigDirName);
            return true;
        }

        public static void MigrateAndSave(PencilUgConfig config)
        {
            MigrateConfig(config);
            Save(config);
        }

        public static PencilUgConfig CreateDefault()
        {
            return new PencilUgConfig();
        }

        public static void Save(PencilUgConfig config)
        {
            PencilUgHarnessBootstrap.EnsureProjectTools();
            PencilUgHarnessPaths.EnsureHarnessDirectories(config);
            var json = JsonUtility.ToJson(config, true);
            File.WriteAllText(PencilUgHarnessPaths.ConfigFilePath, json);
        }
    }
}
