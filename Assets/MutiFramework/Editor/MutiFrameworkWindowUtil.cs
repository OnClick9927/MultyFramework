#if MutiFramework
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
namespace MutiFramework
{
    class MutiFrameworkWindowUtil
    {
        internal static void CreateClass()
        {

            string flag = "//ToDo";
            string editorPath = "Assets/MutiFramework/Editor/EditorFrameworks.cs";
            string rtPath = "Assets/MutiFramework/Frameworks.cs";
            Encoding utf8 = Encoding.UTF8;
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany((a) => { return a.GetTypes(); });
            EditorUtility.DisplayProgressBar("Fresh Scripts", "", 0);

            string txt = File.ReadAllText(editorPath, utf8);
            string add = "";
            types
                 .Where((type) => {
                     return !type.IsAbstract && type.IsSubclassOf(typeof(Framework)) &&
                         type.IsDefined(typeof(FrameworkAttribute), false) &&
                         (type.GetCustomAttributes(typeof(FrameworkAttribute), false).First() as FrameworkAttribute).env.HasFlag(EnvironmentType.Editor);
                 })
                 .Select((type) => {
                     Framework f = Activator.CreateInstance(type) as Framework;
                     return f;
                 }).ToList()
                 .ForEach((f) => {
                     add = add + "\t\tpublic static " + f.GetType() + " " + f.name + "{ get { return GetFramework(\"" + f.name + "\") as " + f.GetType() + ";}} \n";
                     //_container.Subscribe(f);
                     f.Dispose();
                 });
            int first = txt.IndexOf(flag);
            int last = txt.LastIndexOf(flag);
            string _1 = txt.Substring(0, first);
            string _2 = flag + "\n" + add;
            string _3 = txt.Substring(last, txt.Length - last);

            txt = _1 + _2 + _3;
            File.WriteAllText(editorPath, txt.Replace("\r\n", "\n"), utf8);
            EditorUtility.DisplayProgressBar("Fresh Scripts", "", 0.5f);



            txt = File.ReadAllText(rtPath, utf8);
            add = "";
            types
                 .Where((type) => {
                     return !type.IsAbstract && type.IsSubclassOf(typeof(Framework)) &&
                         type.IsDefined(typeof(FrameworkAttribute), false) &&
                         (type.GetCustomAttributes(typeof(FrameworkAttribute), false).First() as FrameworkAttribute).env.HasFlag(EnvironmentType.Runtime);
                 })
                 .Select((type) => {
                     Framework f = Activator.CreateInstance(type) as Framework;
                     return f;
                 }).ToList()
                 .ForEach((f) => {
                     add = add + "\t\tpublic static " + f.GetType() + " " + f.name + "{ get { return GetFramework(\"" + f.name + "\") as " + f.GetType() + ";}} \n";
                     //_container.Subscribe(f);
                     f.Dispose();
                 });

            first = txt.IndexOf(flag);
            last = txt.LastIndexOf(flag);
            _1 = txt.Substring(0, first);
            _2 = flag + "\n" + add;
            _3 = txt.Substring(last, txt.Length - last);

            txt = _1 + _2 + _3;
            File.WriteAllText(rtPath, txt.Replace("\r\n", "\n"), utf8);
            EditorUtility.DisplayProgressBar("Fresh Scripts", "", 0.8f);

            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();

        }


        internal static List<CollectionInfo> GetCollections()
        {
            return new List<CollectionInfo>()
            {
                new CollectionInfo("TestName02","7.26","author","describtion","Assets",null,null,Application.unityVersion,null),
                new CollectionInfo("TestName01","7.26.54","author","describtion","Assets",null,null,Application.unityVersion,null),
            };
        }
        internal static void RemovePakage(string path)
        {
            Debug.Log(string.Format("remove assets at path\n {0}", path));
        }
        internal static void InstallPakage(CollectionInfo info)
        {
            Debug.Log(string.Format("Install Pakage with name{1}\n at path\n {0} ", info.assetPath,info.name));
        }
        internal static void InstallPakageAgain(CollectionInfo info)
        {
            Debug.Log(string.Format("Install Pakage again with name{1}\n at path\n {0} ", info.assetPath, info.name));

        }

    }
}
#endif
