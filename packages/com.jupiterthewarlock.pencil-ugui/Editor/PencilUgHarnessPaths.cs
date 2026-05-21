using System.IO;
using UnityEngine;

namespace OpenPencilUGUI
{
    public static class PencilUgHarnessPaths
    {
        public const string ConfigDirName = ".pencil-ugui";
        public const string LegacyConfigDirName = ".pencil-ugui";
        public const string ConfigFileName = "config.json";
        public const string ConfigRelativePath = ConfigDirName + "/" + ConfigFileName;
        public const string ProjectToolsDirName = "tools";

        public static string ProjectRoot => Directory.GetParent(Application.dataPath).FullName;

        public static string ConfigDirectory => Path.Combine(ProjectRoot, ConfigDirName);

        public static string ConfigFilePath => Path.Combine(ConfigDirectory, ConfigFileName);

        public static string ProjectToolsDirectory => Path.Combine(ConfigDirectory, ProjectToolsDirName);

        public static string GeneratedDirectory(PencilUgConfig config)
        {
            return ResolveProjectPath(config.generatedDir);
        }

        public static string ResolveProjectPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return ProjectRoot;
            }

            return Path.IsPathRooted(path) ? path : Path.Combine(ProjectRoot, path);
        }

        public static string ToProjectRelativePath(string absolutePath)
        {
            if (string.IsNullOrWhiteSpace(absolutePath))
            {
                return string.Empty;
            }

            var normalizedAbsolute = Path.GetFullPath(absolutePath);
            var normalizedRoot = Path.GetFullPath(ProjectRoot);
            if (!normalizedAbsolute.StartsWith(normalizedRoot))
            {
                return absolutePath;
            }

            return normalizedAbsolute
                .Substring(normalizedRoot.Length)
                .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .Replace('\\', '/');
        }

        public static void EnsureHarnessDirectories(PencilUgConfig config)
        {
            Directory.CreateDirectory(ConfigDirectory);
            Directory.CreateDirectory(GeneratedDirectory(config));
            Directory.CreateDirectory(Path.Combine(ConfigDirectory, "logs"));
            Directory.CreateDirectory(Path.Combine(ConfigDirectory, "prompts"));
        }
    }
}
