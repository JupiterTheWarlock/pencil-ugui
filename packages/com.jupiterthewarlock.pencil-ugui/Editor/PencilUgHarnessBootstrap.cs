using System.IO;
using UnityEditor;
using UnityEngine;

namespace OpenPencilUGUI
{
    public static class PencilUgHarnessBootstrap
    {
        const string EmbeddedToolsFolderName = "HarnessTools";

        public static void EnsureProjectTools()
        {
            var embeddedToolsRoot = GetEmbeddedToolsRoot();
            if (!Directory.Exists(embeddedToolsRoot))
            {
                Debug.LogWarning($"Embedded harness tools not found: {embeddedToolsRoot}");
                return;
            }

            CopyIfMissing(
                Path.Combine(embeddedToolsRoot, "pencil-ugui-cli"),
                Path.Combine(PencilUgHarnessPaths.ProjectToolsDirectory, "pencil-ugui-cli"));
            CopyIfMissing(
                Path.Combine(embeddedToolsRoot, "open-pencil-exporter"),
                Path.Combine(PencilUgHarnessPaths.ProjectToolsDirectory, "open-pencil-exporter"));
        }

        static string GetEmbeddedToolsRoot()
        {
            var guids = AssetDatabase.FindAssets("PencilUgHarnessBootstrap t:Script");
            if (guids.Length == 0)
            {
                return string.Empty;
            }

            var scriptPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            var editorDir = Path.GetDirectoryName(scriptPath);
            return Path.Combine(editorDir, EmbeddedToolsFolderName);
        }

        static void CopyIfMissing(string sourceDir, string destinationDir)
        {
            if (!Directory.Exists(sourceDir))
            {
                return;
            }

            if (Directory.Exists(destinationDir))
            {
                return;
            }

            CopyDirectory(sourceDir, destinationDir);
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
}
