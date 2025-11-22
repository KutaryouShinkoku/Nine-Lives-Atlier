//    ▄▀▀▀▄▄▄▄▄▄▄▀▀▀▄
//    █▒▒░░░░░░░░░▒▒█    WanFrameworkEditorUtils.cs
//     █░░█░░░░░█░░█     Created by WanNeng
//  ▄▄  █░░░▀█▀░░░█  ▄▄  Created   01/01/2024 18:10
// █░░█ ▀▄░░░░░░░▄▀ █░░█

using System.IO;
using UnityEditor.Compilation;

namespace WanFramework.Editor.Utils
{
    public class WanFrameworkEditorUtils
    {
        public static string GetEditorRoot()
        {
            return Path.GetDirectoryName(CompilationPipeline.GetAssemblyDefinitionFilePathFromAssemblyName("WanFramework.Editor"));
        }
    }
}