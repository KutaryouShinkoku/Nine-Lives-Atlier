//    ▄▀▀▀▄▄▄▄▄▄▄▀▀▀▄
//    █▒▒░░░░░░░░░▒▒█    CodeRunner.cs
//     █░░█░░░░░█░░█     Created by WanNeng
//  ▄▄  █░░░▀█▀░░░█  ▄▄  Created   12/24/2023 15:07
// █░░█ ▀▄░░░░░░░▄▀ █░░█

using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using UnityEditor;
using UnityEditor.Compilation;
using System.Reflection;
using DynamicExpresso;
using Microsoft.CSharp;
using UnityEngine;
using WanFramework.Base;
using Assembly = UnityEditor.Compilation.Assembly;

namespace WanFramework.Editor.Debugger
{
    public class CodeRunner
    {
        private readonly Interpreter _interpreter;

        private static Type[] _referenceTypes = new[]
        {
            typeof(UnityEngine.Debug),
        };
        public CodeRunner()
        {
            _interpreter = new();
            _interpreter.EnableReflection();
            _interpreter.Reference(typeof(UnityEngine.Debug));
            _interpreter.Reference(typeof(Application));
            _interpreter.Reference(typeof(UnityEngine.Object));
            foreach (var type in 
                     AppDomain.CurrentDomain
                         .GetAssemblies()
                         .SelectMany(asm => asm.GetTypes())
                         .Where(t => typeof(Component).IsAssignableFrom(t) && !t.IsAbstract))
                _interpreter.Reference(type);
            _interpreter.SetFunction("Help", (Func<string>)CommandHelp);
        }

        private string CommandHelp()
        {
            return
                "Help => Display this menu.\n";
        }
        
        public object Run(string script)
        {
            return _interpreter.Eval(script);
        }
    }
}