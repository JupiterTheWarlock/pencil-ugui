using System;
using UnityEditor;
using UnityEngine;

namespace OpenPencilUGUI
{
    public static class PencilUgImportService
    {
        public static Transform GetSelectedImportTarget()
        {
            var selected = Selection.activeGameObject;
            if (selected == null)
            {
                return null;
            }

            return selected.transform is RectTransform
                ? selected.transform
                : null;
        }

        public static SelectionTargetInfo DescribeSelectionTarget()
        {
            var selected = Selection.activeGameObject;
            if (selected == null)
            {
                return new SelectionTargetInfo
                {
                    valid = false,
                    message = "No GameObject selected in the Hierarchy."
                };
            }

            var target = GetSelectedImportTarget();
            if (target == null)
            {
                return new SelectionTargetInfo
                {
                    valid = false,
                    message = "Select a GameObject with a RectTransform."
                };
            }

            var targetPath = BuildHierarchyPath(target);
            return new SelectionTargetInfo
            {
                valid = true,
                target = "selection",
                targetName = target.name,
                targetPath = targetPath,
                canvasName = target.name,
                canvasPath = targetPath,
                selectedName = selected.name
            };
        }

        public static ImportResult Import(string jsonPath, string targetMode)
        {
            if (!string.Equals(targetMode, "selection", StringComparison.OrdinalIgnoreCase))
            {
                return ImportResult.Fail($"Unsupported target mode: {targetMode}");
            }

            var target = GetSelectedImportTarget();
            if (target == null)
            {
                return ImportResult.Fail("Select a GameObject with a RectTransform before importing UI IR.");
            }

            PencilUgImporter.Import(jsonPath, target);
            return ImportResult.Ok(target.name, BuildHierarchyPath(target));
        }

        static string BuildHierarchyPath(Transform transform)
        {
            var path = transform.name;
            var current = transform.parent;
            while (current != null)
            {
                path = $"{current.name}/{path}";
                current = current.parent;
            }

            return path;
        }
    }

    [Serializable]
    public class SelectionTargetInfo
    {
        public bool valid;
        public string target;
        public string targetName;
        public string targetPath;
        public string canvasName;
        public string canvasPath;
        public string selectedName;
        public string message;
    }

    [Serializable]
    public class ImportResult
    {
        public bool ok;
        public string message;
        public string targetName;
        public string targetPath;
        public string canvasName;
        public string canvasPath;

        public static ImportResult Ok(string targetName, string targetPath)
        {
            return new ImportResult
            {
                ok = true,
                message = "Import completed.",
                targetName = targetName,
                targetPath = targetPath,
                canvasName = targetName,
                canvasPath = targetPath
            };
        }

        public static ImportResult Fail(string message)
        {
            return new ImportResult
            {
                ok = false,
                message = message
            };
        }
    }
}
