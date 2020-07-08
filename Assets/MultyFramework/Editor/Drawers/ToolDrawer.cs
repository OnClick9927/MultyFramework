using UnityEngine;

namespace MultyFramework
{
    /// <summary>
    /// 绘制工具栏目
    /// </summary>
    public abstract class ToolDrawer:PanelGUIDrawer
    {
        private bool _describtionFold = true;
        private bool _dependencesFold = true;
        private Vector2 _scroll;


        public override void OnGUI(Rect rect)
        {
            base.OnGUI(rect);
            GUILayout.BeginArea(rect);
            {
                _scroll = GUILayout.BeginScrollView(_scroll);
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(name, Styles.header);
                    if (GUILayout.Button(Contents.help, Styles.controlLabel))
                    {
                        UnityEditor.Help.BrowseURL(helpurl);
                    }
                    GUILayout.Space(Contents.gap);
                    GUILayout.Label(Application.unityVersion);
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                }

                GUILayout.Label("Version  " + version, Styles.boldLabel);
                GUILayout.Label("Author " + author, Styles.boldLabel);

                {
                    GUILayout.Label("Dependences", Styles.in_title);
                    Rect last = GUILayoutUtility.GetLastRect();
                    last.width -= Contents.gap;
                    last.xMin += Contents.gap;
                    if (Event.current.type == EventType.MouseUp && last.Contains(Event.current.mousePosition))
                    {
                        _dependencesFold = !_dependencesFold;
                        Event.current.Use();
                    }
                    last.xMin -= Contents.gap;
                    last.width = Contents.gap;

                    _dependencesFold = UnityEditor.EditorGUI.Foldout(last, _dependencesFold, "");
                    if (_dependencesFold)
                    {
                        if (dependences != null)
                        {
                            for (int i = 0; i < dependences.Length; i++)
                            {
                                GUILayout.Label(dependences[i]);
                            }
                        }
                    }
                }
                {
                    GUILayout.Label("Describtion ", Styles.in_title);
                    Rect last = GUILayoutUtility.GetLastRect();
                    last.width -= Contents.gap;
                    last.xMin += Contents.gap;
                    if (Event.current.type == EventType.MouseUp && last.Contains(Event.current.mousePosition))
                    {
                        _describtionFold = !_describtionFold;
                        Event.current.Use();
                    }
                    last.xMin -= Contents.gap;
                    last.width = Contents.gap;
                    _describtionFold = UnityEditor.EditorGUI.Foldout(last, _describtionFold, "");
                    if (_describtionFold)
                    {
                        GUILayout.Label(describtion);
                    }
                }
                GUILayout.Label("", Styles.in_title, GUILayout.Height(0));
                ToolGUI();
                GUILayout.EndScrollView();
            }
            GUILayout.EndArea();
        }

        /// <summary>
        /// 自身的界面
        /// </summary>
        protected abstract void ToolGUI();

    }

}

