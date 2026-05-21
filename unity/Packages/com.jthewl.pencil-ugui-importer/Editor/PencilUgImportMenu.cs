using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace JtheWL.PencilUguiImporter
{
    public static class PencilUgImportMenu
    {
        [MenuItem("Tools/Pencil UGUI/Import UI IR...")]
        static void ImportUiIr()
        {
            var canvas = Selection.activeGameObject?.GetComponent<Canvas>();
            if (canvas == null)
            {
                EditorUtility.DisplayDialog(
                    "Pencil UGUI Importer",
                    "Select a Canvas in the Hierarchy before importing UI IR.",
                    "OK");
                return;
            }

            var jsonPath = EditorUtility.OpenFilePanel("Import UI IR", "", "json");
            if (string.IsNullOrEmpty(jsonPath))
            {
                return;
            }

            try
            {
                PencilUgImporter.Import(jsonPath, canvas);
                Debug.Log($"Imported UI IR from {jsonPath}");
            }
            catch (System.Exception exception)
            {
                Debug.LogError($"UI IR import failed: {exception.Message}");
                EditorUtility.DisplayDialog("Pencil UGUI Importer", exception.Message, "OK");
            }
        }

        [MenuItem("Tools/Pencil UGUI/Import UI IR...", true)]
        static bool ImportUiIrValidate()
        {
            return Selection.activeGameObject != null
                && Selection.activeGameObject.GetComponent<Canvas>() != null;
        }
    }
}
