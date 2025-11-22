using System.IO;
using UnityEditor;
using UnityEngine;

namespace WanFramework.Editor.Utils
{
    public static class PersistentDataHelper
    {
        [MenuItem("WanFramework/存档/打开持久化数据档路径")]
        private static void ShowPersistentInExplorer()
            => EditorUtility.RevealInFinder(Application.persistentDataPath + "/");

        [MenuItem("WanFramework/存档/清空持久化数据")]
        private static void DeleteAllPersistentData()
        {
            if (EditorUtility.DisplayDialog(
                    "确认", 
                    $"你真的真的要删掉{Application.persistentDataPath}目录下的所有文件吗？", 
                    "是的",
                    "我后悔了"))
                Directory.Delete(Application.persistentDataPath, true);
        }
    }
}