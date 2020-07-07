using System;

namespace MutiFramework
{
    /// <summary>
    /// 框架抽象
    /// </summary>
    public abstract class Framework : IDisposable
    {
        /// <summary>
        /// 当前环境
        /// </summary>
        public EnvironmentType env { get; set; }
        /// <summary>
        /// 框架名称
        /// </summary>
        public abstract string name { get; }
        /// <summary>
        /// 优先级，越大释放越早
        /// </summary>
        public abstract int priority { get; }
        /// <summary>
        /// 是否释放
        /// </summary>
        public bool disposed { get; private set; }
        /// <summary>
        /// 开启
        /// </summary>
        public void Startup()
        {
            disposed = false;
            OnStartup();
        }
        /// <summary>
        /// 开启时
        /// </summary>
        protected abstract void OnStartup();


        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            OnDispose();
        }
        /// <summary>
        /// 释放时
        /// </summary>
        protected abstract void OnDispose();

    }


}