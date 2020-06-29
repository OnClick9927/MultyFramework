using System;
using System.Linq;
using UnityEditor;

namespace MutiFramework
{
    public static class EditorFrameworks
    {
//ToDo
        public static ExampleFrame1 ExampleFrame1 { get { return GetFramework("ExampleFrame1") as ExampleFrame1; } }
//ToDo

        /// <summary>
        /// ¿ò¼ÜÈÝÆ÷
        /// </summary>
        public static MutiFrameworkContaner _container;

        [InitializeOnLoadMethod]
        static void Startup()
        {
            _container = new MutiFrameworkContaner();
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
                     _container.Subscribe(f);
                 });
            EditorApplication.update += _container.Update;
# if UNITY_2018_1_OR_NEWER
            EditorApplication.quitting += _container.Dispose;
#endif
            _container.Startup();
        }

        /// <summary>
        /// »ñÈ¡¿ò¼Ü
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Framework GetFramework(string name)
        {
            return _container.Get(name);
        }
    }
}
