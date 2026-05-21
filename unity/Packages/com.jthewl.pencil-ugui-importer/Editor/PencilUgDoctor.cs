using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace OpenPencilUGUI
{
    public static class PencilUgDoctor
    {
        public static DoctorReport Run(PencilUgConfig config)
        {
            var checks = new List<DoctorCheck>();
            var failed = false;

            var configExists = File.Exists(PencilUgHarnessPaths.ConfigFilePath);
            checks.Add(new DoctorCheck("config", configExists, PencilUgHarnessPaths.ConfigRelativePath));
            failed |= !configExists;

            var providerOk = config.provider == "open-pencil";
            checks.Add(new DoctorCheck("provider", providerOk, config.provider));
            failed |= !providerOk;

            var openPencilDir = PencilUgHarnessPaths.ResolveProjectPath(config.openPencilDir);
            var openPencilOk = !string.IsNullOrWhiteSpace(config.openPencilDir) && Directory.Exists(openPencilDir);
            checks.Add(new DoctorCheck(
                "open-pencil dir",
                openPencilOk,
                DescribePathCheck(config.openPencilDir, openPencilDir, "Select your open-pencil checkout. Relative paths resolve from the Unity project root.")));
            failed |= !openPencilOk;

            var exporterScript = ResolveExporterScript(config);
            var exporterOk = File.Exists(exporterScript);
            checks.Add(new DoctorCheck(
                "exporter script",
                exporterOk,
                DescribePathCheck(config.exporterCommand, exporterScript, "Harness exporter should live under .open-pencil-ugui/tools/ after setup.")));
            failed |= !exporterOk;

            try
            {
                PencilUgHarnessPaths.EnsureHarnessDirectories(config);
                checks.Add(new DoctorCheck("generated dir", true, config.generatedDir));
            }
            catch (System.Exception exception)
            {
                checks.Add(new DoctorCheck("generated dir", false, exception.Message));
                failed = true;
            }

            foreach (var target in PencilUgSkillInstaller.GetRequiredSkillTargets(config))
            {
                var skillOk = PencilUgSkillInstaller.IsSkillInstalled(config, target);
                var skillPath = PencilUgSkillInstaller.GetSkillFileRelativePath(config, target);
                checks.Add(new DoctorCheck($"{target} skill", skillOk, skillPath));
                failed |= !skillOk;
            }

            var serverOk = TryHealthCheck(config, out var serverDetail);
            checks.Add(new DoctorCheck("unity local server", serverOk, serverDetail));
            failed |= !serverOk;

            return new DoctorReport
            {
                passed = !failed,
                checks = checks.ToArray()
            };
        }

        static string DescribePathCheck(string configuredValue, string resolvedPath, string hint)
        {
            if (string.IsNullOrWhiteSpace(configuredValue))
            {
                return hint;
            }

            if (Directory.Exists(resolvedPath) || File.Exists(resolvedPath))
            {
                return configuredValue;
            }

            return $"{configuredValue} -> {resolvedPath}. {hint}";
        }

        static bool TryHealthCheck(PencilUgConfig config, out string detail)
        {
            detail = $"http://127.0.0.1:{config.serverPort}/health";
            if (!PencilUgLocalServer.IsRunning)
            {
                detail = "Server is not running. Start it from the setup window.";
                return false;
            }

            if (PencilUgLocalServer.TryGetHealth(out var healthError))
            {
                return true;
            }

            detail = healthError;
            return false;
        }

        static string ResolveExporterScript(PencilUgConfig config)
        {
            var command = config.exporterCommand ?? string.Empty;
            var parts = command.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
            {
                return string.Empty;
            }

            return PencilUgHarnessPaths.ResolveProjectPath(parts[1]);
        }
    }

    [System.Serializable]
    public class DoctorCheck
    {
        public string name;
        public bool ok;
        public string detail;

        public DoctorCheck(string name, bool ok, string detail)
        {
            this.name = name;
            this.ok = ok;
            this.detail = detail;
        }
    }

    [System.Serializable]
    public class DoctorReport
    {
        public bool passed;
        public DoctorCheck[] checks;

        public string Format()
        {
            var builder = new StringBuilder();
            foreach (var check in checks)
            {
                builder.Append(check.ok ? "[ok] " : "[fail] ");
                builder.Append(check.name);
                if (!string.IsNullOrWhiteSpace(check.detail))
                {
                    builder.Append(": ").Append(check.detail);
                }

                builder.AppendLine();
            }

            builder.AppendLine(passed ? "Doctor passed." : "Doctor found issues.");
            return builder.ToString();
        }
    }
}
