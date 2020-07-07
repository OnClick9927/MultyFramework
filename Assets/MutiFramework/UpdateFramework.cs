namespace MutiFramework
{
    /// <summary>
    /// 需要不断刷新的框架
    /// </summary>
    public abstract class UpdateFramework : Framework
    {
        /// <summary>
        /// 每帧调用
        /// </summary>
        public void Update()
        {
            if (disposed) return;
            OnUpdate();
        }
        /// <summary>
        /// 刷新时
        /// </summary>
        protected abstract void OnUpdate();
    }
}