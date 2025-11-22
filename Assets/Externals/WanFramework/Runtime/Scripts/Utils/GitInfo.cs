using System;
using System.IO;
using JetBrains.Annotations;

namespace WanFramework.Utils
{
    public static class GitInfo
    {
        private const string UnknownGitHash = "ffffffffffffffffffffffffffffffffffffffff";
        
        [CanBeNull]
        private static string _gitInfo;
        
        #if UNITY_EDITOR
        public static string GetRevisionHash()
        {
            if (!File.Exists(".git/HEAD")) return UnknownGitHash;
            var headInfo = File.ReadAllText(".git/HEAD").Split('\n')[0];
            if (!headInfo.StartsWith("ref:"))
                return headInfo.Length == 40 ? headInfo : UnknownGitHash;
            var refSubPath = headInfo.Substring("ref:".Length).Trim();
            var refPath = $".git/{refSubPath}";
            if (!File.Exists(refPath)) return UnknownGitHash;
            var refInfo = File.ReadAllText(refPath).Split('\n')[0];
            _gitInfo = refInfo.Length == 40 ? refInfo : UnknownGitHash;
            return _gitInfo;
        }
        #else
        public static string GetRevisionHash()
        {
            if (_gitInfo != null) return _gitInfo;
            var gitInfo = UnityEngine.Resources.Load<UnityEngine.TextAsset>("GitInfo");
            if (gitInfo != null) return gitInfo.text;
            return UnknownGitHash;
        }
        #endif
    }
}