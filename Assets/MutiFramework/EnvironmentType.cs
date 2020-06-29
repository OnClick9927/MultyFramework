using System;

namespace MutiFramework
{
    /// <summary>
    /// 环境
    /// </summary>
    [Flags]
    public enum EnvironmentType
    {
        /// <summary>
        /// 运行时
        /// </summary>
        Runtime = 1,
        /// <summary>
        /// 编辑器模式
        /// </summary>
        Editor = 2
    }
}

