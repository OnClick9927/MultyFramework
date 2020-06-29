using UnityEngine;

namespace MutiFramework
{
    /// <summary>
    /// 绘制一条栏目
    /// </summary>
    public abstract class LineGUIDrawer
    {
        /// <summary>
        /// 名字
        /// </summary>
        public abstract string name { get; }
        /// <summary>
        /// 提示信息
        /// </summary>
        public virtual string tooltip { get; }
        public virtual void OnGUI()
        {
            GUILayout.Label(new GUIContent(name, tooltip), "boldlabel", GUILayout.Width(150));
         
        }
    }

}

