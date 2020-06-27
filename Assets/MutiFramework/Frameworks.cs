using System;
using System.Linq;
using UnityEngine;

namespace MutiFramework
{
    public class Frameworks : MonoBehaviour
    {
//ToDo
		public static ExampleFrame1 ExampleFrame1{ get { return GetFrame("ExampleFrame1") as ExampleFrame1;}} 
		public static ExampleFrame2 ExampleFrame2{ get { return GetFrame("ExampleFrame2") as ExampleFrame2;}} 
//ToDo
        private static MutiFrameworkContaner _container;
        static void Startup()
        {
            _container = new MutiFrameworkContaner();
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany((a) => { return a.GetTypes(); });
            types
                 .Where((type) => {
                     return !type.IsAbstract && type.IsSubclassOf(typeof(Framework)) &&
         type.IsDefined(typeof(FrameworkAttribute), false) &&
         (type.GetCustomAttributes(typeof(FrameworkAttribute), false).First() as FrameworkAttribute).env.HasFlag(Environment.Editor);
                 })
                 .Select((type) => {
                     Framework f = Activator.CreateInstance(type) as Framework;
                     f.env = Environment.Editor;
                     return f;
                 }).ToList()
                 .ForEach((f) => {
                     _container.Subscribe(f);
                 });

        }
        private static Framework GetFrame(string name)
        {
            return _container.Get(name);
        }
        public void Start()
        {
            Startup();
            _container.Startup();
        }
        public void Update()
        {
            _container.Update();
        }
        public void OnDisable()
        {
            _container.Dispose();
            _container = null;
        }
    }
}

