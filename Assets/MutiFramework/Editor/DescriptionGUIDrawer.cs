#if MutiFramework
using UnityEditor;
using UnityEngine;

namespace MutiFramework
{
    /// <summary>
    /// 绘制栏目
    /// </summary>
    public abstract class DescriptionGUIDrawer
    {
        protected class Styles
        {
            public static GUIStyle header = "AM MixerHeader2";
            public static GUIStyle in_title = "IN Title";
            public static GUIStyle boldLabel = "BoldLabel";
            public static GUIStyle controlLabel = "ControlLabel";
        }
        protected class Contents
        {
            public static GUIContent help = EditorGUIUtility.IconContent("_Help");
        }

        /// <summary>
        /// 名字
        /// </summary>
        public abstract string name { get; }
        /// <summary>
        /// 代码版本
        /// </summary>
        public abstract string version { get; }
        /// <summary>
        /// 作者
        /// </summary>
        public abstract string author { get; }
        /// <summary>
        /// 描述
        /// </summary>
        public abstract string describtion { get; }
        /// <summary>
        /// 编辑器下的路径
        /// </summary>
        public abstract string assetPath { get; }
        /// <summary>
        /// 帮助链接
        /// </summary>
        public virtual string helpurl { get { return "https://www.baidu.com"; } }
        /// <summary>
        /// 依赖内容
        /// </summary>
        public virtual string[] dependences { get; }

        /// <summary>
        /// 所在位置
        /// </summary>
        protected Rect position { get; private set; }
        /// <summary>
        /// 所属窗体
        /// </summary>
        public EditorWindow window { get; set; }
        /// <summary>
        /// 输出提示
        /// </summary>
        /// <param name="message"></param>
        protected void ShowNotification(string message)
        {
            window.ShowNotification(new GUIContent(message));
        }

        /// <summary>
        /// 绘制
        /// </summary>
        /// <param name="rect"></param>
        public virtual void OnGUI(Rect rect)
        {
            this.position = rect;
        }
        /// <summary>
        /// 失去焦点
        /// </summary>
        public virtual void OnDisable()
        {

        }
        /// <summary>
        /// 得到焦点
        /// </summary>
        public virtual void OnEnable()
        {
        }
    }

}
#endif
