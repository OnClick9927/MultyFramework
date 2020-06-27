using System;
using System.Linq;
using UnityEditor;
namespace MutiFramework
{
    public static class EditorFrameworks
    {
//ToDo
        private static MutiFrameworkContaner _container;
        [InitializeOnLoadMethod]
        static void Startup()
        {
            _container = new MutiFrameworkContaner();
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany((a) => { return a.GetTypes(); });
           types
                .Where((type) => { return !type.IsAbstract && type.IsSubclassOf(typeof(Framework)) &&
                    type.IsDefined(typeof(FrameworkAttribute),false) && 
                    (type.GetCustomAttributes(typeof(FrameworkAttribute), false).First() as FrameworkAttribute).env.HasFlag(Environment.Editor); })
                .Select((type) => {
                    Framework f = Activator.CreateInstance(type) as Framework;
                    f.env = Environment.Editor;
                    return f; }).ToList()
                .ForEach((f)=> {
                    _container.Subscribe(f);
                });
            EditorApplication.update += _container.Update;
            EditorApplication.quitting += _container.Dispose;
            _container.Startup();
        }
        private static Framework GetFrame(string name)
        {
            return _container.Get(name);
        }
    }
}
