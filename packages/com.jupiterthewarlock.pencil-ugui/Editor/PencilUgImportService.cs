using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace OpenPencilUGUI
{
    public static class PencilUgImportService
    {
        public static Canvas GetSelectedCanvas()
        {
            var selected = Selection.activeGameObject;
            if (selected == null)
            {
                return null;
            }

            var canvas = selected.GetComponent<Canvas>();
            if (canvas != null)
            {
                return canvas;
            }

            return selected.GetComponentInParent<Canvas>();
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

            var canvas = GetSelectedCanvas();
            if (canvas == null)
            {
                return new SelectionTargetInfo
                {
                    valid = false,
                    message = "Select a Canvas or a child of a Canvas."
                };
            }

            return new SelectionTargetInfo
            {
                valid = true,
                target = "selection",
                canvasName = canvas.name,
                canvasPath = BuildHierarchyPath(canvas.transform),
                selectedName = selected.name
            };
        }

        public static ImportResult Import(string jsonPath, string targetMode)
        {
            if (!string.Equals(targetMode, "selection", StringComparison.OrdinalIgnoreCase))
            {
                return ImportResult.Fail($"Unsupported target mode: {targetMode}");
            }

            var canvas = GetSelectedCanvas();
            if (canvas == null)
            {
                return ImportResult.Fail("Select a Canvas in the Hierarchy before importing UI IR.");
            }

            PencilUgImporter.Import(jsonPath, canvas);
            return ImportResult.Ok(canvas.name, BuildHierarchyPath(canvas.transform));
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
        public string canvasName;
        public string canvasPath;

        public static ImportResult Ok(string canvasName, string canvasPath)
        {
            return new ImportResult
            {
                ok = true,
                message = "Import completed.",
                canvasName = canvasName,
                canvasPath = canvasPath
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
