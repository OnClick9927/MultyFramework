using System;

namespace MutiFramework
{
    /// <summary>
    /// 框架标记
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class FrameworkAttribute : Attribute
    {
        public EnvironmentType env { get; private set; }
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="env">支持的环境</param>
        public FrameworkAttribute(EnvironmentType env)
        {
            this.env = env;
        }
    }


}

