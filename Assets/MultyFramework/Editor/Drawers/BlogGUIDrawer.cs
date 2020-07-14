using UnityEditor;
using UnityEngine;
namespace MultyFramework
{
    public abstract class BlogGUIDrawer : ToolDrawer
    {
        public override string name { get { return author + "'s Blog"; } }
        public override string helpurl { get { return blogUrl; } }

        protected abstract string blogUrl { get; }
        private WebViewHook _webView;
        public override void Awake()
        {

        }
        public override void OnDestroy()
        {
            GameObject.DestroyImmediate(_webView);
        }
        public override void OnDisable()
        {
            if (_webView)
            {
                _webView.Detach();
            }
        }
        private void ReadMe(Rect rect)
        {
            if (_webView.Hook(window))
                _webView.LoadURL(helpurl);
            if (GUI.Button(new Rect(rect.x, rect.y, 25, 20), "<"))
                _webView.Back();
            if (GUI.Button(new Rect(rect.x + 25, rect.y, 25, 20), ">"))
                _webView.Forward();
            if (GUI.Button(new Rect(rect.x + 50, rect.y, 25, 20), new GUIContent(EditorGUIUtility.IconContent("refresh"))))
                _webView.Reload();
            if (GUI.Button(new Rect(rect.x + 75, rect.y, 25, 20), "→"))
                _webView.LoadURL(helpurl);
            if (GUI.Button(new Rect(rect.x + 100, rect.y, 25, 20), Contents.help,Styles.boldLabel))
                Help.BrowseURL(helpurl);
            GUI.SetNextControlName("urlfield");
            var ev = Event.current;
            if (ev.isKey && GUI.GetNameOfFocusedControl().Equals("urlfield"))
                if (ev.keyCode == KeyCode.Return)
                {
                    _webView.LoadURL(helpurl);
                    GUIUtility.keyboardControl = 0;
                    _webView.SetApplicationFocus(true);
                    ev.Use();
                }
            if (ev.type == EventType.Repaint)
            {
                _webView.OnGUI(rect.Zoom(AnchorType.LowerCenter, new Vector2(0, -20)));
            }
        }
        public override void OnEnable()
        {
            if (!_webView)
            {
                _webView = ScriptableObject.CreateInstance<WebViewHook>();
            }
        }
        protected override void ToolGUI()
        {
            var rect = GUILayoutUtility.GetLastRect();
            Rect r = new Rect(rect.x, rect.yMax, rect.width, window.position.height - rect.yMax - Contents.gap * 3);
            ReadMe(r);
        }
    }
}

