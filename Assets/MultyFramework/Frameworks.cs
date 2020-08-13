#if MultyFramework
using System;
using System.Linq;
using UnityEngine;

namespace MultyFramework
{
    /// <summary>
    /// 框架入口
    /// </summary>
    public class Frameworks : MonoBehaviour
    {
//ToDo
//ToDo

        /// <summary>
        /// 框架容器
        /// </summary>
        public static MultyFrameworkContaner container;
        /// <summary>
        /// 开启
        /// </summary>
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
        public void Start()
        {
            Startup();
            container.Startup();
        }
        public void Update()
        {
            container.Update();
        }
        public void OnDisable()
        {
            container.Dispose();
            container = null;
        }
    }
}

#endif