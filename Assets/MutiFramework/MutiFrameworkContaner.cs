using System.Collections.Generic;
using UnityEngine;
using System;

namespace MutiFramework
{
    public class MutiFrameworkContaner : IDisposable
    {
        private Dictionary<string, Framework> _frameworks;
        private List<UpdateFramework> _updates;

        public MutiFrameworkContaner()
        {
            _frameworks = new Dictionary<string, Framework>();
            _updates = new List<UpdateFramework>();
        }
        public Framework Get(string name)
        {
            Framework f;
            if (_frameworks.TryGetValue(name, out f))
            {
                return f;
            }
            else
            {
                Debug.LogError(string.Format("The Framework with name :   {0}  Not Exist", name));
                return null;
            }
        }
        public void Dispose()
        {
            _updates.Clear();
            List<Framework> fs = new List<Framework>();
            foreach (var item in _frameworks.Values)
            {
                fs.Add(item);
            }
            _frameworks.Clear();
            fs.Sort((x, y) => { return y.priority.CompareTo(x.priority); });
            for (int i = 0; i < fs.Count; i++)
            {
                var f = fs[i];
                f.Dispose();
            }
        }
        public void Startup()
        {
            foreach (var item in _frameworks.Values)
            {
                item.Startup();
            }
        }
        public void Update()
        {
            for (int i = 0; i < _updates.Count; i++)
            {
                var f = _updates[i];
                f.Update();
            }
        }
        public void Subscribe(Framework framework)
        {
            Framework f;
            if (_frameworks.TryGetValue(framework.name, out f))
            {
                Debug.LogError(string.Format("The Framework with name :   {0}   Exist", framework.name));
            }
            else
            {
                _frameworks.Add(framework.name, framework);
                if (framework is UpdateFramework)
                {
                    _updates.Add(framework as UpdateFramework);
                }
            }
        }
        public void UnSubscribe(string name)
        {
            Framework f;
            if (_frameworks.TryGetValue(name, out f))
            {
                _frameworks.Remove(name);
                if (f is UpdateFramework)
                {
                    _updates.Remove(f as UpdateFramework);
                }
            }
            else
            {
                Debug.LogError(string.Format("The Framework with name :   {0}  Not Exist", name));
            }
        }
    }


}

