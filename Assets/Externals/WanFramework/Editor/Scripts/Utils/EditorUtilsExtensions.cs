using System.Text;
using UnityEditor;
using UnityEngine;

namespace WanFramework.Editor.Utils
{
    public static class EditorUtilsExtensions
    {
        private static string GetHierarchyPath(Transform t)
        {
            var path = $"/{t.name}";
            var current = t;
            while (current.parent != null)
            {
                current = current.parent;
                path = $"/{current.name}{path}";
            }
            return path;
        }
        [MenuItem("CONTEXT/Transform/Copy Path to Clipboard")]
        public static void CopyPath2Clipboard(MenuCommand command)
        {
            var t = (Transform)command.context;
            if (t == null) return;
            string path = GetHierarchyPath(t);
            GUIUtility.systemCopyBuffer = path;
        }
    }
}