using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace JtheWL.PencilUguiImporter
{
    public static class PencilUgImporter
    {
        static readonly Regex IdSuffixPattern = new Regex(@" \[([^\]]+)\]$", RegexOptions.Compiled);

        public static void Import(string jsonPath, Canvas canvas)
        {
            var json = File.ReadAllText(jsonPath);
            var document = JsonUtility.FromJson<UiIrDocument>(json);
            if (document?.nodes == null || document.nodes.Length == 0)
            {
                throw new System.InvalidOperationException("UI IR document has no root nodes.");
            }

            if (string.IsNullOrEmpty(document.documentId))
            {
                throw new System.InvalidOperationException("UI IR documentId is required.");
            }

            var documentRoot = GetOrCreateDocumentRoot(canvas.transform, document.documentId);
            var existingById = CollectExistingNodes(documentRoot);
            foreach (var node in document.nodes)
            {
                ImportNode(node, documentRoot, existingById);
            }

            EditorUtility.SetDirty(canvas.gameObject);
        }

        static Transform GetOrCreateDocumentRoot(Transform canvas, string documentId)
        {
            var rootName = $"UI [{documentId}]";
            for (var i = 0; i < canvas.childCount; i++)
            {
                var child = canvas.GetChild(i);
                if (child.name == rootName)
                {
                    return child;
                }
            }

            var documentRoot = new GameObject(rootName, typeof(RectTransform));
            var rectTransform = documentRoot.GetComponent<RectTransform>();
            rectTransform.SetParent(canvas, false);
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            Undo.RegisterCreatedObjectUndo(documentRoot, "Import UI IR");
            return documentRoot.transform;
        }

        static Dictionary<string, GameObject> CollectExistingNodes(Transform root)
        {
            var map = new Dictionary<string, GameObject>();
            CollectExistingNodesRecursive(root, map);
            return map;
        }

        static void CollectExistingNodesRecursive(Transform transform, Dictionary<string, GameObject> map)
        {
            for (var i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                var match = IdSuffixPattern.Match(child.name);
                if (match.Success)
                {
                    map[match.Groups[1].Value] = child.gameObject;
                }

                CollectExistingNodesRecursive(child, map);
            }
        }

        static void ImportNode(UiIrNode node, Transform parent, Dictionary<string, GameObject> existingById)
        {
            var gameObject = GetOrCreateNode(node, parent, existingById);
            ApplyRectTransform(gameObject.GetComponent<RectTransform>(), node.bounds);
            ApplyVisuals(gameObject, node);

            if (node.children == null)
            {
                return;
            }

            foreach (var child in node.children)
            {
                ImportNode(child, gameObject.transform, existingById);
            }
        }

        static GameObject GetOrCreateNode(UiIrNode node, Transform parent, Dictionary<string, GameObject> existingById)
        {
            var objectName = $"{node.name} [{node.id}]";
            if (existingById.TryGetValue(node.id, out var existing) && existing != null)
            {
                existing.name = objectName;
                existing.transform.SetParent(parent, false);
                return existing;
            }

            var gameObject = new GameObject(objectName, typeof(RectTransform));
            gameObject.transform.SetParent(parent, false);
            existingById[node.id] = gameObject;
            Undo.RegisterCreatedObjectUndo(gameObject, "Import UI IR");
            return gameObject;
        }

        static void ApplyRectTransform(RectTransform rectTransform, UiIrBounds bounds)
        {
            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = new Vector2(0f, 1f);
            rectTransform.pivot = new Vector2(0f, 1f);
            rectTransform.anchoredPosition = new Vector2(bounds.x, -bounds.y);
            rectTransform.sizeDelta = new Vector2(bounds.width, bounds.height);
        }

        static void ApplyVisuals(GameObject gameObject, UiIrNode node)
        {
            var nodeType = node.type?.ToLowerInvariant();
            var solidFill = GetFirstSolidFill(node.fills);

            if (nodeType == "text")
            {
                ApplyText(gameObject, node, solidFill);
                return;
            }

            RemoveComponent<TextMeshProUGUI>(gameObject);

            if (nodeType == "rectangle" || (nodeType == "frame" && solidFill != null))
            {
                var image = GetOrAddComponent<Image>(gameObject);
                image.color = solidFill?.color.ToUnityColor() ?? Color.white;
                if (IsButtonNode(node))
                {
                    ApplyButton(gameObject, image);
                }
                else
                {
                    image.raycastTarget = false;
                    RemoveComponent<Button>(gameObject);
                }

                return;
            }

            RemoveComponent<Image>(gameObject);
            RemoveComponent<Button>(gameObject);
        }

        static bool IsButtonNode(UiIrNode node)
        {
            return node.name != null
                && node.name.IndexOf("Button", System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        static void ApplyButton(GameObject gameObject, Image image)
        {
            var button = GetOrAddComponent<Button>(gameObject);
            image.raycastTarget = true;
            button.targetGraphic = image;
        }

        static void ApplyText(GameObject gameObject, UiIrNode node, UiIrFill solidFill)
        {
            RemoveComponent<Image>(gameObject);
            RemoveComponent<Button>(gameObject);

            var text = GetOrAddComponent<TextMeshProUGUI>(gameObject);
            text.text = node.text?.characters ?? string.Empty;
            text.fontSize = node.text?.fontSize > 0f ? node.text.fontSize : 14f;
            text.color = solidFill?.color.ToUnityColor() ?? Color.black;
            text.alignment = TextAlignmentOptions.TopLeft;
            text.raycastTarget = false;
        }

        static UiIrFill GetFirstSolidFill(UiIrFill[] fills)
        {
            if (fills == null)
            {
                return null;
            }

            foreach (var fill in fills)
            {
                if (fill != null && fill.type == "SOLID" && fill.color != null)
                {
                    return fill;
                }
            }

            return null;
        }

        static T GetOrAddComponent<T>(GameObject gameObject) where T : Component
        {
            var component = gameObject.GetComponent<T>();
            if (component != null)
            {
                return component;
            }

            return gameObject.AddComponent<T>();
        }

        static void RemoveComponent<T>(GameObject gameObject) where T : Component
        {
            var component = gameObject.GetComponent<T>();
            if (component != null)
            {
                Object.DestroyImmediate(component);
            }
        }
    }
}
