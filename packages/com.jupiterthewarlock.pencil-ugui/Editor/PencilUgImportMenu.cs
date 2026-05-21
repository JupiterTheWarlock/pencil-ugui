using UnityEditor;
using UnityEngine;

namespace OpenPencilUGUI
{
    public static class PencilUgImportMenu
    {
        [MenuItem("Tools/Pencil UGUI/Import UI IR...")]
        static void ImportUiIr()
        {
            var target = PencilUgImportService.GetSelectedImportTarget();
            if (target == null)
            {
                EditorUtility.DisplayDialog(
                    "Pencil UGUI Importer",
                    "Select a GameObject with a RectTransform before importing UI IR.",
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
                PencilUgImporter.Import(jsonPath, target);
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
            return PencilUgImportService.GetSelectedImportTarget() != null;
        }

        [MenuItem("GameObject/Pencil UGUI/Import UI IR...", false, 49)]
        static void ImportUiIrFromGameObject()
        {
            ImportUiIr();
        }

        [MenuItem("GameObject/Pencil UGUI/Import UI IR...", true)]
        static bool ImportUiIrFromGameObjectValidate()
        {
            return ImportUiIrValidate();
        }
    }
}
