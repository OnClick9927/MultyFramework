﻿#if MultyFramework
using System;
using System.Linq;
using UnityEditor;
namespace MultyFramework
{
    public class EditorFrameworks
    {
        //ToDo
//ToDo

        /// <summary>
        /// 框架容器
        /// </summary>
        public static MultyFrameworkContaner container;

        [InitializeOnLoadMethod]
        static void Startup()
        {
            container = new MultyFrameworkContaner();
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany((a) => { return a.GetTypes(); });
            types
                 .Where((type) => {
                     return !type.IsAbstract && type.IsSubclassOf(typeof(Framework)) &&
     type.IsDefined(typeof(FrameworkAttribute), false) &&
     (type.GetCustomAttributes(typeof(FrameworkAttribute), false).First() as FrameworkAttribute).env.HasFlag(EnvironmentType.Editor);
                 })
                 .Select((type) => {
                     Framework f = Activator.CreateInstance(type) as Framework;
                     f.env = EnvironmentType.Editor;
                     return f;
                 }).ToList()
                 .ForEach((f) => {
                     container.Subscribe(f);
                 });
            EditorApplication.update += container.Update;
            UnityEditor.Compilation.CompilationPipeline.assemblyCompilationStarted += (str) => {
                container.Dispose();
                UnityEngine.Debug.Log("EditorFrameworks Dispose");
            };
            container.Startup();
            UnityEngine.Debug.Log("EditorFrameworks Start");
        }

        /// <summary>
        /// 获取框架
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Framework GetFramework(string name)
        {
            return container.Get(name);
        }
    }
}

#endif
