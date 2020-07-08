using System.Collections.Generic;
using UnityEngine;
using System;

namespace MultyFramework
{
    /// <summary>
    /// 框架容器
    /// </summary>
    public class MultyFrameworkContaner : IDisposable
    {
        /// <summary>
        /// 框架集合
        /// </summary>
        private Dictionary<string, Framework> _frameworks;
        /// <summary>
        /// 每帧刷新的框架
        /// </summary>
        private List<UpdateFramework> _updates;

        /// <summary>
        /// ctor
        /// </summary>
        public MultyFrameworkContaner()
        {
            _frameworks = new Dictionary<string, Framework>();
            _updates = new List<UpdateFramework>();
        }
        /// <summary>
        /// 通过名字获取一个框架
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
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
        /// <summary>
        /// 释放容器
        /// </summary>
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
        /// <summary>
        /// 开启
        /// </summary>
        public void Startup()
        {
            foreach (var item in _frameworks.Values)
            {
                item.Startup();
            }
        }
        /// <summary>
        /// 刷新
        /// </summary>
        public void Update()
        {
            for (int i = 0; i < _updates.Count; i++)
            {
                var f = _updates[i];
                f.Update();
            }
        }
        /// <summary>
        /// 注册框架
        /// </summary>
        /// <param name="framework"></param>
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
        /// <summary>
        /// 取消注册通过框架名字
        /// </summary>
        /// <param name="name"></param>
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