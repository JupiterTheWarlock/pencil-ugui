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
        public string exporterCommand = "node .open-pencil-ugui/tools/open-pencil-exporter/export.mjs";
        public int serverPort = 47123;
        public string generatedDir = ".open-pencil-ugui/generated";
        public string defaultTargetMode = "selection";
        public string[] skillTargets = { "cursor" };
        public string customSkillDir = "";
    }

    public static class PencilUgConfigStore
    {
        public static PencilUgConfig LoadOrCreateDefault()
        {
            PencilUgHarnessBootstrap.EnsureProjectTools();

            if (!File.Exists(PencilUgHarnessPaths.ConfigFilePath))
            {
                var config = CreateDefault();
                Save(config);
                return config;
            }

            try
            {
                var json = File.ReadAllText(PencilUgHarnessPaths.ConfigFilePath);
                var config = JsonUtility.FromJson<PencilUgConfig>(json);
                if (config == null)
                {
                    throw new InvalidOperationException("Config file is empty.");
                }

                if (config.skillTargets == null || config.skillTargets.Length == 0)
                {
                    config.skillTargets = new[] { "cursor" };
                }

                MigrateConfig(config);
                return config;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Failed to read harness config, using defaults: {exception.Message}");
                return CreateDefault();
            }
        }

        public static PencilUgConfig ReadFromDisk()
        {
            if (!File.Exists(PencilUgHarnessPaths.ConfigFilePath))
            {
                return CreateDefault();
            }

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

            MigrateConfig(config);
            return config;
        }

        static void MigrateConfig(PencilUgConfig config)
        {
            if (string.IsNullOrWhiteSpace(config.exporterCommand)
                || (config.exporterCommand.Contains("tools/open-pencil-exporter/export.mjs")
                    && !config.exporterCommand.Contains(".open-pencil-ugui/tools/")))
            {
                config.exporterCommand = "node .open-pencil-ugui/tools/open-pencil-exporter/export.mjs";
            }
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
