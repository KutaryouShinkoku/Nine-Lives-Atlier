using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;
using UnityEngine;
using WanFramework.Utils;

namespace WanFramework.Editor.Utils
{
    internal class AddGitHashBuildPreprocessor : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        private static readonly string TmpRootPath = "Assets/Tmp/Resources";
        private static readonly string TmpResourcePath = $"{TmpRootPath}/Resources";

        public int callbackOrder => 1;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (!Directory.Exists(TmpResourcePath))
                Directory.CreateDirectory(TmpResourcePath);
            var filePath = Path.Combine(TmpResourcePath, "GitInfo.txt");
            var hash = GitInfo.GetRevisionHash();
            File.WriteAllText(filePath, hash);
            AssetDatabase.Refresh();
            Debug.Log($"Git hash: {hash}");
        }
        
        public void OnPostprocessBuild(BuildReport report)
        {
            File.Delete($"{TmpRootPath}.meta");
            Directory.Delete(TmpRootPath, true);
            AssetDatabase.Refresh();
        }
    }
}