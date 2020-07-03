#if MutiFramework
using UnityEditor;
using UnityEngine;

namespace MutiFramework
{
    /// <summary>
    /// 绘制栏目
    /// </summary>
    public abstract class PanelGUIDrawer
    {
        protected class Styles
        {
            public static GUIStyle header = "AM MixerHeader2";
            public static GUIStyle in_title = "IN Title";
            public static GUIStyle boldLabel = "BoldLabel";
            public static GUIStyle controlLabel = "ControlLabel";
            public static GUIStyle in_LockButton = "IN LockButton";
            public static GUIStyle minus ="OL Minus";
            public static GUIStyle plus = "OL Plus";
            public static GUIStyle box = "box";
            public static GUIStyle buttonLeft = "ButtonLeft";
        }
        protected class Contents
        {
            public static GUIContent help = EditorGUIUtility.IconContent("_Help");
            public static GUIContent Go = new GUIContent("Go");
            public static GUIContent refresh = EditorGUIUtility.IconContent("Refresh");
            public const float searchTxtWith = 200;
            public const float gap = 10;
            public const int btnWith = 20;
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
        /// 帮助链接
        /// </summary>
        public virtual string helpurl { get { return MutiFrameworkEditorTool.baidu; } }
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
        public static MutiFrameworkWindow window { get; set; }
        /// <summary>
        /// 输出提示
        /// </summary>
        /// <param name="message"></param>
        protected static void ShowNotification(string message)
        {
            window.ShowNotification(new GUIContent(message));
        }
        /// <summary>
        /// 显示进度条
        /// </summary>
        /// <param name="title"></param>
        /// <param name="info"></param>
        /// <param name="progress"></param>
        protected static void DisplayProgressBar(string title, string info, float progress)
        {
            EditorUtility.DisplayProgressBar(title, info, progress);
        }
        /// <summary>
        /// 清理京都条
        /// </summary>
        protected static void ClearProgressBar()
        {
            EditorUtility.ClearProgressBar();
        }
        /// <summary>
        /// 显示可以取消进度条
        /// </summary>
        /// <param name="title"></param>
        /// <param name="info"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        protected static bool DisplayCancelableProgressBar(string title, string info, float progress)
        {
            return EditorUtility.DisplayCancelableProgressBar(title, info, progress);
        }
        /// <summary>
        /// 显示对话框
        /// </summary>
        /// <param name="title"></param>
        /// <param name="info"></param>
        /// <param name="ok"></param>
        /// <param name="cancel"></param>
        /// <returns></returns>
        protected static bool DisplayDialog(string title, string info, string ok="ok", string cancel="cancel")
        {
            return EditorUtility.DisplayDialog(title, info, ok, cancel);
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

        public virtual void Awake()
        {
        }

        public virtual void OnDestroy()
        {
        }
    }

}
#endif
